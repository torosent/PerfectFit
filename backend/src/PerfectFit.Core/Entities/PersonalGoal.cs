using PerfectFit.Core.Enums;

namespace PerfectFit.Core.Entities;

public class PersonalGoal
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public GoalType Type { get; private set; }
    public int TargetValue { get; private set; }
    public int CurrentValue { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public bool IsCompleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    // Navigation properties
    public User? User { get; private set; }

    // Computed property
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    // Private constructor for EF Core
    private PersonalGoal() { }

    public static PersonalGoal Create(
        int userId,
        GoalType type,
        int targetValue,
        string description,
        DateTime? expiresAt)
    {
        if (targetValue < 0)
        {
            throw new ArgumentException("Target value cannot be negative.", nameof(targetValue));
        }

        return new PersonalGoal
        {
            UserId = userId,
            Type = type,
            TargetValue = targetValue,
            CurrentValue = 0,
            Description = description ?? string.Empty,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };
    }

    public void UpdateProgress(int value)
    {
        CurrentValue = value;
        if (CurrentValue >= TargetValue)
        {
            IsCompleted = true;
        }
    }
}
