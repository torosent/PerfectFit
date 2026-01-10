using PerfectFit.Core.Enums;

namespace PerfectFit.Web.DTOs;

#region Achievement DTOs

/// <summary>
/// Admin response DTO for achievements with full details.
/// </summary>
/// <param name="Id">The unique identifier of the achievement.</param>
/// <param name="Name">The display name of the achievement.</param>
/// <param name="Description">The description explaining how to unlock the achievement.</param>
/// <param name="Category">The achievement category (Score, Streak, Games, Challenge, Special).</param>
/// <param name="IconUrl">URL to the achievement icon.</param>
/// <param name="UnlockCondition">JSON string defining the unlock conditions.</param>
/// <param name="RewardType">The type of reward granted (Cosmetic, StreakFreeze, XPBoost).</param>
/// <param name="RewardValue">The value/amount of the reward.</param>
/// <param name="IsSecret">Whether this achievement is hidden until unlocked.</param>
/// <param name="DisplayOrder">The order in which to display this achievement.</param>
/// <param name="RewardCosmeticCode">Optional cosmetic code for cosmetic rewards.</param>
public record AdminAchievementDto(
    int Id,
    string Name,
    string Description,
    string Category,
    string IconUrl,
    string UnlockCondition,
    string RewardType,
    int RewardValue,
    bool IsSecret,
    int DisplayOrder,
    string? RewardCosmeticCode
);

/// <summary>
/// Request to create a new achievement.
/// </summary>
/// <param name="Name">The display name of the achievement.</param>
/// <param name="Description">The description explaining how to unlock the achievement.</param>
/// <param name="Category">The achievement category.</param>
/// <param name="IconUrl">URL to the achievement icon.</param>
/// <param name="UnlockCondition">JSON string defining the unlock conditions (validated at endpoint).</param>
/// <param name="RewardType">The type of reward granted.</param>
/// <param name="RewardValue">The value/amount of the reward.</param>
/// <param name="IsSecret">Whether this achievement is hidden until unlocked.</param>
/// <param name="DisplayOrder">The order in which to display this achievement.</param>
/// <param name="RewardCosmeticCode">Optional cosmetic code for cosmetic rewards.</param>
public record CreateAchievementRequest(
    string Name,
    string Description,
    AchievementCategory Category,
    string IconUrl,
    string UnlockCondition,
    RewardType RewardType,
    int RewardValue,
    bool IsSecret = false,
    int DisplayOrder = 0,
    string? RewardCosmeticCode = null
);

/// <summary>
/// Request to update an existing achievement.
/// </summary>
/// <param name="Name">The display name of the achievement.</param>
/// <param name="Description">The description explaining how to unlock the achievement.</param>
/// <param name="Category">The achievement category.</param>
/// <param name="IconUrl">URL to the achievement icon.</param>
/// <param name="UnlockCondition">JSON string defining the unlock conditions (validated at endpoint).</param>
/// <param name="RewardType">The type of reward granted.</param>
/// <param name="RewardValue">The value/amount of the reward.</param>
/// <param name="IsSecret">Whether this achievement is hidden until unlocked.</param>
/// <param name="DisplayOrder">The order in which to display this achievement.</param>
/// <param name="RewardCosmeticCode">Optional cosmetic code for cosmetic rewards.</param>
public record UpdateAchievementRequest(
    string Name,
    string Description,
    AchievementCategory Category,
    string IconUrl,
    string UnlockCondition,
    RewardType RewardType,
    int RewardValue,
    bool IsSecret,
    int DisplayOrder,
    string? RewardCosmeticCode = null
);

#endregion

#region Challenge Template DTOs

/// <summary>
/// Admin response DTO for challenge templates with full details.
/// </summary>
/// <param name="Id">The unique identifier of the challenge template.</param>
/// <param name="Name">The display name of the challenge.</param>
/// <param name="Description">The description of the challenge.</param>
/// <param name="Type">The challenge type (Daily, Weekly).</param>
/// <param name="TargetValue">The target value to complete the challenge.</param>
/// <param name="XPReward">The XP reward for completing the challenge.</param>
/// <param name="IsActive">Whether this template is active and can generate challenges.</param>
public record AdminChallengeTemplateDto(
    int Id,
    string Name,
    string Description,
    string Type,
    int TargetValue,
    int XPReward,
    bool IsActive
);

