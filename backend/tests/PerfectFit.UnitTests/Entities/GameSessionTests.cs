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
}
