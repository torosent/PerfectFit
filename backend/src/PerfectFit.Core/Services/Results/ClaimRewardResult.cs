using PerfectFit.Core.Enums;

namespace PerfectFit.Core.Services.Results;

/// <summary>
/// Result of claiming a season reward.
/// </summary>
/// <param name="Success">Whether the reward was successfully claimed.</param>
/// <param name="RewardType">The type of reward that was claimed.</param>
/// <param name="RewardValue">The value of the reward that was claimed.</param>
/// <param name="ErrorMessage">Error message if the operation failed.</param>
public record ClaimRewardResult(
    bool Success,
    RewardType? RewardType,
    int? RewardValue,
    string? ErrorMessage = null
);
