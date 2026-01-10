namespace PerfectFit.Core.Services.Results;

/// <summary>
/// Result of a streak update operation.
/// </summary>
/// <param name="Success">Whether the streak was successfully updated.</param>
/// <param name="NewStreak">The new current streak value.</param>
/// <param name="LongestStreak">The user's longest streak value.</param>
/// <param name="StreakBroken">Whether the streak was broken.</param>
/// <param name="UsedFreeze">Whether a streak freeze token was used to save the streak.</param>
/// <param name="ErrorMessage">Error message if the operation failed.</param>
public record StreakResult(
    bool Success,
    int NewStreak,
    int LongestStreak,
    bool StreakBroken,
    bool UsedFreeze,
    string? ErrorMessage = null
);
