using PerfectFit.Core.Enums;

namespace PerfectFit.Web.DTOs;

#region Response DTOs

/// <summary>
/// Comprehensive gamification status for a user.
/// </summary>
public record GamificationStatusDto(
    StreakDto Streak,
    IReadOnlyList<ChallengeDto> ActiveChallenges,
    IReadOnlyList<AchievementDto> RecentAchievements,
    SeasonPassInfoDto? SeasonPass,
    EquippedCosmeticsDto EquippedCosmetics,
    IReadOnlyList<PersonalGoalDto> ActiveGoals
);

/// <summary>
/// User streak information.
/// </summary>
public record StreakDto(
    int CurrentStreak,
    int LongestStreak,
    int? FreezeTokens,
    bool? IsAtRisk,
    DateTimeOffset? ResetTime
);

/// <summary>
/// Challenge details with user's progress.
/// </summary>
public record ChallengeDto(
    int Id,
    string Name,
    string Description,
    string Type,
    int TargetValue,
    int CurrentProgress,
    int XPReward,
    bool IsCompleted,
    DateTime EndsAt
);

/// <summary>
/// Achievement details with unlock status.
/// </summary>
public record AchievementDto(
    int Id,
    string Name,
    string Description,
    string Category,
    string IconUrl,
    bool IsUnlocked,
    int Progress,
    DateTime? UnlockedAt,
    bool IsSecret
);

/// <summary>
/// Result of getting all achievements for a user.
/// </summary>
public record AchievementsDto(
    IReadOnlyList<AchievementDto> Achievements,
    int TotalUnlocked,
    int TotalAchievements
);

/// <summary>
/// Season pass details including XP, tier, and rewards.
/// </summary>
public record SeasonPassInfoDto(
    int SeasonId,
    string SeasonName,
    int SeasonNumber,
    int CurrentXP,
    int CurrentTier,
    DateTime EndsAt,
    IReadOnlyList<SeasonRewardDto> Rewards
);

/// <summary>
/// Season pass result wrapper.
/// </summary>
public record SeasonPassDto(
    SeasonPassInfoDto? SeasonPass,
    bool HasActiveSeason
);

/// <summary>
/// Season reward with claim status.
/// </summary>
public record SeasonRewardDto(
    int Id,
    int Tier,
    string RewardType,
    int RewardValue,
    int XPRequired,
    bool IsClaimed,
    bool CanClaim
);

/// <summary>
/// Cosmetic item with ownership and equipped status.
/// </summary>
public record CosmeticDto(
    int Id,
    string Name,
    string Description,
    string Type,
    string Rarity,
    string AssetUrl,
    string PreviewUrl,
    bool IsOwned,
    bool IsEquipped,
    bool IsDefault
);

/// <summary>
/// Result of getting all cosmetics for a user.
/// </summary>
public record CosmeticsDto(
    IReadOnlyList<CosmeticDto> Cosmetics
);

/// <summary>
/// Information about a user's equipped cosmetics.
/// </summary>
public record EquippedCosmeticsDto(
    CosmeticInfoDto? BoardTheme,
    CosmeticInfoDto? AvatarFrame,
    CosmeticInfoDto? Badge
);

/// <summary>
/// Basic cosmetic information for equipped items.
/// </summary>
public record CosmeticInfoDto(
    int Id,
    string Name,
    string Type,
    string AssetUrl,
    string Rarity
);

/// <summary>
/// Personal goal details with progress.
/// </summary>
public record PersonalGoalDto(
    int Id,
    string Type,
    string Description,
    int TargetValue,
    int CurrentValue,
    int ProgressPercentage,
    bool IsCompleted,
    DateTime? ExpiresAt
);

#endregion

#region Request DTOs

/// <summary>
/// Request to equip a cosmetic item.
/// </summary>
public record EquipCosmeticRequest(Guid CosmeticId);

/// <summary>
/// Request to claim a season reward.
/// </summary>
public record ClaimRewardRequest(Guid RewardId);

/// <summary>
/// Request to set user timezone.
/// </summary>
public record SetTimezoneRequest(string Timezone);

#endregion

#region Command Response DTOs

/// <summary>
/// Response for using a streak freeze.
/// </summary>
public record UseStreakFreezeResponseDto(
    bool Success,
    string? ErrorMessage = null
);

/// <summary>
/// Response for equipping a cosmetic.
/// </summary>
public record EquipCosmeticResponseDto(
    bool Success,
    string? ErrorMessage = null
);

/// <summary>
/// Response for claiming a season reward.
/// </summary>
public record ClaimRewardResponseDto(
    bool Success,
    string? RewardType = null,
    int? RewardValue = null,
    string? ErrorMessage = null
);

/// <summary>
/// Response for setting timezone.
/// </summary>
public record SetTimezoneResponseDto(
    bool Success,
    string? ErrorMessage = null
);

#endregion

#region Game End Gamification DTOs

/// <summary>
/// Gamification updates returned after a game ends.
/// </summary>
public record GameEndGamificationDto(
    StreakDto Streak,
    IReadOnlyList<ChallengeProgressDto> ChallengeUpdates,
    IReadOnlyList<AchievementUnlockDto> NewAchievements,
    SeasonXPDto SeasonProgress,
    IReadOnlyList<GoalProgressDto> GoalUpdates
);

/// <summary>
/// Challenge progress update.
/// </summary>
public record ChallengeProgressDto(
    int ChallengeId,
    string ChallengeName,
    int NewProgress,
    bool JustCompleted,
    int? XPEarned
);

/// <summary>
/// Achievement unlock notification.
/// </summary>
public record AchievementUnlockDto(
    int AchievementId,
    string Name,
    string Description,
    string IconUrl,
    string RewardType,
    int RewardValue
);

/// <summary>
/// Season XP progress update.
/// </summary>
public record SeasonXPDto(
    int XPEarned,
    int TotalXP,
    int NewTier,
    bool TierUp,
    int NewRewardsCount
);

/// <summary>
/// Personal goal progress update.
/// </summary>
public record GoalProgressDto(
    int GoalId,
    string Description,
    int NewProgress,
    bool JustCompleted
);

#endregion
