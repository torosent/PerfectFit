using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;
using System.Reflection;

namespace PerfectFit.UnitTests.Services;

public class AchievementServiceTests
{
    private readonly Mock<IGamificationRepository> _repositoryMock;
    private readonly Mock<ICosmeticService> _cosmeticServiceMock;
    private readonly AchievementService _service;

    public AchievementServiceTests()
    {
        _repositoryMock = new Mock<IGamificationRepository>();
        _cosmeticServiceMock = new Mock<ICosmeticService>();
        _service = new AchievementService(_repositoryMock.Object, _cosmeticServiceMock.Object);
    }

    #region CheckAndUnlockAchievementsAsync Tests

    [Fact]
    public async Task CheckAndUnlock_ScoreAchievement_UnlocksWhenMet()
    {
        // Arrange
        var user = CreateUser(highScore: 1000);
        var achievement = CreateAchievement(
            id: 1,
            category: AchievementCategory.Score,
            unlockCondition: """{"type":"score","value":500}""",
            rewardType: RewardType.XPBoost,
            rewardValue: 100);

        var achievements = new List<Achievement> { achievement };
        var userAchievements = new List<UserAchievement>();

        _repositoryMock.Setup(r => r.GetAllAchievementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);
        _repositoryMock.Setup(r => r.GetUserAchievementsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAchievements);

        // Act
        var result = await _service.CheckAndUnlockAchievementsAsync(user);

        // Assert
        result.UnlockedAchievements.Should().HaveCount(1);
        result.UnlockedAchievements[0].Id.Should().Be(1);
        result.TotalRewardsGranted.Should().Be(100);
        _repositoryMock.Verify(r => r.AddUserAchievementAsync(It.IsAny<UserAchievement>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckAndUnlock_StreakAchievement_UnlocksWhenMet()
    {
        // Arrange
        var user = CreateUser(currentStreak: 7, longestStreak: 10);
        var achievement = CreateAchievement(
            id: 1,
            category: AchievementCategory.Streak,
            unlockCondition: """{"type":"streak","value":7}""",
            rewardType: RewardType.StreakFreeze,
            rewardValue: 1);

        var achievements = new List<Achievement> { achievement };
        var userAchievements = new List<UserAchievement>();

        _repositoryMock.Setup(r => r.GetAllAchievementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);
        _repositoryMock.Setup(r => r.GetUserAchievementsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAchievements);

        // Act
        var result = await _service.CheckAndUnlockAchievementsAsync(user);

        // Assert
        result.UnlockedAchievements.Should().HaveCount(1);
        result.TotalRewardsGranted.Should().Be(1);
    }

    [Fact]
    public async Task CheckAndUnlock_GamesAchievement_UnlocksWhenMet()
    {
        // Arrange
        var user = CreateUser(gamesPlayed: 100);
        var achievement = CreateAchievement(
            id: 1,
            category: AchievementCategory.Games,
            unlockCondition: """{"type":"games","value":50}""",
            rewardType: RewardType.XPBoost,
            rewardValue: 50);

        var achievements = new List<Achievement> { achievement };
        var userAchievements = new List<UserAchievement>();

        _repositoryMock.Setup(r => r.GetAllAchievementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);
        _repositoryMock.Setup(r => r.GetUserAchievementsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAchievements);

        // Act
        var result = await _service.CheckAndUnlockAchievementsAsync(user);

        // Assert
        result.UnlockedAchievements.Should().HaveCount(1);
    }

    [Fact]
    public async Task CheckAndUnlock_AlreadyUnlocked_SkipsRecheck()
    {
        // Arrange
        var user = CreateUser(highScore: 1000);
        var achievement = CreateAchievement(
            id: 1,
            category: AchievementCategory.Score,
            unlockCondition: """{"type":"score","value":500}""");

        var existingUserAchievement = UserAchievement.Create(user.Id, achievement.Id);
        existingUserAchievement.Unlock(); // Already unlocked

        var achievements = new List<Achievement> { achievement };
        var userAchievements = new List<UserAchievement> { existingUserAchievement };

        _repositoryMock.Setup(r => r.GetAllAchievementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);
        _repositoryMock.Setup(r => r.GetUserAchievementsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAchievements);

        // Act
        var result = await _service.CheckAndUnlockAchievementsAsync(user);

        // Assert
        result.UnlockedAchievements.Should().BeEmpty();
        _repositoryMock.Verify(r => r.AddUserAchievementAsync(It.IsAny<UserAchievement>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckAndUnlock_MultipleAchievements_UnlocksAll()
    {
        // Arrange
        var user = CreateUser(highScore: 1000, gamesPlayed: 100, currentStreak: 10);
        var achievements = new List<Achievement>
        {
            CreateAchievement(1, AchievementCategory.Score, """{"type":"score","value":500}""", RewardType.XPBoost, 50),
            CreateAchievement(2, AchievementCategory.Games, """{"type":"games","value":50}""", RewardType.XPBoost, 25),
            CreateAchievement(3, AchievementCategory.Streak, """{"type":"streak","value":7}""", RewardType.StreakFreeze, 1),
        };

        _repositoryMock.Setup(r => r.GetAllAchievementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);
        _repositoryMock.Setup(r => r.GetUserAchievementsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserAchievement>());

        // Act
        var result = await _service.CheckAndUnlockAchievementsAsync(user);

        // Assert
        result.UnlockedAchievements.Should().HaveCount(3);
        result.TotalRewardsGranted.Should().Be(76); // 50 + 25 + 1
    }

    [Fact]
    public async Task CheckAndUnlock_ChallengeAchievement_UnlocksBasedOnCompletedChallenges()
    {
        // Arrange
        var user = CreateUser();
        var achievement = CreateAchievement(
            id: 1,
            category: AchievementCategory.Challenge,
            unlockCondition: """{"type":"challenges","value":10}""",
            rewardType: RewardType.XPBoost,
            rewardValue: 100);

        _repositoryMock.Setup(r => r.GetAllAchievementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Achievement> { achievement });
        _repositoryMock.Setup(r => r.GetUserAchievementsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserAchievement>());
        _repositoryMock.Setup(r => r.GetCompletedChallengeCountAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(15);

        // Act
        var result = await _service.CheckAndUnlockAchievementsAsync(user);

        // Assert
        result.UnlockedAchievements.Should().HaveCount(1);
    }

    #endregion

    #region CalculateProgressAsync Tests

    [Fact]
    public async Task CalculateProgress_ReturnsCorrectPercentage()
    {
        // Arrange
        var user = CreateUser(highScore: 250);
        var achievement = CreateAchievement(
            id: 1,
            category: AchievementCategory.Score,
            unlockCondition: """{"type":"score","value":500}""");

        // Act
        var progress = await _service.CalculateProgressAsync(user, achievement);

        // Assert
        progress.Should().Be(50); // 250/500 * 100 = 50%
    }

    [Fact]
    public async Task CalculateProgress_CapsAt100()
    {
        // Arrange
        var user = CreateUser(highScore: 1000);
        var achievement = CreateAchievement(
            id: 1,
            category: AchievementCategory.Score,
            unlockCondition: """{"type":"score","value":500}""");

        // Act
        var progress = await _service.CalculateProgressAsync(user, achievement);

        // Assert
        progress.Should().Be(100);
    }

    [Fact]
    public async Task CalculateProgress_StreakAchievement_UsesCurrentStreak()
    {
        // Arrange
        var user = CreateUser(currentStreak: 3, longestStreak: 5);
        var achievement = CreateAchievement(
            id: 1,
            category: AchievementCategory.Streak,
            unlockCondition: """{"type":"streak","value":10}""");

        // Act
        var progress = await _service.CalculateProgressAsync(user, achievement);

        // Assert
        progress.Should().Be(30); // 3/10 * 100 = 30% (uses current, not longest)
    }

    #endregion

    #region GetAllAchievementsAsync Tests

    [Fact]
    public async Task GetAllAchievements_ReturnsAllAchievements()
    {
        // Arrange
        var achievements = new List<Achievement>
        {
            CreateAchievement(1, AchievementCategory.Score),
            CreateAchievement(2, AchievementCategory.Streak),
            CreateAchievement(3, AchievementCategory.Games),
        };

        _repositoryMock.Setup(r => r.GetAllAchievementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);

        // Act
        var result = await _service.GetAllAchievementsAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region Helper Methods

    private static User CreateUser(
        int id = 1,
        int highScore = 0,
        int gamesPlayed = 0,
        int currentStreak = 0,
        int longestStreak = 0)
    {
        var user = User.Create("ext_123", "test@test.com", "TestUser", AuthProvider.Local);

        SetProperty(user, "Id", id);
        SetProperty(user, "HighScore", highScore);
        SetProperty(user, "GamesPlayed", gamesPlayed);
        SetProperty(user, "CurrentStreak", currentStreak);
        SetProperty(user, "LongestStreak", longestStreak);

        return user;
    }

    private static Achievement CreateAchievement(
        int id,
        AchievementCategory category,
        string unlockCondition = """{"type":"score","value":100}""",
        RewardType rewardType = RewardType.XPBoost,
        int rewardValue = 10)
    {
        var achievement = Achievement.Create(
            $"Achievement {id}",
            $"Description {id}",
            category,
            "/icons/achievement.png",
            unlockCondition,
            rewardType,
            rewardValue);

        SetProperty(achievement, "Id", id);

        return achievement;
    }

    private static void SetProperty<T, TValue>(T obj, string propertyName, TValue value) where T : class
    {
        var backingField = typeof(T).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(obj, value);
    }

    #endregion
}
