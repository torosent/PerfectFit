namespace PerfectFit.Core.Enums;

/// <summary>
/// Defines the type of goal/progress calculation for a challenge.
/// </summary>
public enum ChallengeGoalType
{
    /// <summary>Accumulate total score across all games</summary>
    ScoreTotal = 0,

    /// <summary>Achieve a target score in a single game</summary>
    ScoreSingleGame = 1,

    /// <summary>Complete a number of games</summary>
    GameCount = 2,

    /// <summary>Win/complete consecutive games (streak)</summary>
    WinStreak = 3,

    /// <summary>Achieve a target accuracy percentage</summary>
    Accuracy = 4,

    /// <summary>Time-based challenges (play for X minutes)</summary>
    TimeBased = 5
}