/// <summary>
/// Request to create a new challenge template.
/// </summary>
/// <param name="Name">The display name of the challenge.</param>
/// <param name="Description">The description of the challenge.</param>
/// <param name="Type">The challenge type.</param>
/// <param name="TargetValue">The target value to complete the challenge.</param>
/// <param name="XPReward">The XP reward for completing the challenge.</param>
public record CreateChallengeTemplateRequest(
    string Name,
    string Description,
    ChallengeType Type,
    int TargetValue,
    int XPReward
);

/// <summary>
/// Request to update an existing challenge template.
/// </summary>
/// <param name="Name">The display name of the challenge.</param>
/// <param name="Description">The description of the challenge.</param>
/// <param name="Type">The challenge type.</param>
/// <param name="TargetValue">The target value to complete the challenge.</param>
/// <param name="XPReward">The XP reward for completing the challenge.</param>
/// <param name="IsActive">Whether this template is active and can generate challenges.</param>
public record UpdateChallengeTemplateRequest(
    string Name,
    string Description,
    ChallengeType Type,
    int TargetValue,
    int XPReward,
    bool IsActive
);

#endregion

#region Cosmetic DTOs

/// <summary>
/// Admin response DTO for cosmetics with full details.
/// </summary>
/// <param name="Id">The unique identifier of the cosmetic.</param>
/// <param name="Code">The unique code for the cosmetic (used for references).</param>
/// <param name="Name">The display name of the cosmetic.</param>
/// <param name="Description">The description of the cosmetic.</param>
/// <param name="Type">The cosmetic type (BoardTheme, AvatarFrame, Badge).</param>
/// <param name="AssetUrl">URL to the cosmetic asset.</param>
/// <param name="PreviewUrl">URL to the cosmetic preview image.</param>
/// <param name="Rarity">The cosmetic rarity (Common, Rare, Epic, Legendary).</param>
/// <param name="IsDefault">Whether this cosmetic is available to all users by default.</param>
public record AdminCosmeticDto(
    int Id,
    string Code,
    string Name,
    string Description,
    string Type,
    string AssetUrl,
    string PreviewUrl,
    string Rarity,
    bool IsDefault
);

/// <summary>
/// Request to create a new cosmetic.
/// </summary>
/// <param name="Code">The unique code for the cosmetic (used for references).</param>
/// <param name="Name">The display name of the cosmetic.</param>
/// <param name="Description">The description of the cosmetic.</param>
/// <param name="Type">The cosmetic type.</param>
/// <param name="AssetUrl">URL to the cosmetic asset.</param>
/// <param name="PreviewUrl">URL to the cosmetic preview image.</param>
/// <param name="Rarity">The cosmetic rarity.</param>
/// <param name="IsDefault">Whether this cosmetic is available to all users by default.</param>
public record CreateCosmeticRequest(
    string Code,
    string Name,
    string Description,
    CosmeticType Type,
    string AssetUrl,
    string PreviewUrl,
    CosmeticRarity Rarity,
    bool IsDefault = false
);

/// <summary>
/// Request to update an existing cosmetic.
/// </summary>
/// <param name="Code">The unique code for the cosmetic (used for references).</param>
/// <param name="Name">The display name of the cosmetic.</param>
/// <param name="Description">The description of the cosmetic.</param>
/// <param name="Type">The cosmetic type.</param>
/// <param name="AssetUrl">URL to the cosmetic asset.</param>
/// <param name="PreviewUrl">URL to the cosmetic preview image.</param>
/// <param name="Rarity">The cosmetic rarity.</param>
/// <param name="IsDefault">Whether this cosmetic is available to all users by default.</param>
public record UpdateCosmeticRequest(
    string Code,
    string Name,
    string Description,
    CosmeticType Type,
    string AssetUrl,
    string PreviewUrl,
    CosmeticRarity Rarity,
    bool IsDefault
);

#endregion

#region Common Response DTOs

/// <summary>
/// Response when deletion is prevented because the entity is in use.
/// </summary>
/// <param name="Message">A human-readable message explaining why deletion was prevented.</param>
/// <param name="EntityType">The type of entity that cannot be deleted.</param>
/// <param name="EntityId">The ID of the entity that cannot be deleted.</param>
/// <param name="UsageCount">The number of places where this entity is in use.</param>
/// <param name="UsageDetails">Optional details about where the entity is in use.</param>
public record EntityInUseResponse(
    string Message,
    string EntityType,
    int EntityId,
    int UsageCount,
    string? UsageDetails = null
);

#endregion
