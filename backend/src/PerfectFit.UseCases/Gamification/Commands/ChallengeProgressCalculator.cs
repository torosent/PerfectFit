using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.UseCases.Gamification.Commands;

/// <summary>
/// Static class for calculating challenge progress based on GoalType or description parsing.
/// </summary>
public static class ChallengeProgressCalculator
{
    /// <summary>
    /// Calculates the progress delta for a challenge based on a completed game session.
    /// Uses GoalType if explicitly set, otherwise falls back to description parsing.
    /// </summary>
    /// <param name="challenge">The challenge to calculate progress for.</param>
    /// <param name="gameSession">The completed game session.</param>
    /// <returns>The progress delta to add to the user's challenge progress.</returns>
    public static int CalculateChallengeProgress(Challenge challenge, GameSession gameSession)
    {
        // Use GoalType if explicitly set
        if (challenge.GoalType.HasValue)
        {
            return challenge.GoalType.Value switch
            {
                ChallengeGoalType.ScoreTotal => gameSession.Score,
                ChallengeGoalType.ScoreSingleGame => gameSession.Score >= challenge.TargetValue ? challenge.TargetValue : 0,
                ChallengeGoalType.GameCount => 1,
                ChallengeGoalType.WinStreak => 1, // Streak tracking handled elsewhere
                ChallengeGoalType.Accuracy => 1, // Accuracy tracking handled elsewhere
                ChallengeGoalType.TimeBased => CalculateTimeProgress(gameSession),
                _ => 1
            };
        }

        // Fallback to description parsing for backward compatibility
        return CalculateProgressFromDescription(challenge, gameSession);
    }

    /// <summary>
    /// Calculates time-based progress as game duration in minutes (ceiling).
    /// </summary>
    private static int CalculateTimeProgress(GameSession gameSession)
    {
        if (gameSession.EndedAt.HasValue && gameSession.StartedAt != default)
        {
            var duration = (gameSession.EndedAt.Value - gameSession.StartedAt).TotalMinutes;
            return (int)Math.Ceiling(duration);
        }
        return 1;
    }

    /// <summary>
    /// Legacy method: Calculates progress by parsing challenge description.
    /// Kept for backward compatibility with challenges that don't have GoalType set.
    /// </summary>
    private static int CalculateProgressFromDescription(Challenge challenge, GameSession gameSession)
    {
        // Determine progress type based on challenge name/description patterns
        var descLower = challenge.Description.ToLowerInvariant();

        // Check more specific patterns FIRST before general ones

        // High score challenges: "score at least X in a single game", "in a game", "in one"
        // Must check BEFORE "points" since these also contain "points"
        if (descLower.Contains("single game") || descLower.Contains("in a game") || descLower.Contains("in one"))
        {
            // Progress is whether the threshold was met (1 = met, 0 = not met)
            return gameSession.Score >= challenge.TargetValue ? 1 : 0;
        }

        // Streak challenges: "X in a row", "consecutive"
        // Must check BEFORE "games" since these may also contain "games"
        if (descLower.Contains("in a row") || descLower.Contains("consecutive") || descLower.Contains("streak"))
        {
            return 1; // Would need streak tracking logic
        }

        // Game count challenges: "complete X games", "play X games", "win X games"
        // Check for "games" (plural) which indicates counting games
        if (descLower.Contains("games") && (descLower.Contains("complete") || descLower.Contains("play") || descLower.Contains("win")))
        {
            return 1; // Each game completion adds 1
        }

        // Score-based challenges: "score X points total", "X points" (accumulative)
        // Only match if it's clearly about accumulating total points
        if ((descLower.Contains("points") && descLower.Contains("total")) || 
            (descLower.Contains("score") && descLower.Contains("total")))
        {
            return gameSession.Score;
        }

        // Accuracy challenges: "X% accuracy"
        if (descLower.Contains("accuracy"))
        {
            // Would need accuracy data from game session - for now assume 1 if game completed
            return 1;
        }

        // Time-based challenges: "X minutes", "X seconds", "under X"
        if (descLower.Contains("minute") || descLower.Contains("second") || descLower.Contains("under"))
        {
            // Duration tracking - estimate based on game session if available
            return CalculateTimeProgress(gameSession);
        }

        // Default: count as 1 game completion
        return 1;
    }
}
