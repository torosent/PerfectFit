namespace PerfectFit.Core.Services.Results;

/// <summary>
/// Result of updating personal goal progress.
/// </summary>
/// <param name="Success">Whether the progress was successfully updated.</param>
/// <param name="GoalId">The ID of the goal.</param>
/// <param name="Description">The description of the goal.</param>
/// <param name="NewProgress">The new progress value.</param>
/// <param name="IsCompleted">Whether the goal is now completed.</param>
/// <param name="ErrorMessage">Error message if the operation failed.</param>
public record GoalProgressResult(
    bool Success,
    int GoalId,
    string Description,
    int NewProgress,
    bool IsCompleted,
    string? ErrorMessage = null
);
