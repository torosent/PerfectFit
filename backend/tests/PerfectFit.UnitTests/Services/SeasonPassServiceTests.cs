using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;
using System.Reflection;

namespace PerfectFit.UnitTests.Services;

public class SeasonPassServiceTests
{
    private readonly Mock<IGamificationRepository> _repositoryMock;
    private readonly Mock<ICosmeticService> _cosmeticServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly SeasonPassService _service;

    public SeasonPassServiceTests()
    {
        _repositoryMock = new Mock<IGamificationRepository>();
        _cosmeticServiceMock = new Mock<ICosmeticService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _service = new SeasonPassService(_repositoryMock.Object, _cosmeticServiceMock.Object, _userRepositoryMock.Object);
    }

    #region GetCurrentSeasonAsync Tests

    [Fact]
    public async Task GetCurrentSeason_ReturnsActiveSeason()
    {
        // Arrange
        var season = CreateSeason(1, "Winter 2025", isActive: true);

        _repositoryMock.Setup(r => r.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(season);

        // Act
        var result = await _service.GetCurrentSeasonAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Winter 2025");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentSeason_NoActiveSeason_ReturnsNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Season?)null);

        // Act
        var result = await _service.GetCurrentSeasonAsync();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddXPAsync Tests

    [Fact]
    public async Task AddXP_IncrementsCorrectly()
    {
        // Arrange
        var user = CreateUser(seasonXP: 50, currentTier: 0);

        // Act
        var result = await _service.AddXPAsync(user, 30, "game_completion");

        // Assert
        result.Success.Should().BeTrue();
        result.NewXP.Should().Be(80);
        result.TierUp.Should().BeFalse();
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddXP_TriggersTierUp()
    {
        // Arrange
        var user = CreateUser(seasonXP: 90, currentTier: 0);

        _repositoryMock.Setup(r => r.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSeason(1, "Test Season"));
        _repositoryMock.Setup(r => r.GetSeasonRewardsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SeasonReward>
            {
                CreateSeasonReward(1, 1, 1, RewardType.Cosmetic, 1, 100)
            });
        _repositoryMock.Setup(r => r.GetClaimedRewardIdsAsync(user.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());

        // Act
        var result = await _service.AddXPAsync(user, 20, "challenge");

        // Assert
        result.Success.Should().BeTrue();
        result.NewXP.Should().Be(110);
        result.NewTier.Should().Be(1);
        result.TierUp.Should().BeTrue();
        result.RewardsAvailable.Should().Be(1);
    }

    [Fact]
    public async Task AddXP_MultipleTierUps()
    {
        // Arrange
        var user = CreateUser(seasonXP: 0, currentTier: 0);

        _repositoryMock.Setup(r => r.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSeason(1, "Test Season"));
        _repositoryMock.Setup(r => r.GetSeasonRewardsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SeasonReward>
            {
                CreateSeasonReward(1, 1, 1, RewardType.Cosmetic, 1, 100),
                CreateSeasonReward(2, 1, 2, RewardType.StreakFreeze, 1, 250),
            });
        _repositoryMock.Setup(r => r.GetClaimedRewardIdsAsync(user.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());

        // Act
        var result = await _service.AddXPAsync(user, 300, "bulk_reward");

        // Assert
        result.Success.Should().BeTrue();
        result.NewXP.Should().Be(300);
        result.NewTier.Should().Be(2);
        result.TierUp.Should().BeTrue();
        result.RewardsAvailable.Should().Be(2);
    }

    #endregion

    #region CalculateTierFromXP Tests

    [Theory]
    [InlineData(0, 0)]
    [InlineData(50, 0)]
    [InlineData(99, 0)]
    [InlineData(100, 1)]
    [InlineData(249, 1)]
    [InlineData(250, 2)]
    [InlineData(499, 2)]
    [InlineData(500, 3)]
    [InlineData(799, 3)]
    [InlineData(800, 4)]
    [InlineData(1199, 4)]
    [InlineData(1200, 5)]
    [InlineData(1699, 5)]
    [InlineData(1700, 6)]
    [InlineData(2299, 6)]
    [InlineData(2300, 7)]
    [InlineData(2999, 7)]
    [InlineData(3000, 8)]
    [InlineData(3999, 8)]
    [InlineData(4000, 9)]
    [InlineData(4999, 9)]
    [InlineData(5000, 10)]
    [InlineData(10000, 10)]
    public void CalculateTierFromXP_CorrectTiers(int xp, int expectedTier)
    {
        // Act
        var tier = _service.CalculateTierFromXP(xp);

        // Assert
        tier.Should().Be(expectedTier);
    }

    #endregion

    #region ClaimRewardAsync Tests

    [Fact]
    public async Task ClaimReward_Success_GrantsCosmetic()
    {
        // Arrange
        var user = CreateUser(seasonXP: 150, currentTier: 1);
        var reward = CreateSeasonReward(1, 1, 1, RewardType.Cosmetic, 42, 100);

        _repositoryMock.Setup(r => r.GetSeasonRewardByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reward);
        _repositoryMock.Setup(r => r.GetClaimedRewardIdsAsync(user.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());
        _repositoryMock.Setup(r => r.TryAddClaimedRewardAsync(user.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _cosmeticServiceMock.Setup(c => c.GrantCosmeticAsync(user, 42, ObtainedFrom.SeasonPass, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ClaimRewardAsync(user, 1);

        // Assert
        result.Success.Should().BeTrue();
        result.RewardType.Should().Be(RewardType.Cosmetic);
        result.RewardValue.Should().Be(42);
        _repositoryMock.Verify(r => r.TryAddClaimedRewardAsync(user.Id, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClaimReward_Success_GrantsStreakFreeze()
    {
        // Arrange
        var user = CreateUser(seasonXP: 300, currentTier: 2);
        var reward = CreateSeasonReward(2, 1, 2, RewardType.StreakFreeze, 2, 250);

        _repositoryMock.Setup(r => r.GetSeasonRewardByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reward);
        _repositoryMock.Setup(r => r.GetClaimedRewardIdsAsync(user.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());

        // Act
        var result = await _service.ClaimRewardAsync(user, 2);

        // Assert
        result.Success.Should().BeTrue();
        result.RewardType.Should().Be(RewardType.StreakFreeze);
        result.RewardValue.Should().Be(2);
    }

    [Fact]
    public async Task ClaimReward_NotReached_Fails()
    {
        // Arrange
        var user = CreateUser(seasonXP: 50, currentTier: 0);
        var reward = CreateSeasonReward(1, 1, 1, RewardType.Cosmetic, 42, 100);

        _repositoryMock.Setup(r => r.GetSeasonRewardByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reward);

        // Act
        var result = await _service.ClaimRewardAsync(user, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("tier");
    }

    [Fact]
    public async Task ClaimReward_AlreadyClaimed_Fails()
    {
        // Arrange
        var user = CreateUser(seasonXP: 150, currentTier: 1);
        var reward = CreateSeasonReward(1, 1, 1, RewardType.Cosmetic, 42, 100);

        _repositoryMock.Setup(r => r.GetSeasonRewardByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reward);
        _repositoryMock.Setup(r => r.GetClaimedRewardIdsAsync(user.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1 }); // Already claimed

        // Act
        var result = await _service.ClaimRewardAsync(user, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already been claimed");
    }

    [Fact]
    public async Task ClaimReward_RewardNotFound_Fails()
    {
        // Arrange
        var user = CreateUser(seasonXP: 150, currentTier: 1);

        _repositoryMock.Setup(r => r.GetSeasonRewardByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SeasonReward?)null);

        // Act
        var result = await _service.ClaimRewardAsync(user, 999);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task ClaimReward_CosmeticGrantFails_ReturnsFailure()
    {
        // Arrange
        var user = CreateUser(seasonXP: 150, currentTier: 1);
        var reward = CreateSeasonReward(1, 1, 1, RewardType.Cosmetic, 42, 100);

        _repositoryMock.Setup(r => r.GetSeasonRewardByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reward);
        _repositoryMock.Setup(r => r.GetClaimedRewardIdsAsync(user.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());
        
        // Cosmetic grant fails (returns false)
        _cosmeticServiceMock.Setup(c => c.GrantCosmeticAsync(user, 42, ObtainedFrom.SeasonPass, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ClaimRewardAsync(user, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to grant cosmetic");
        
        // Reward should not be marked as claimed
        _repositoryMock.Verify(r => r.AddClaimedRewardAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    private static User CreateUser(
        int id = 1,
        int seasonXP = 0,
        int currentTier = 0,
        int freezeTokens = 0)
    {
        var user = User.Create("ext_123", "test@test.com", "TestUser", AuthProvider.Local);

        SetProperty(user, "Id", id);
        SetProperty(user, "SeasonPassXP", seasonXP);
        SetProperty(user, "CurrentSeasonTier", currentTier);
        SetProperty(user, "StreakFreezeTokens", freezeTokens);

        return user;
    }

    private static Season CreateSeason(int id, string name, bool isActive = true)
    {
        var season = Season.Create(name, 1, "Default Theme", DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(60));

        SetProperty(season, "Id", id);
        if (!isActive) season.Deactivate();

        return season;
    }

    private static SeasonReward CreateSeasonReward(
        int id,
        int seasonId,
        int tier,
        RewardType rewardType,
        int rewardValue,
        int xpRequired)
    {
        var reward = SeasonReward.Create(seasonId, tier, rewardType, rewardValue, xpRequired);
        SetProperty(reward, "Id", id);
        return reward;
    }

    private static void SetProperty<T, TValue>(T obj, string propertyName, TValue value) where T : class
    {
        var backingField = typeof(T).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(obj, value);
    }

    #endregion
}
