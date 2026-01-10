using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.UseCases.Gamification.Commands;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Commands;

/// <summary>
/// Tests for challenge progress calculation based on GoalType enum.
/// </summary>
public class ChallengeProgressCalculationTests
{
    #region GoalType-based Progress Tests

    [Fact]
    public void CalculateChallengeProgress_ScoreTotal_ReturnsGameSessionScore()
    {
        // Arrange
        var challenge = CreateChallenge(
            "Score Challenge",
            "Score 1000 points total",
            targetValue: 1000,
            goalType: ChallengeGoalType.ScoreTotal);
        var gameSession = CreateGameSession(score: 500);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(500);
    }

    [Fact]
    public void CalculateChallengeProgress_ScoreSingleGame_ReturnsOneWhenScoreExceedsTarget()
    {
        // Arrange
        var challenge = CreateChallenge(
            "High Score Challenge",
            "Score 500 in a single game",
            targetValue: 500,
            goalType: ChallengeGoalType.ScoreSingleGame);
        var gameSession = CreateGameSession(score: 600);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(1);
    }

    [Fact]
    public void CalculateChallengeProgress_ScoreSingleGame_ReturnsOneWhenScoreEqualsTarget()
    {
        // Arrange
        var challenge = CreateChallenge(
            "High Score Challenge",
            "Score 500 in a single game",
            targetValue: 500,
            goalType: ChallengeGoalType.ScoreSingleGame);
        var gameSession = CreateGameSession(score: 500);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(1);
    }

    [Fact]
    public void CalculateChallengeProgress_ScoreSingleGame_ReturnsZeroWhenScoreBelowTarget()
    {
        // Arrange
        var challenge = CreateChallenge(
            "High Score Challenge",
            "Score 500 in a single game",
            targetValue: 500,
            goalType: ChallengeGoalType.ScoreSingleGame);
        var gameSession = CreateGameSession(score: 499);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(0);
    }

    [Fact]
    public void CalculateChallengeProgress_GameCount_ReturnsOne()
    {
        // Arrange
        var challenge = CreateChallenge(
            "Play Games Challenge",
            "Complete 10 games",
            targetValue: 10,
            goalType: ChallengeGoalType.GameCount);
        var gameSession = CreateGameSession(score: 100);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(1);
    }

    [Fact]
    public void CalculateChallengeProgress_WinStreak_ReturnsOne()
    {
        // Arrange
        var challenge = CreateChallenge(
            "Streak Challenge",
            "Achieve 5 wins in a row",
            targetValue: 5,
            goalType: ChallengeGoalType.WinStreak);
        var gameSession = CreateGameSession(score: 100);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(1);
    }

    [Fact]
    public void CalculateChallengeProgress_Accuracy_ReturnsOne()
    {
        // Arrange
        var challenge = CreateChallenge(
            "Accuracy Challenge",
            "Achieve 80% accuracy",
            targetValue: 80,
            goalType: ChallengeGoalType.Accuracy);
        var gameSession = CreateGameSession(score: 100);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(1);
    }

    [Fact]
    public void CalculateChallengeProgress_TimeBased_ReturnsGameDurationInMinutes()
    {
        // Arrange
        var challenge = CreateChallenge(
            "Time Challenge",
            "Play for 10 minutes",
            targetValue: 10,
            goalType: ChallengeGoalType.TimeBased);
        
        // Use exact times to avoid timing issues
        var startTime = new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2026, 1, 10, 10, 5, 0, DateTimeKind.Utc); // Exactly 5 minutes later
        var gameSession = CreateGameSession(score: 100, startedAt: startTime, endedAt: endTime);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(5); // 5 minutes, ceiling rounded
    }

    [Fact]
    public void CalculateChallengeProgress_TimeBased_CeilsPartialMinutes()
    {
        // Arrange
        var challenge = CreateChallenge(
            "Time Challenge",
            "Play for 10 minutes",
            targetValue: 10,
            goalType: ChallengeGoalType.TimeBased);
        
        // Use exact times to avoid timing issues - 3 minutes 30 seconds
        var startTime = new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2026, 1, 10, 10, 3, 30, DateTimeKind.Utc);
        var gameSession = CreateGameSession(score: 100, startedAt: startTime, endedAt: endTime);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(4); // 3.5 minutes rounds up to 4
    }

    [Fact]
    public void CalculateChallengeProgress_TimeBased_ReturnsOneWhenNoEndTime()
    {
        // Arrange
        var challenge = CreateChallenge(
            "Time Challenge",
            "Play for 10 minutes",
            targetValue: 10,
            goalType: ChallengeGoalType.TimeBased);
        
        var gameSession = CreateGameSession(score: 100);
        // Clear the EndedAt property to simulate an unfinished game
        SetProperty(gameSession, "EndedAt", (DateTime?)null);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(1);
    }

    #endregion

    #region Fallback to Description Parsing Tests

    [Fact]
    public void CalculateChallengeProgress_NullGoalType_FallsBackToDescriptionParsing_ScoreChallenge()
    {
        // Arrange
        var challenge = CreateChallenge(
            "Score Challenge",
            "Score 1000 points total",
            targetValue: 1000,
            goalType: null);
        var gameSession = CreateGameSession(score: 500);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(500); // Description contains "points" so returns score
    }

    [Fact]
    public void CalculateChallengeProgress_NullGoalType_FallsBackToDescriptionParsing_GameCountChallenge()
    {
        // Arrange
        var challenge = CreateChallenge(
            "Play Challenge",
            "Complete 10 games",
            targetValue: 10,
            goalType: null);
        var gameSession = CreateGameSession(score: 500);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(1); // Description contains "games" so returns 1
    }

    [Fact]
    public void CalculateChallengeProgress_NullGoalType_FallsBackToDescriptionParsing_SingleGameChallenge()
    {
        // Arrange
        var challenge = CreateChallenge(
            "High Score Challenge",
            "Score at least 500 in a single game",
            targetValue: 500,
            goalType: null);
        var gameSession = CreateGameSession(score: 600);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(1); // Description contains "single game" and score >= target
    }

    [Fact]
    public void CalculateChallengeProgress_NullGoalType_FallsBackToDescriptionParsing_StreakChallenge()
    {
        // Arrange
        var challenge = CreateChallenge(
            "Streak Challenge",
            "Win 5 games in a row",
            targetValue: 5,
            goalType: null);
        var gameSession = CreateGameSession(score: 500);

        // Act
        var progress = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);

        // Assert
        progress.Should().Be(1); // Description contains "in a row" so returns 1
    }

    #endregion

    #region Helper Methods

    private static Challenge CreateChallenge(
        string name,
        string description,
        int targetValue,
        ChallengeGoalType? goalType)
    {
        var challenge = Challenge.Create(
            name,
            description,
            ChallengeType.Weekly,
            targetValue,
            100, // XP reward
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7),
            templateId: null,
            goalType: goalType);
        return challenge;
    }

    private static GameSession CreateGameSession(
        int score,
        DateTime? startedAt = null,
        DateTime? endedAt = null)
    {
        var session = GameSession.Create(1);
        SetProperty(session, "Score", score);
        
        if (startedAt.HasValue)
        {
            SetProperty(session, "StartedAt", startedAt.Value);
        }
        
        if (endedAt.HasValue)
        {
            SetProperty(session, "EndedAt", endedAt.Value);
        }
        else
        {
            session.EndGame();
        }
        
        return session;
    }

    private static void SetProperty<TEntity, TValue>(TEntity entity, string propertyName, TValue value) where TEntity : class
    {
        var backingField = typeof(TEntity).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(entity, value);
    }

    #endregion
}
