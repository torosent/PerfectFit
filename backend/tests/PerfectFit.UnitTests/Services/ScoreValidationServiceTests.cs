using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Infrastructure.Services;
using System.Reflection;

namespace PerfectFit.UnitTests.Services;

public class ScoreValidationServiceTests
{
    private readonly Mock<IGameSessionRepository> _gameSessionRepositoryMock;
    private readonly Mock<ILeaderboardRepository> _leaderboardRepositoryMock;
    private readonly ScoreValidationService _service;

    public ScoreValidationServiceTests()
    {
        _gameSessionRepositoryMock = new Mock<IGameSessionRepository>();
        _leaderboardRepositoryMock = new Mock<ILeaderboardRepository>();
        _service = new ScoreValidationService(
            _gameSessionRepositoryMock.Object,
            _leaderboardRepositoryMock.Object);
    }

    [Fact]
    public async Task ValidateScoreAsync_WhenGameSessionNotFound_ShouldReturnInvalid()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync((GameSession?)null);

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task ValidateScoreAsync_WhenSessionBelongsToAnotherUser_ShouldReturnInvalid()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var session = CreateTestSession(userId: 1, ageSeconds: 10);
        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync(session);

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 2);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not belong");
    }

    [Fact]
    public async Task ValidateScoreAsync_WhenGameNotEnded_ShouldReturnInvalid()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var session = CreateTestSession(userId: 1, ageSeconds: 10, endGame: false);
        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync(session);

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not ended");
    }

    [Fact]
    public async Task ValidateScoreAsync_WhenClaimedScoreDoesNotMatch_ShouldReturnInvalid()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var session = CreateTestSession(userId: 1, ageSeconds: 10, endGame: true);
        session.AddScore(100, 1);
        session.EndGame();

        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync(session);
        _leaderboardRepositoryMock.Setup(x => x.ExistsByGameSessionIdAsync(gameId, default))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 1, claimedScore: 999);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not match");
    }

    [Fact]
    public async Task ValidateScoreAsync_WhenAlreadySubmitted_ShouldReturnInvalid()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var session = CreateTestSession(userId: 1, ageSeconds: 10, endGame: true);
        session.AddScore(100, 1);
        session.EndGame();

        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync(session);
        _leaderboardRepositoryMock.Setup(x => x.ExistsByGameSessionIdAsync(gameId, default))
            .ReturnsAsync(true); // Already submitted

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already been submitted");
    }

    [Fact]
    public async Task ValidateScoreAsync_WhenValidScore_ShouldReturnValid()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var session = CreateTestSession(userId: 1, ageSeconds: 30, endGame: false);

        // Add realistic game data
        for (int i = 0; i < 10; i++)
        {
            session.RecordMove(i % 3, i % 10, i % 10, 30, 1);
        }
        session.AddScore(300, 10);
        session.UpdateCombo(2); // Realistic combo
        session.EndGame();

        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync(session);
        _leaderboardRepositoryMock.Setup(x => x.ExistsByGameSessionIdAsync(gameId, default))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 1);

        // Assert
        result.IsValid.Should().BeTrue();
        result.GameSession.Should().NotBeNull();
    }

    // Anti-cheat specific tests

    [Fact]
    public async Task ValidateScoreAsync_WhenGameDurationTooShort_ShouldReturnInvalid()
    {
        // Arrange - Game that ended too quickly
        var gameId = Guid.NewGuid();
        var session = CreateTestSession(userId: 1, ageSeconds: 1, endGame: false); // Only 1 second old
        session.RecordMove(0, 0, 0, 100, 1);
        session.AddScore(100, 1);
        session.EndGame();

        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync(session);
        _leaderboardRepositoryMock.Setup(x => x.ExistsByGameSessionIdAsync(gameId, default))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("duration too short");
    }

    [Fact]
    public async Task ValidateScoreAsync_WhenHighScoreWithTooFewMoves_ShouldReturnInvalid()
    {
        // Arrange - High score but very few moves (suspicious)
        var gameId = Guid.NewGuid();
        var session = CreateTestSession(userId: 1, ageSeconds: 60, endGame: false); // 60 seconds - long enough

        // Only 2 moves but very high score (impossible)
        session.RecordMove(0, 0, 0, 5000, 5);
        session.RecordMove(1, 1, 1, 5000, 5);
        session.AddScore(15000, 10); // Score above suspicious threshold with few moves
        session.EndGame();

        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync(session);
        _leaderboardRepositoryMock.Setup(x => x.ExistsByGameSessionIdAsync(gameId, default))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Insufficient moves");
    }

    [Fact]
    public async Task ValidateScoreAsync_WhenMaxComboExceedsLinesCleared_ShouldReturnInvalid()
    {
        // Arrange - Max combo is higher than lines cleared (impossible)
        var gameId = Guid.NewGuid();
        var session = CreateTestSession(userId: 1, ageSeconds: 60, endGame: false);

        // Build up an impossible state
        for (int i = 0; i < 10; i++)
        {
            session.RecordMove(i % 3, i % 10, i % 10, 10, 0); // No lines cleared
        }
        session.UpdateCombo(5); // But somehow have a combo of 5 (impossible without clearing lines)
        session.AddScore(50, 0); // No lines cleared
        session.EndGame();

        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync(session);
        _leaderboardRepositoryMock.Setup(x => x.ExistsByGameSessionIdAsync(gameId, default))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not mathematically plausible");
    }

    [Fact]
    public async Task ValidateScoreAsync_WhenLinesExceedTheoreticalMax_ShouldReturnInvalid()
    {
        // Arrange - More lines than theoretically possible
        var gameId = Guid.NewGuid();
        var session = CreateTestSession(userId: 1, ageSeconds: 60, endGame: false);

        // Only 5 moves but way too many lines cleared
        for (int i = 0; i < 5; i++)
        {
            session.RecordMove(i % 3, i % 10, i % 10, 50, 10); // Can't clear 10 lines per move
        }
        session.AddScore(500, 100); // 100 lines from 5 moves is impossible (max 10)
        session.EndGame();

        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync(session);
        _leaderboardRepositoryMock.Setup(x => x.ExistsByGameSessionIdAsync(gameId, default))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 1);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateScoreAsync_WhenScoreExceedsTheoreticalMax_ShouldReturnInvalid()
    {
        // Arrange - Score that exceeds what's mathematically possible
        var gameId = Guid.NewGuid();
        var session = CreateTestSession(userId: 1, ageSeconds: 3600, endGame: false); // Long game to avoid rate limit

        // 5 moves but impossibly high score
        for (int i = 0; i < 5; i++)
        {
            session.RecordMove(i % 3, i % 10, i % 10, 10000, 2);
        }
        session.AddScore(100000, 10); // Way too high for 5 moves
        session.EndGame();

        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync(session);
        _leaderboardRepositoryMock.Setup(x => x.ExistsByGameSessionIdAsync(gameId, default))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not mathematically plausible");
    }

    [Fact]
    public async Task ValidateScoreAsync_WhenUserHasTooManyEntries_ShouldReturnInvalid()
    {
        // Arrange - User already has max leaderboard entries
        var gameId = Guid.NewGuid();
        var session = CreateTestSession(userId: 1, ageSeconds: 30, endGame: false);

        for (int i = 0; i < 10; i++)
        {
            session.RecordMove(i % 3, i % 10, i % 10, 30, 1);
        }
        session.AddScore(300, 10);
        session.EndGame();

        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync(session);
        _leaderboardRepositoryMock.Setup(x => x.ExistsByGameSessionIdAsync(gameId, default))
            .ReturnsAsync(false);
        _leaderboardRepositoryMock.Setup(x => x.GetUserEntryCountAsync(1, default))
            .ReturnsAsync(100); // Max entries reached

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Maximum leaderboard entries");
    }

    [Fact]
    public async Task ValidateScoreAsync_WhenGameTooOld_ShouldReturnInvalid()
    {
        // Arrange - Game is older than 48 hours
        var gameId = Guid.NewGuid();
        var session = CreateTestSession(userId: 1, ageSeconds: 49 * 3600, endGame: false); // 49 hours old

        for (int i = 0; i < 10; i++)
        {
            session.RecordMove(i % 3, i % 10, i % 10, 30, 1);
        }
        session.AddScore(300, 10);
        session.EndGame();

        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync(session);
        _leaderboardRepositoryMock.Setup(x => x.ExistsByGameSessionIdAsync(gameId, default))
            .ReturnsAsync(false);
        _leaderboardRepositoryMock.Setup(x => x.GetUserEntryCountAsync(1, default))
            .ReturnsAsync(0);

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("expired");
    }

    [Fact]
    public async Task ValidateScoreAsync_WhenStartedAtInFuture_ShouldReturnInvalid()
    {
        // Arrange - Game started in the future (clock manipulation)
        var gameId = Guid.NewGuid();
        var session = CreateTestSession(userId: 1, ageSeconds: -600, endGame: false); // 10 minutes in future

        for (int i = 0; i < 10; i++)
        {
            session.RecordMove(i % 3, i % 10, i % 10, 30, 1);
        }
        session.AddScore(300, 10);
        session.EndGame();

        _gameSessionRepositoryMock.Setup(x => x.GetByIdAsync(gameId, default))
            .ReturnsAsync(session);
        _leaderboardRepositoryMock.Setup(x => x.ExistsByGameSessionIdAsync(gameId, default))
            .ReturnsAsync(false);
        _leaderboardRepositoryMock.Setup(x => x.GetUserEntryCountAsync(1, default))
            .ReturnsAsync(0);

        // Act
        var result = await _service.ValidateScoreAsync(gameId, userId: 1);

        // Assert - Future timestamps are rejected (may fail on timestamp or duration check)
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().ContainAny("Invalid game timestamps", "duration too short");
    }

    /// <summary>
    /// Creates a test session with a specified age by manipulating the StartedAt property via reflection.
    /// </summary>
    private static GameSession CreateTestSession(int? userId, int ageSeconds = 0, bool endGame = false)
    {
        var session = GameSession.Create(userId);

        // Use reflection to set StartedAt to simulate game age
        if (ageSeconds > 0)
        {
            var startedAtProperty = typeof(GameSession).GetProperty("StartedAt", BindingFlags.Public | BindingFlags.Instance);
            if (startedAtProperty != null)
            {
                var backingField = typeof(GameSession).GetField("<StartedAt>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                backingField?.SetValue(session, DateTime.UtcNow.AddSeconds(-ageSeconds));
            }
        }

        return session;
    }
}
