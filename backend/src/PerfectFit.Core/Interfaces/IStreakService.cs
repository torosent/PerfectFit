using PerfectFit.Core.Entities;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.Core.Interfaces;

/// <summary>
/// Service for managing user streaks with timezone-aware calculations.
/// </summary>
public interface IStreakService
{
    /// <summary>
    /// Updates the user's streak based on a completed game session.
    /// </summary>
    /// <param name="user">The user whose streak to update.</param>
    /// <param name="gameEndTime">The time the game ended (UTC).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the streak update operation.</returns>
    Task<StreakResult> UpdateStreakAsync(User user, DateTimeOffset gameEndTime, CancellationToken ct = default);

    /// <summary>
    /// Uses a streak freeze token to maintain the user's streak.
    /// </summary>
    /// <param name="user">The user attempting to use a freeze token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the freeze was used successfully, false if no tokens available.</returns>
    Task<bool> UseStreakFreezeAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Checks if the user's streak is at risk of being lost.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="currentTime">The current time (UTC).</param>
    /// <returns>True if the streak is at risk (within last hour before reset).</returns>
    bool IsStreakAtRisk(User user, DateTimeOffset currentTime);

    /// <summary>
    /// Gets the time when the user's streak will reset if no game is played.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="currentTime">The current time (UTC).</param>
    /// <returns>The UTC time when the streak will reset.</returns>
    DateTimeOffset GetStreakResetTime(User user, DateTimeOffset currentTime);
}
