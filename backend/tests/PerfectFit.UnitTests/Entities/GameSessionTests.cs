using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.UnitTests.Entities;

public class GameSessionTests
{
    [Fact]
    public void Create_WithUserId_ShouldCreateGameSession()
    {
        // Arrange
        int userId = 1;

        // Act
        var session = GameSession.Create(userId);

        // Assert
        session.Id.Should().NotBeEmpty();
        session.UserId.Should().Be(userId);
        session.BoardState.Should().NotBeNullOrEmpty();
        session.CurrentPieces.Should().NotBeNullOrEmpty();
        session.PieceBagState.Should().NotBeNullOrEmpty();
        session.Score.Should().Be(0);
        session.Combo.Should().Be(0);
        session.LinesCleared.Should().Be(0);
        session.MaxCombo.Should().Be(0);
        session.Status.Should().Be(GameStatus.Playing);
        session.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        session.EndedAt.Should().BeNull();
        session.LastActivityAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithNullUserId_ShouldCreateGuestGameSession()
    {
        // Arrange & Act
        var session = GameSession.Create(null);

        // Assert
        session.Id.Should().NotBeEmpty();
        session.UserId.Should().BeNull();
        session.Status.Should().Be(GameStatus.Playing);
    }

    [Fact]
    public void UpdateBoard_WithValidState_ShouldUpdateBoardAndActivity()
    {
        // Arrange
        var session = GameSession.Create(1);
        var newBoardState = """{"grid":[[0,0,0,0,0,0,0,0,0,0]]}""";
        var newPieces = """[{"type":"L"}]""";
        var newPieceBag = """{"index":5}""";

        // Act
        session.UpdateBoard(newBoardState, newPieces, newPieceBag);

        // Assert
        session.BoardState.Should().Be(newBoardState);
        session.CurrentPieces.Should().Be(newPieces);
        session.PieceBagState.Should().Be(newPieceBag);
        session.LastActivityAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateBoard_WhenGameEnded_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var session = GameSession.Create(1);
        session.EndGame();

        // Act
        var act = () => session.UpdateBoard("state", "pieces", "bag");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public void AddScore_ShouldAddToCurrentScoreAndLines()
    {
        // Arrange
        var session = GameSession.Create(1);

        // Act
        session.AddScore(100, 1);
        session.AddScore(50, 2);

        // Assert
        session.Score.Should().Be(150);
        session.LinesCleared.Should().Be(3);
    }

    [Fact]
    public void AddScore_WhenGameEnded_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var session = GameSession.Create(1);
        session.EndGame();

        // Act
        var act = () => session.AddScore(100, 1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public void UpdateCombo_ShouldUpdateComboAndTrackMaxCombo()
    {
        // Arrange
        var session = GameSession.Create(1);

        // Act
        session.UpdateCombo(3);

        // Assert
        session.Combo.Should().Be(3);
        session.MaxCombo.Should().Be(3);
    }

    [Fact]
    public void UpdateCombo_WhenNewComboIsHigher_ShouldUpdateMaxCombo()
    {
        // Arrange
        var session = GameSession.Create(1);
        session.UpdateCombo(3);
        session.UpdateCombo(1);  // Reset combo
        session.UpdateCombo(5);  // New higher combo

        // Assert
        session.Combo.Should().Be(5);
        session.MaxCombo.Should().Be(5);
    }

    [Fact]
    public void UpdateCombo_WhenNewComboIsLower_ShouldNotUpdateMaxCombo()
    {
        // Arrange
        var session = GameSession.Create(1);
        session.UpdateCombo(5);

        // Act
        session.UpdateCombo(2);

        // Assert
        session.Combo.Should().Be(2);
        session.MaxCombo.Should().Be(5);
    }

    [Fact]
    public void EndGame_ShouldSetStatusToEndedAndSetEndedAt()
    {
        // Arrange
        var session = GameSession.Create(1);

        // Act
        session.EndGame();

        // Assert
        session.Status.Should().Be(GameStatus.Ended);
        session.EndedAt.Should().NotBeNull();
        session.EndedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void EndGame_WhenAlreadyEnded_ShouldNotThrow()
    {
        // Arrange
        var session = GameSession.Create(1);
        session.EndGame();

        // Act
        var act = () => session.EndGame();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AbandonGame_ShouldSetStatusToAbandoned()
    {
        // Arrange
        var session = GameSession.Create(1);

        // Act
        session.AbandonGame();

        // Assert
        session.Status.Should().Be(GameStatus.Abandoned);
        session.EndedAt.Should().NotBeNull();
    }

    // Anti-cheat tests
    [Fact]
    public void Create_ShouldInitializeAntiCheatFields()
    {
        // Arrange & Act
        var session = GameSession.Create(1, "test-fingerprint");

        // Assert
        session.MoveCount.Should().Be(0);
        session.LastMoveAt.Should().BeNull();
        session.MoveHistory.Should().Be("[]");
        session.ClientFingerprint.Should().Be("test-fingerprint");
    }

    [Fact]
    public void RecordMove_ShouldIncrementMoveCount()
    {
        // Arrange
        var session = GameSession.Create(1);

        // Act
        session.RecordMove(0, 5, 5, 100, 1);
        session.RecordMove(1, 3, 3, 50, 0);

        // Assert
        session.MoveCount.Should().Be(2);
    }

    [Fact]
    public void RecordMove_ShouldUpdateLastMoveAt()
    {
        // Arrange
        var session = GameSession.Create(1);

        // Act
        session.RecordMove(0, 5, 5, 100, 1);

        // Assert
        session.LastMoveAt.Should().NotBeNull();
        session.LastMoveAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RecordMove_ShouldAppendToMoveHistory()
    {
        // Arrange
        var session = GameSession.Create(1);

        // Act
        session.RecordMove(0, 5, 5, 100, 1);
        session.RecordMove(1, 3, 3, 50, 0);

        // Assert
        session.MoveHistory.Should().StartWith("[{");
        session.MoveHistory.Should().Contain("\"i\":0");
        session.MoveHistory.Should().Contain("\"i\":1");
        session.MoveHistory.Should().Contain("\"r\":5");
        session.MoveHistory.Should().Contain("\"c\":5");
        session.MoveHistory.Should().Contain("\"p\":100");
        session.MoveHistory.Should().Contain("\"l\":1");
    }

    [Fact]
    public void RecordMove_WhenGameEnded_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var session = GameSession.Create(1);
        session.EndGame();

        // Act
        var act = () => session.RecordMove(0, 5, 5, 100, 1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public void GetTimeSinceLastMove_WhenNoMoves_ShouldReturnNull()
    {
        // Arrange
        var session = GameSession.Create(1);

        // Act
        var result = session.GetTimeSinceLastMove();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTimeSinceLastMove_AfterMove_ShouldReturnPositiveValue()
    {
        // Arrange
        var session = GameSession.Create(1);
        session.RecordMove(0, 5, 5, 100, 1);

        // Act
        var result = session.GetTimeSinceLastMove();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetGameDuration_ShouldReturnPositiveValue()
    {
        // Arrange
        var session = GameSession.Create(1);

        // Act
        var result = session.GetGameDuration();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetGameDuration_WhenGameEnded_ShouldUseEndedAt()
    {
        // Arrange
        var session = GameSession.Create(1);
        session.EndGame();

        // Act
        var duration1 = session.GetGameDuration();
        Thread.Sleep(100); // Wait a bit
        var duration2 = session.GetGameDuration();

        // Assert - duration should not change after game ended
        duration1.Should().BeApproximately(duration2, 0.1);
    }
}
