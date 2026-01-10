using PerfectFit.Core.Entities;

namespace PerfectFit.Core.Services.Results;

/// <summary>
/// Result of checking and unlocking achievements.
/// </summary>
/// <param name="UnlockedAchievements">List of achievements that were unlocked.</param>
/// <param name="TotalRewardsGranted">Total value of rewards granted from all unlocked achievements.</param>
public record AchievementUnlockResult(
    IReadOnlyList<Achievement> UnlockedAchievements,
    int TotalRewardsGranted
);
