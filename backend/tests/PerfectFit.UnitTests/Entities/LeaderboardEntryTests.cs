using FluentAssertions;
using PerfectFit.Core.Entities;

namespace PerfectFit.UnitTests.Entities;

public class LeaderboardEntryTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateLeaderboardEntry()
    {
        // Arrange
        var userId = 1;
        var score = 1000;
        var linesCleared = 50;
        var maxCombo = 5;
        var gameSessionId = Guid.NewGuid();

        // Act
        var entry = LeaderboardEntry.Create(userId, score, linesCleared, maxCombo, gameSessionId);

        // Assert
        entry.UserId.Should().Be(userId);
        entry.Score.Should().Be(score);
        entry.LinesCleared.Should().Be(linesCleared);
        entry.MaxCombo.Should().Be(maxCombo);
        entry.GameSessionId.Should().Be(gameSessionId);
        entry.AchievedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithZeroScore_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => LeaderboardEntry.Create(1, 0, 10, 5, Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("score");
    }

    [Fact]
    public void Create_WithNegativeScore_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => LeaderboardEntry.Create(1, -100, 10, 5, Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("score");
    }

    [Fact]
    public void Create_WithZeroUserId_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => LeaderboardEntry.Create(0, 100, 10, 5, Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("userId");
    }

    [Fact]
    public void Create_WithEmptyGameSessionId_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => LeaderboardEntry.Create(1, 100, 10, 5, Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("gameSessionId");
    }

    [Fact]
    public void Create_WithNegativeLinesCleared_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => LeaderboardEntry.Create(1, 100, -5, 5, Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("linesCleared");
    }

    [Fact]
    public void Create_WithNegativeMaxCombo_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => LeaderboardEntry.Create(1, 100, 10, -1, Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("maxCombo");
    }
}
