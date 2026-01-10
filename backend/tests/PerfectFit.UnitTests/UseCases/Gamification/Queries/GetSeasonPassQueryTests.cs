using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Gamification;
using PerfectFit.UseCases.Gamification.Queries;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Queries;

public class GetSeasonPassQueryTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ISeasonPassService> _seasonPassServiceMock;
    private readonly GetSeasonPassQueryHandler _handler;

    public GetSeasonPassQueryTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _seasonPassServiceMock = new Mock<ISeasonPassService>();
        _handler = new GetSeasonPassQueryHandler(_userRepositoryMock.Object, _seasonPassServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ActiveSeason_ReturnsSeasonPassInfo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId, seasonXP: 500, tier: 3);
        var season = CreateSeason(1, "Winter Season", 1);
        var rewards = new List<SeasonReward>
        {
            CreateSeasonReward(1, 1, 1, RewardType.StreakFreeze, 1, 100),
            CreateSeasonReward(2, 1, 2, RewardType.Cosmetic, 1, 250),
            CreateSeasonReward(3, 1, 3, RewardType.StreakFreeze, 2, 500)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _seasonPassServiceMock
            .Setup(x => x.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(season);

        _seasonPassServiceMock
            .Setup(x => x.GetSeasonRewardsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rewards);

        var query = new GetSeasonPassQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.HasActiveSeason.Should().BeTrue();
        result.SeasonPass.Should().NotBeNull();
        result.SeasonPass!.CurrentXP.Should().Be(500);
        result.SeasonPass.CurrentTier.Should().Be(3);
        result.SeasonPass.Rewards.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_NoActiveSeason_ReturnsNoSeasonInfo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _seasonPassServiceMock
            .Setup(x => x.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Season?)null);

        var query = new GetSeasonPassQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.HasActiveSeason.Should().BeFalse();
        result.SeasonPass.Should().BeNull();
    }

    [Fact]
    public async Task Handle_RewardsIncludeClaimStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId, seasonXP: 300, tier: 2);
        var season = CreateSeason(1, "Winter Season", 1);
        var rewards = new List<SeasonReward>
        {
            CreateSeasonReward(1, 1, 1, RewardType.StreakFreeze, 1, 100), // Can claim (tier 2 >= tier 1)
            CreateSeasonReward(2, 1, 2, RewardType.Cosmetic, 1, 250),    // Can claim
            CreateSeasonReward(3, 1, 3, RewardType.StreakFreeze, 2, 500)  // Cannot claim (tier 2 < tier 3)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _seasonPassServiceMock
            .Setup(x => x.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(season);

        _seasonPassServiceMock
            .Setup(x => x.GetSeasonRewardsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rewards);

        var query = new GetSeasonPassQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.SeasonPass!.Rewards.Should().HaveCount(3);
        result.SeasonPass.Rewards[0].CanClaim.Should().BeTrue();
        result.SeasonPass.Rewards[1].CanClaim.Should().BeTrue();
        result.SeasonPass.Rewards[2].CanClaim.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetSeasonPassQuery(userId);

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_SeasonWithNoRewards_ReturnsEmptyRewardsList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var season = CreateSeason(1, "Empty Season", 1);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _seasonPassServiceMock
            .Setup(x => x.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(season);

        _seasonPassServiceMock
            .Setup(x => x.GetSeasonRewardsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SeasonReward>());

        var query = new GetSeasonPassQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.HasActiveSeason.Should().BeTrue();
        result.SeasonPass!.Rewards.Should().BeEmpty();
    }

    private static User CreateUser(int id, Guid externalGuid, int seasonXP = 0, int tier = 0)
    {
        var user = User.Create(externalGuid.ToString(), "test@test.com", "TestUser", AuthProvider.Local);
        SetProperty(user, "Id", id);
        SetProperty(user, "SeasonPassXP", seasonXP);
        SetProperty(user, "CurrentSeasonTier", tier);
        return user;
    }

    private static Season CreateSeason(int id, string name, int number)
    {
        var season = Season.Create(name, number, "Theme", DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(60));
        SetProperty(season, "Id", id);
        return season;
    }

    private static SeasonReward CreateSeasonReward(int id, int seasonId, int tier, RewardType rewardType, int rewardValue, int xpRequired)
    {
        var reward = SeasonReward.Create(seasonId, tier, rewardType, rewardValue, xpRequired);
        SetProperty(reward, "Id", id);
        return reward;
    }

    private static void SetProperty<TEntity, TValue>(TEntity entity, string propertyName, TValue value) where TEntity : class
    {
        var backingField = typeof(TEntity).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(entity, value);
    }
}
