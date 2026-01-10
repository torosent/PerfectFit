using PerfectFit.Core.Entities;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.Core.Interfaces;

/// <summary>
/// Service for managing season passes, XP, and rewards.
/// </summary>
public interface ISeasonPassService
{
    /// <summary>
    /// Gets the currently active season.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current season, or null if no active season.</returns>
    Task<Season?> GetCurrentSeasonAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all rewards for a specific season.
    /// </summary>
    /// <param name="seasonId">The season's ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of season rewards.</returns>
    Task<IReadOnlyList<SeasonReward>> GetSeasonRewardsAsync(int seasonId, CancellationToken ct = default);

    /// <summary>
    /// Adds XP to a user's season pass from a specified source.
    /// </summary>
    /// <param name="user">The user to add XP to.</param>
    /// <param name="xpAmount">The amount of XP to add.</param>
    /// <param name="source">The source of the XP (e.g., "game_completion", "challenge", "achievement").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the XP addition.</returns>
    Task<SeasonXPResult> AddXPAsync(User user, int xpAmount, string source, CancellationToken ct = default);

    /// <summary>
    /// Claims a specific season reward for a user.
    /// </summary>
    /// <param name="user">The user claiming the reward.</param>
    /// <param name="seasonRewardId">The ID of the season reward to claim.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of claiming the reward.</returns>
    Task<ClaimRewardResult> ClaimRewardAsync(User user, int seasonRewardId, CancellationToken ct = default);

    /// <summary>
    /// Calculates the tier from total XP based on tier thresholds.
    /// Thresholds: 100, 250, 500, 800, 1200, 1700, 2300, 3000, 4000, 5000
    /// </summary>
    /// <param name="totalXP">The total XP amount.</param>
    /// <returns>The tier level (1-10, or 0 if below first threshold).</returns>
    int CalculateTierFromXP(int totalXP);
}
