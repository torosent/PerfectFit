using PerfectFit.Core.Entities;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.Core.Interfaces;

/// <summary>
/// Service for managing achievements and tracking user progress.
/// </summary>
public interface IAchievementService
{
    /// <summary>
    /// Gets all achievements in the system.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of all achievements.</returns>
    Task<IReadOnlyList<Achievement>> GetAllAchievementsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all achievements for a specific user with their progress.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of user achievements with progress.</returns>
    Task<IReadOnlyList<UserAchievement>> GetUserAchievementsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Checks all achievements and unlocks any that the user has met the requirements for.
    /// </summary>
    /// <param name="user">The user to check achievements for.</param>
    /// <param name="gameSession">Optional game session context for achievement checking.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result containing newly unlocked achievements and rewards.</returns>
    Task<AchievementUnlockResult> CheckAndUnlockAchievementsAsync(User user, GameSession? gameSession = null, CancellationToken ct = default);

    /// <summary>
    /// Calculates the progress percentage for a specific achievement.
    /// </summary>
    /// <param name="user">The user to calculate progress for.</param>
    /// <param name="achievement">The achievement to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Progress percentage (0-100).</returns>
    Task<int> CalculateProgressAsync(User user, Achievement achievement, CancellationToken ct = default);
}
