using PerfectFit.Core.Enums;

namespace PerfectFit.Core.Entities;

public class Achievement
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AchievementCategory Category { get; private set; }
    public string IconUrl { get; private set; } = string.Empty;
    public string UnlockCondition { get; private set; } = string.Empty;
    public RewardType RewardType { get; private set; }
    public int RewardValue { get; private set; }
    public bool IsSecret { get; private set; }
    public int DisplayOrder { get; private set; }
    
    /// <summary>
    /// Optional code reference for cosmetic rewards. Used instead of hard-coded IDs.
    /// </summary>
    public string? RewardCosmeticCode { get; private set; }

    // Navigation properties
    public ICollection<UserAchievement> UserAchievements { get; private set; } = new List<UserAchievement>();

    // Private constructor for EF Core
    private Achievement() { }

    public static Achievement Create(
        string name,
        string description,
        AchievementCategory category,
        string iconUrl,
        string unlockCondition,
        RewardType rewardType,
        int rewardValue,
        bool isSecret = false,
        int displayOrder = 0,
        string? rewardCosmeticCode = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        return new Achievement
        {
            Name = name,
            Description = description,
            Category = category,
            IconUrl = iconUrl,
            UnlockCondition = unlockCondition,
            RewardType = rewardType,
            RewardValue = rewardValue,
            IsSecret = isSecret,
            DisplayOrder = displayOrder,
            RewardCosmeticCode = rewardCosmeticCode
        };
    }

    /// <summary>
    /// Updates the achievement properties.
    /// </summary>
    public void Update(
        string name,
        string description,
        AchievementCategory category,
        string iconUrl,
        string unlockCondition,
        RewardType rewardType,
        int rewardValue,
        bool isSecret,
        int displayOrder,
        string? rewardCosmeticCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        Name = name;
        Description = description;
        Category = category;
        IconUrl = iconUrl;
        UnlockCondition = unlockCondition;
        RewardType = rewardType;
        RewardValue = rewardValue;
        IsSecret = isSecret;
        DisplayOrder = displayOrder;
        RewardCosmeticCode = rewardCosmeticCode;
    }
}
