using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.UseCases.Gamification;

/// <summary>
/// Comprehensive result returned after processing all gamification updates at the end of a game.
/// </summary>
public record GameEndGamificationResult(
    StreakResult Streak,
    IReadOnlyList<ChallengeProgressResult> ChallengeUpdates,
    AchievementUnlockResult AchievementUpdates,
    SeasonXPResult SeasonProgress,
    IReadOnlyList<GoalProgressResult> GoalUpdates
);

/// <summary>
/// Comprehensive gamification status for a user.
/// </summary>
public record GamificationStatusResult(
    StreakInfo Streak,
    IReadOnlyList<ChallengeWithProgressResult> ActiveChallenges,
    IReadOnlyList<AchievementInfo> RecentAchievements,
    SeasonPassInfo? SeasonPass,
    EquippedCosmeticsInfo EquippedCosmetics,
    IReadOnlyList<PersonalGoalResult> ActiveGoals
);

/// <summary>
/// Information about a user's streak status.
/// </summary>
public record StreakInfo(
    int CurrentStreak,
    int LongestStreak,
    int FreezeTokens,
    bool IsAtRisk,
    DateTimeOffset ResetTime
);

/// <summary>
/// Challenge details with user's progress.
/// </summary>
public record ChallengeWithProgressResult(
    int ChallengeId,
    string Name,
    string Description,
    ChallengeType Type,
    int TargetValue,
    int CurrentProgress,
    int XPReward,
    DateTime EndDate,
    bool IsCompleted
);

/// <summary>
/// Achievement details with unlock status and progress.
/// </summary>
public record AchievementInfo(
    int AchievementId,
    string Name,
    string Description,
    AchievementCategory Category,
    string IconUrl,
    int Progress,
    bool IsUnlocked,
    DateTime? UnlockedAt,
    bool IsSecret
);

/// <summary>
/// Season pass details including XP, tier, and rewards.
/// </summary>
public record SeasonPassInfo(
    int SeasonId,
    string SeasonName,
    int SeasonNumber,
    int CurrentXP,
    int CurrentTier,
    DateTime EndDate,
    IReadOnlyList<SeasonRewardInfo> Rewards
);

/// <summary>
/// Season reward with claim status.
/// </summary>
public record SeasonRewardInfo(
    int RewardId,
    int Tier,
    RewardType RewardType,
    int RewardValue,
    int XPRequired,
    bool IsClaimed,
    bool CanClaim
);

/// <summary>
/// Information about a user's equipped cosmetics.
/// </summary>
public record EquippedCosmeticsInfo(
    CosmeticInfo? BoardTheme,
    CosmeticInfo? AvatarFrame,
    CosmeticInfo? Badge
);

/// <summary>
/// Basic cosmetic information.
/// </summary>
public record CosmeticInfo(
    int CosmeticId,
    string Name,
    CosmeticType Type,
    string AssetUrl,
    CosmeticRarity Rarity
);

/// <summary>
/// Personal goal details with progress.
/// </summary>
public record PersonalGoalResult(
    int GoalId,
    GoalType Type,
    string Description,
    int TargetValue,
    int CurrentValue,
    int ProgressPercentage,
    bool IsCompleted,
    DateTime? ExpiresAt
);

/// <summary>
/// Result of getting all achievements for a user.
/// </summary>
public record AchievementsResult(
    IReadOnlyList<AchievementInfo> Achievements,
    int TotalUnlocked,
    int TotalAchievements
);

/// <summary>
/// Result of getting all cosmetics with ownership status.
/// </summary>
public record CosmeticsResult(
    IReadOnlyList<CosmeticWithOwnershipResult> Cosmetics
);

/// <summary>
/// Cosmetic with ownership status.
/// </summary>
public record CosmeticWithOwnershipResult(
    int CosmeticId,
    string Name,
    string Description,
    CosmeticType Type,
    string AssetUrl,
    string PreviewUrl,
    CosmeticRarity Rarity,
    bool IsOwned,
    bool IsEquipped,
    bool IsDefault
);

/// <summary>
/// Result of getting season pass information.
/// </summary>
public record SeasonPassResult(
    SeasonPassInfo? SeasonPass,
    bool HasActiveSeason
);
