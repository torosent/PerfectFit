using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Gamification;
using PerfectFit.UseCases.Gamification.Queries;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Queries;

public class GetGamificationStatusQueryTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IStreakService> _streakServiceMock;
    private readonly Mock<IChallengeService> _challengeServiceMock;
    private readonly Mock<IAchievementService> _achievementServiceMock;
    private readonly Mock<ISeasonPassService> _seasonPassServiceMock;
    private readonly Mock<ICosmeticService> _cosmeticServiceMock;
    private readonly Mock<IPersonalGoalService> _personalGoalServiceMock;
    private readonly GetGamificationStatusQueryHandler _handler;

    public GetGamificationStatusQueryTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _streakServiceMock = new Mock<IStreakService>();
        _challengeServiceMock = new Mock<IChallengeService>();
        _achievementServiceMock = new Mock<IAchievementService>();
        _seasonPassServiceMock = new Mock<ISeasonPassService>();
        _cosmeticServiceMock = new Mock<ICosmeticService>();
        _personalGoalServiceMock = new Mock<IPersonalGoalService>();

        _handler = new GetGamificationStatusQueryHandler(
            _userRepositoryMock.Object,
            _streakServiceMock.Object,
            _challengeServiceMock.Object,
            _achievementServiceMock.Object,
            _seasonPassServiceMock.Object,
            _cosmeticServiceMock.Object,
            _personalGoalServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUser_ReturnsComprehensiveStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId, currentStreak: 5, longestStreak: 10, freezeTokens: 2);
        SetupMocksForSuccessfulQuery(user);

        var query = new GetGamificationStatusQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Streak.Should().NotBeNull();
        result.ActiveChallenges.Should().NotBeNull();
        result.RecentAchievements.Should().NotBeNull();
        // SeasonPass can be null if no active season exists
        result.EquippedCosmetics.Should().NotBeNull();
        result.ActiveGoals.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ReturnsCorrectStreakInfo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;
        var currentTime = DateTimeOffset.UtcNow;
        var resetTime = currentTime.AddHours(3);

        var user = CreateUser(userIntId, userId, currentStreak: 7, longestStreak: 15, freezeTokens: 3);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Setup other mocks first (with defaults)
        SetupOtherMocks(user);

        // Then override specific mocks for this test
        _streakServiceMock
            .Setup(x => x.IsStreakAtRisk(It.IsAny<User>(), It.IsAny<DateTimeOffset>()))
            .Returns(true);

        _streakServiceMock
            .Setup(x => x.GetStreakResetTime(It.IsAny<User>(), It.IsAny<DateTimeOffset>()))
            .Returns(resetTime);

        var query = new GetGamificationStatusQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Streak.CurrentStreak.Should().Be(7);
        result.Streak.LongestStreak.Should().Be(15);
        result.Streak.FreezeTokens.Should().Be(3);
        result.Streak.IsAtRisk.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsActiveChallenges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var challenges = new List<Challenge>
        {
            CreateChallenge(1, "Daily Challenge", ChallengeType.Daily, 100, 50),
            CreateChallenge(2, "Weekly Challenge", ChallengeType.Weekly, 500, 200)
        };
        var userChallenges = new List<UserChallenge>
        {
            CreateUserChallenge(userIntId, 1, currentProgress: 50),
            CreateUserChallenge(userIntId, 2, currentProgress: 200)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Setup other mocks first (with defaults)
        SetupOtherMocks(user);

        // Then override specific mocks for this test
        _challengeServiceMock
            .Setup(x => x.GetActiveChallengesAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenges);

        _challengeServiceMock
            .Setup(x => x.GetOrCreateUserChallengeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userChallenges[0]);

        var query = new GetGamificationStatusQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ActiveChallenges.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ReturnsRecentAchievements()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var userAchievements = new List<UserAchievement>
        {
            CreateUserAchievement(userIntId, 1, progress: 100, unlocked: true),
            CreateUserAchievement(userIntId, 2, progress: 50, unlocked: false)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Setup other mocks first (with defaults)
        SetupOtherMocks(user);

        // Then override specific mocks for this test
        _achievementServiceMock
            .Setup(x => x.GetUserAchievementsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAchievements);

        var query = new GetGamificationStatusQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.RecentAchievements.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetGamificationStatusQuery(userId);

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    private void SetupMocksForSuccessfulQuery(User user)
    {
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        SetupOtherMocks(user);
    }

    private void SetupOtherMocks(User user)
    {
        _streakServiceMock
            .Setup(x => x.IsStreakAtRisk(It.IsAny<User>(), It.IsAny<DateTimeOffset>()))
            .Returns(false);

        _streakServiceMock
            .Setup(x => x.GetStreakResetTime(It.IsAny<User>(), It.IsAny<DateTimeOffset>()))
            .Returns(DateTimeOffset.UtcNow.AddHours(5));

        _challengeServiceMock
            .Setup(x => x.GetActiveChallengesAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge>());

        _achievementServiceMock
            .Setup(x => x.GetUserAchievementsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserAchievement>());

        _seasonPassServiceMock
            .Setup(x => x.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Season?)null);

        _cosmeticServiceMock
            .Setup(x => x.GetAllCosmeticsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Cosmetic>());

        _personalGoalServiceMock
            .Setup(x => x.GetActiveGoalsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PersonalGoal>());
    }

    private static User CreateUser(int id, Guid externalGuid, int currentStreak = 0, int longestStreak = 0, int freezeTokens = 0)
    {
        var user = User.Create(externalGuid.ToString(), "test@test.com", "TestUser", AuthProvider.Local);
        SetProperty(user, "Id", id);
        SetProperty(user, "CurrentStreak", currentStreak);
        SetProperty(user, "LongestStreak", longestStreak);
        SetProperty(user, "StreakFreezeTokens", freezeTokens);
        return user;
    }

    private static Challenge CreateChallenge(int id, string name, ChallengeType type, int targetValue, int xpReward)
    {
        var challenge = Challenge.Create(name, "Test", type, targetValue, xpReward, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));
        SetProperty(challenge, "Id", id);
        return challenge;
    }

    private static UserChallenge CreateUserChallenge(int userId, int challengeId, int currentProgress = 0)
    {
        var userChallenge = UserChallenge.Create(userId, challengeId);
        SetProperty(userChallenge, "CurrentProgress", currentProgress);
        return userChallenge;
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
