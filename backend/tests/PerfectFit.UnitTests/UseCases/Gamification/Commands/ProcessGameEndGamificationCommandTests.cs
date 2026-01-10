using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;
using PerfectFit.UseCases.Gamification;
using PerfectFit.UseCases.Gamification.Commands;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Commands;

public class ProcessGameEndGamificationCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IGameSessionRepository> _gameSessionRepositoryMock;
    private readonly Mock<IStreakService> _streakServiceMock;
    private readonly Mock<IChallengeService> _challengeServiceMock;
    private readonly Mock<IAchievementService> _achievementServiceMock;
    private readonly Mock<ISeasonPassService> _seasonPassServiceMock;
    private readonly Mock<IPersonalGoalService> _personalGoalServiceMock;
    private readonly ProcessGameEndGamificationCommandHandler _handler;

    public ProcessGameEndGamificationCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _gameSessionRepositoryMock = new Mock<IGameSessionRepository>();
        _streakServiceMock = new Mock<IStreakService>();
        _challengeServiceMock = new Mock<IChallengeService>();
        _achievementServiceMock = new Mock<IAchievementService>();
        _seasonPassServiceMock = new Mock<ISeasonPassService>();
        _personalGoalServiceMock = new Mock<IPersonalGoalService>();

        _handler = new ProcessGameEndGamificationCommandHandler(
            _userRepositoryMock.Object,
            _gameSessionRepositoryMock.Object,
            _streakServiceMock.Object,
            _challengeServiceMock.Object,
            _achievementServiceMock.Object,
            _seasonPassServiceMock.Object,
            _personalGoalServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidGame_ProcessesAllGamification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var gameSession = CreateGameSession(gameSessionId, userIntId, score: 1000, linesCleared: 5);

        SetupMocksForSuccessfulProcessing(user, gameSession);

        var command = new ProcessGameEndGamificationCommand(userId, gameSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Streak.Should().NotBeNull();
        result.ChallengeUpdates.Should().NotBeNull();
        result.AchievementUpdates.Should().NotBeNull();
        result.SeasonProgress.Should().NotBeNull();
        result.GoalUpdates.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_UpdatesStreak_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var gameSession = CreateGameSession(gameSessionId, userIntId);
        var expectedStreakResult = new StreakResult(true, 5, 10, false, false);

        SetupMocksForSuccessfulProcessing(user, gameSession, streakResult: expectedStreakResult);

        var command = new ProcessGameEndGamificationCommand(userId, gameSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Streak.Should().Be(expectedStreakResult);
        _streakServiceMock.Verify(x => x.UpdateStreakAsync(user, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ChecksAchievements_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var gameSession = CreateGameSession(gameSessionId, userIntId);
        var unlockedAchievement = CreateAchievement(1, "First Win");
        var expectedAchievementResult = new AchievementUnlockResult(new List<Achievement> { unlockedAchievement }, 100);

        SetupMocksForSuccessfulProcessing(user, gameSession, achievementResult: expectedAchievementResult);

        var command = new ProcessGameEndGamificationCommand(userId, gameSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AchievementUpdates.UnlockedAchievements.Should().HaveCount(1);
        result.AchievementUpdates.TotalRewardsGranted.Should().Be(100);
        _achievementServiceMock.Verify(x => x.CheckAndUnlockAchievementsAsync(user, gameSession, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AddsSeasonXP_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var gameSession = CreateGameSession(gameSessionId, userIntId);
        var expectedSeasonResult = new SeasonXPResult(true, 150, 2, true, 1);

        SetupMocksForSuccessfulProcessing(user, gameSession, seasonXPResult: expectedSeasonResult);

        var command = new ProcessGameEndGamificationCommand(userId, gameSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.SeasonProgress.TierUp.Should().BeTrue();
        result.SeasonProgress.NewTier.Should().Be(2);
        _seasonPassServiceMock.Verify(x => x.AddXPAsync(user, It.IsAny<int>(), "game_completion", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new ProcessGameEndGamificationCommand(userId, gameSessionId);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_GameSessionNotFound_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _gameSessionRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameSession?)null);

        var command = new ProcessGameEndGamificationCommand(userId, gameSessionId);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_GameSessionBelongsToDifferentUser_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();
        var userIntId = 1;
        var otherUserIntId = 2;

        var user = CreateUser(userIntId, userId);
        var gameSession = CreateGameSession(gameSessionId, otherUserIntId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _gameSessionRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameSession);

        var command = new ProcessGameEndGamificationCommand(userId, gameSessionId);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*does not belong*");
    }

    [Fact]
    public async Task Handle_UpdatesPersonalGoals_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var gameSession = CreateGameSession(gameSessionId, userIntId);
        var personalGoals = new List<PersonalGoal> { CreatePersonalGoal(1, userIntId) };
        var expectedGoalResult = new GoalProgressResult(true, 50, false);

        SetupMocksForSuccessfulProcessing(user, gameSession);

        // Override the default setup with the specific mock after SetupMocksForSuccessfulProcessing
        _personalGoalServiceMock
            .Setup(x => x.GetActiveGoalsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(personalGoals);

        _personalGoalServiceMock
            .Setup(x => x.UpdateGoalProgressAsync(It.IsAny<PersonalGoal>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGoalResult);

        var command = new ProcessGameEndGamificationCommand(userId, gameSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.GoalUpdates.Should().HaveCount(1);
    }

    private void SetupMocksForSuccessfulProcessing(
        User user,
        GameSession gameSession,
        StreakResult? streakResult = null,
        AchievementUnlockResult? achievementResult = null,
        SeasonXPResult? seasonXPResult = null)
    {
        int capturedUserId = 0;
        
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns<int, CancellationToken>((id, ct) =>
            {
                capturedUserId = id;
                SetProperty(user, "Id", id);
                return Task.FromResult<User?>(user);
            });

        _gameSessionRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns<Guid, CancellationToken>((id, ct) =>
            {
                // Update the game session's user ID to match
                SetProperty(gameSession, "UserId", capturedUserId);
                return Task.FromResult<GameSession?>(gameSession);
            });

        _streakServiceMock
            .Setup(x => x.UpdateStreakAsync(It.IsAny<User>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(streakResult ?? new StreakResult(true, 1, 1, false, false));

        _challengeServiceMock
            .Setup(x => x.GetActiveChallengesAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge>());

        _achievementServiceMock
            .Setup(x => x.CheckAndUnlockAchievementsAsync(It.IsAny<User>(), It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievementResult ?? new AchievementUnlockResult(new List<Achievement>(), 0));

        _seasonPassServiceMock
            .Setup(x => x.AddXPAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(seasonXPResult ?? new SeasonXPResult(true, 50, 1, false, 0));

        _personalGoalServiceMock
            .Setup(x => x.GetActiveGoalsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PersonalGoal>());
    }

    private static User CreateUser(int id, Guid externalGuid)
    {
        var user = User.Create(externalGuid.ToString(), "test@test.com", "TestUser", AuthProvider.Local);
        SetProperty(user, "Id", id);
        return user;
    }

    private static GameSession CreateGameSession(Guid id, int userId, int score = 100, int linesCleared = 2)
    {
        var session = GameSession.Create(userId);
        SetProperty(session, "Id", id);
        SetProperty(session, "Score", score);
        SetProperty(session, "LinesCleared", linesCleared);
        session.EndGame();
        return session;
    }

    private static Achievement CreateAchievement(int id, string name)
    {
        var achievement = Achievement.Create(
            name,
            "Test achievement",
            AchievementCategory.Score,
            "/icon.png",
            "score >= 1000",
            RewardType.XPBoost,
            100);
        SetProperty(achievement, "Id", id);
        return achievement;
    }

    private static PersonalGoal CreatePersonalGoal(int id, int userId)
    {
        var goal = PersonalGoal.Create(userId, GoalType.BeatAverage, 1000, "Score 1000 points", DateTime.UtcNow.AddDays(1));
        SetProperty(goal, "Id", id);
        return goal;
    }

    private static void SetProperty<TEntity, TValue>(TEntity entity, string propertyName, TValue value) where TEntity : class
    {
        var backingField = typeof(TEntity).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(entity, value);
    }
}
