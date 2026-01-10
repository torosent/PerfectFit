namespace PerfectFit.Core.Services.Results;

/// <summary>
/// Result of adding XP to a user's season pass.
/// </summary>
/// <param name="Success">Whether the XP was successfully added.</param>
/// <param name="NewXP">The user's new total season XP.</param>
/// <param name="NewTier">The user's new season tier.</param>
/// <param name="TierUp">Whether the user leveled up to a new tier.</param>
/// <param name="RewardsAvailable">Number of unclaimed rewards available.</param>
/// <param name="ErrorMessage">Error message if the operation failed.</param>
public record SeasonXPResult(
    bool Success,
    int NewXP,
    int NewTier,
    bool TierUp,
    int RewardsAvailable,
    string? ErrorMessage = null
);
