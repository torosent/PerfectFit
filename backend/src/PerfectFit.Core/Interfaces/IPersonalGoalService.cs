using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.Core.Interfaces;

/// <summary>
/// Service for managing personalized goals.
/// </summary>
public interface IPersonalGoalService
{
    /// <summary>
    /// Gets all active (non-expired, non-completed) goals for a user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of active personal goals.</returns>
    Task<IReadOnlyList<PersonalGoal>> GetActiveGoalsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Generates a new personalized goal based on user history.
    /// </summary>
    /// <param name="user">The user to generate a goal for.</param>
    /// <param name="type">The type of goal to generate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created personal goal.</returns>
    Task<PersonalGoal> GenerateGoalAsync(User user, GoalType type, CancellationToken ct = default);

    /// <summary>
    /// Updates the progress of a personal goal.
    /// </summary>
    /// <param name="goal">The goal to update.</param>
    /// <param name="newValue">The new progress value.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the progress update.</returns>
    Task<GoalProgressResult> UpdateGoalProgressAsync(PersonalGoal goal, int newValue, CancellationToken ct = default);

    /// <summary>
    /// Calculates comprehensive statistics for a user based on their game history.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>User statistics.</returns>
    Task<UserStats> CalculateUserStatsAsync(int userId, CancellationToken ct = default);
}
