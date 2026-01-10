namespace PerfectFit.Core.Services.Results;

/// <summary>
/// Result of a challenge progress update operation.
/// </summary>
/// <param name="Success">Whether the progress was successfully updated.</param>
/// <param name="ChallengeId">The ID of the challenge.</param>
/// <param name="ChallengeName">The name of the challenge.</param>
/// <param name="NewProgress">The new progress value.</param>
/// <param name="IsCompleted">Whether the challenge is now completed.</param>
/// <param name="XPEarned">Amount of XP earned (if challenge completed).</param>
/// <param name="ErrorMessage">Error message if the operation failed.</param>
public record ChallengeProgressResult(
    bool Success,
    int ChallengeId,
    string ChallengeName,
    int NewProgress,
    bool IsCompleted,
    int XPEarned,
    string? ErrorMessage = null
);
