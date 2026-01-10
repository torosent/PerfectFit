using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;
using System.Reflection;

namespace PerfectFit.UnitTests.Services;

public class PersonalGoalServiceTests
{
    private readonly Mock<IGamificationRepository> _repositoryMock;
    private readonly PersonalGoalService _service;

    public PersonalGoalServiceTests()
    {
        _repositoryMock = new Mock<IGamificationRepository>();
        _service = new PersonalGoalService(_repositoryMock.Object);
    }

    #region GenerateGoalAsync Tests

    [Fact]
    public async Task GenerateGoal_BeatAverage_CorrectTarget()
    {
        // Arrange
        var user = CreateUser();
        var gameSessions = new List<GameSession>
        {
            CreateGameSession(100),
            CreateGameSession(200),
            CreateGameSession(300),
        };

        _repositoryMock.Setup(r => r.GetUserGameSessionsAsync(user.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameSessions);

        // Act
        var goal = await _service.GenerateGoalAsync(user, GoalType.BeatAverage);

        // Assert
        goal.Should().NotBeNull();
        goal.Type.Should().Be(GoalType.BeatAverage);
        // Average is 200, target should be 200 + 10% = 220
        goal.TargetValue.Should().Be(220);
        goal.UserId.Should().Be(user.Id);
        _repositoryMock.Verify(r => r.AddGoalAsync(It.IsAny<PersonalGoal>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateGoal_NewPersonalBest_TargetsHighScore()
    {
        // Arrange
        var user = CreateUser(highScore: 500);
        // Need to setup mock for the internal call
        _repositoryMock.Setup(r => r.GetUserGameSessionsAsync(user.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());

        // Act
        var goal = await _service.GenerateGoalAsync(user, GoalType.NewPersonalBest);

        // Assert
        goal.Should().NotBeNull();
        goal.Type.Should().Be(GoalType.NewPersonalBest);
        goal.TargetValue.Should().Be(501); // Current high score + 1
    }

    [Fact]
    public async Task GenerateGoal_ImproveAccuracy_BasedOnHistory()
    {
        // Arrange
        var user = CreateUser();
        var gameSessions = new List<GameSession>
        {
            CreateGameSessionWithStats(score: 100, linesCleared: 8, moveCount: 10), // 80% accuracy
            CreateGameSessionWithStats(score: 150, linesCleared: 7, moveCount: 10), // 70% accuracy
        };

        _repositoryMock.Setup(r => r.GetUserGameSessionsAsync(user.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameSessions);

        // Act
        var goal = await _service.GenerateGoalAsync(user, GoalType.ImproveAccuracy);

        // Assert
        goal.Should().NotBeNull();
        goal.Type.Should().Be(GoalType.ImproveAccuracy);
        // Average accuracy is 75%, target should be ~82-83% (75 + 10%)
        goal.TargetValue.Should().BeGreaterThanOrEqualTo(80);
    }

    [Fact]
    public async Task GenerateGoal_NoHistory_UsesDefaults()
    {
        // Arrange
        var user = CreateUser(highScore: 0);
        var gameSessions = new List<GameSession>();

        _repositoryMock.Setup(r => r.GetUserGameSessionsAsync(user.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameSessions);

        // Act
        var goal = await _service.GenerateGoalAsync(user, GoalType.BeatAverage);

        // Assert
        goal.Should().NotBeNull();
        goal.TargetValue.Should().BeGreaterThan(0); // Should have some reasonable default
    }

    #endregion

    #region UpdateGoalProgressAsync Tests

    [Fact]
    public async Task UpdateGoalProgress_Completes_WhenTargetReached()
    {
        // Arrange
        var goal = CreatePersonalGoal(targetValue: 100, currentValue: 80);

        // Act
        var result = await _service.UpdateGoalProgressAsync(goal, 120);

        // Assert
        result.Success.Should().BeTrue();
        result.GoalId.Should().Be(1);
        result.Description.Should().Be("Test goal");
        result.NewProgress.Should().Be(120);
        result.IsCompleted.Should().BeTrue();
        _repositoryMock.Verify(r => r.UpdateGoalAsync(goal, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateGoalProgress_NotCompleted_WhenBelowTarget()
    {
        // Arrange
        var goal = CreatePersonalGoal(targetValue: 100, currentValue: 0);

        // Act
        var result = await _service.UpdateGoalProgressAsync(goal, 50);

        // Assert
        result.Success.Should().BeTrue();
        result.GoalId.Should().Be(1);
        result.Description.Should().Be("Test goal");
        result.NewProgress.Should().Be(50);
        result.IsCompleted.Should().BeFalse();
    }

    #endregion

    #region GetActiveGoalsAsync Tests

    [Fact]
    public async Task GetActiveGoals_ExcludesExpired()
    {
        // Arrange
        var activeGoals = new List<PersonalGoal>
        {
            CreatePersonalGoal(id: 1, expiresAt: DateTime.UtcNow.AddHours(12)),
            CreatePersonalGoal(id: 2, expiresAt: DateTime.UtcNow.AddHours(6)),
        };

        _repositoryMock.Setup(r => r.GetActiveGoalsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeGoals);

        // Act
        var result = await _service.GetActiveGoalsAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.All(g => !g.IsExpired).Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveGoals_ExcludesCompleted()
    {
        // Arrange
        var activeGoals = new List<PersonalGoal>
        {
            CreatePersonalGoal(id: 1, isCompleted: false),
        };

        _repositoryMock.Setup(r => r.GetActiveGoalsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeGoals);

        // Act
        var result = await _service.GetActiveGoalsAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.All(g => !g.IsCompleted).Should().BeTrue();
    }

    #endregion

    #region CalculateUserStatsAsync Tests

    [Fact]
    public async Task CalculateUserStats_CorrectValues()
    {
        // Arrange
        var gameSessions = new List<GameSession>
        {
            CreateGameSessionWithStats(score: 100, linesCleared: 10, moveCount: 15, maxCombo: 3),
            CreateGameSessionWithStats(score: 200, linesCleared: 20, moveCount: 25, maxCombo: 5),
            CreateGameSessionWithStats(score: 300, linesCleared: 30, moveCount: 35, maxCombo: 7),
        };

        _repositoryMock.Setup(r => r.GetUserGameSessionsAsync(1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameSessions);

        // Act
        var stats = await _service.CalculateUserStatsAsync(1);

        // Assert
        stats.AverageScore.Should().Be(200);
        stats.BestScore.Should().Be(300);
        stats.TotalGames.Should().Be(3);
        stats.TotalLinesCleared.Should().Be(60);
        stats.AverageLinesCleared.Should().Be(20);
        stats.BestCombo.Should().Be(7);
        stats.AverageCombo.Should().Be(5);
    }

    [Fact]
    public async Task CalculateUserStats_NoGames_ReturnsZeros()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetUserGameSessionsAsync(1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());

        // Act
        var stats = await _service.CalculateUserStatsAsync(1);

        // Assert
        stats.AverageScore.Should().Be(0);
        stats.BestScore.Should().Be(0);
        stats.TotalGames.Should().Be(0);
        stats.Accuracy.Should().Be(0);
    }

    #endregion

    #region Helper Methods

    private static User CreateUser(int id = 1, int highScore = 0)
    {
        var user = User.Create("ext_123", "test@test.com", "TestUser", AuthProvider.Local);
        SetProperty(user, "Id", id);
        SetProperty(user, "HighScore", highScore);
        return user;
    }

    private static GameSession CreateGameSession(int score)
    {
        var session = GameSession.Create(1);
        session.AddScore(score, score / 10);
        session.EndGame();
        return session;
    }

    private static GameSession CreateGameSessionWithStats(
        int score,
        int linesCleared,
        int moveCount = 10,
        int maxCombo = 1)
    {
        var session = GameSession.Create(1);
        session.AddScore(score, linesCleared);
        session.UpdateCombo(maxCombo);

        // Set move count via reflection
        SetProperty(session, "MoveCount", moveCount);

        session.EndGame();
        return session;
    }

    private static PersonalGoal CreatePersonalGoal(
        int id = 1,
        int targetValue = 100,
        int currentValue = 0,
        bool isCompleted = false,
        DateTime? expiresAt = null)
    {
        var goal = PersonalGoal.Create(
            1,
            GoalType.BeatAverage,
            targetValue,
            "Test goal",
            expiresAt ?? DateTime.UtcNow.AddHours(24));

        SetProperty(goal, "Id", id);
        SetProperty(goal, "CurrentValue", currentValue);
        SetProperty(goal, "IsCompleted", isCompleted);

        return goal;
    }

    private static void SetProperty<T, TValue>(T obj, string propertyName, TValue value) where T : class
    {
        var backingField = typeof(T).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(obj, value);
    }

    #endregion
}
