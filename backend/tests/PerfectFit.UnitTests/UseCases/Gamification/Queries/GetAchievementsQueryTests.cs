using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Gamification;
using PerfectFit.UseCases.Gamification.Queries;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Queries;

public class GetAchievementsQueryTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAchievementService> _achievementServiceMock;
    private readonly GetAchievementsQueryHandler _handler;

    public GetAchievementsQueryTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _achievementServiceMock = new Mock<IAchievementService>();
        _handler = new GetAchievementsQueryHandler(_userRepositoryMock.Object, _achievementServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAllAchievementsWithProgress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var achievements = new List<Achievement>
        {
            CreateAchievement(1, "First Win", AchievementCategory.Score),
            CreateAchievement(2, "Combo Master", AchievementCategory.Score),
            CreateAchievement(3, "Streak Champion", AchievementCategory.Streak)
        };
        var userAchievements = new List<UserAchievement>
        {
            CreateUserAchievement(userIntId, 1, progress: 100, unlocked: true),
            CreateUserAchievement(userIntId, 2, progress: 50, unlocked: false),
            CreateUserAchievement(userIntId, 3, progress: 25, unlocked: false)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _achievementServiceMock
            .Setup(x => x.GetAllAchievementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);

        _achievementServiceMock
            .Setup(x => x.GetUserAchievementsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAchievements);

        var query = new GetAchievementsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Achievements.Should().HaveCount(3);
        result.TotalAchievements.Should().Be(3);
        result.TotalUnlocked.Should().Be(1);
    }

    [Fact]
    public async Task Handle_UnlockedAchievements_HaveCorrectStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var achievements = new List<Achievement>
        {
            CreateAchievement(1, "First Win", AchievementCategory.Score)
        };
        var userAchievements = new List<UserAchievement>
        {
            CreateUserAchievement(userIntId, 1, progress: 100, unlocked: true)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _achievementServiceMock
            .Setup(x => x.GetAllAchievementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);

        _achievementServiceMock
            .Setup(x => x.GetUserAchievementsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAchievements);

        var query = new GetAchievementsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Achievements[0].IsUnlocked.Should().BeTrue();
        result.Achievements[0].Progress.Should().Be(100);
        result.Achievements[0].UnlockedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_SecretAchievements_IncludedWithFlag()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var achievements = new List<Achievement>
        {
            CreateAchievement(1, "Hidden Achievement", AchievementCategory.Special, isSecret: true)
        };
        var userAchievements = new List<UserAchievement>
        {
            CreateUserAchievement(userIntId, 1, progress: 0, unlocked: false)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _achievementServiceMock
            .Setup(x => x.GetAllAchievementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);

        _achievementServiceMock
            .Setup(x => x.GetUserAchievementsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAchievements);

        var query = new GetAchievementsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Achievements[0].IsSecret.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetAchievementsQuery(userId);

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_NoAchievements_ReturnsEmptyResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _achievementServiceMock
            .Setup(x => x.GetAllAchievementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Achievement>());

        _achievementServiceMock
            .Setup(x => x.GetUserAchievementsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserAchievement>());

        var query = new GetAchievementsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Achievements.Should().BeEmpty();
        result.TotalAchievements.Should().Be(0);
        result.TotalUnlocked.Should().Be(0);
    }

    private static User CreateUser(int id, Guid externalGuid)
    {
        var user = User.Create(externalGuid.ToString(), "test@test.com", "TestUser", AuthProvider.Local);
        SetProperty(user, "Id", id);
        return user;
    }

    private static Achievement CreateAchievement(int id, string name, AchievementCategory category, bool isSecret = false)
    {
        var achievement = Achievement.Create(name, "Test", category, "/icon.png", "condition", RewardType.XPBoost, 100, isSecret);
        SetProperty(achievement, "Id", id);
        return achievement;
    }

    private static UserAchievement CreateUserAchievement(int userId, int achievementId, int progress, bool unlocked)
    {
        var userAchievement = UserAchievement.Create(userId, achievementId);
        SetProperty(userAchievement, "Progress", progress);
        if (unlocked)
        {
            userAchievement.Unlock();
        }
        return userAchievement;
    }

    private static void SetProperty<TEntity, TValue>(TEntity entity, string propertyName, TValue value) where TEntity : class
    {
        var backingField = typeof(TEntity).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(entity, value);
    }
}
