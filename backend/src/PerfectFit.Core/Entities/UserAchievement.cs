namespace PerfectFit.Core.Entities;

public class UserAchievement
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int AchievementId { get; private set; }
    public DateTime? UnlockedAt { get; private set; }
    public int Progress { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public Achievement? Achievement { get; private set; }

    // Computed property
    public bool IsUnlocked => UnlockedAt.HasValue;

    // Private constructor for EF Core
    private UserAchievement() { }

    public static UserAchievement Create(int userId, int achievementId)
    {
        return new UserAchievement
        {
            UserId = userId,
            AchievementId = achievementId,
            Progress = 0,
            UnlockedAt = null
        };
    }

    public void UpdateProgress(int progress)
    {
        Progress = Math.Min(progress, 100);
    }

    public void Unlock()
    {
        if (UnlockedAt.HasValue)
        {
            return; // Already unlocked
        }

        Progress = 100;
        UnlockedAt = DateTime.UtcNow;
    }
}
