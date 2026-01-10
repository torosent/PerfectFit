using PerfectFit.Core.Enums;

namespace PerfectFit.Core.Entities;

public class Challenge
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ChallengeType Type { get; private set; }
    public int TargetValue { get; private set; }
    public int XPReward { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public ICollection<UserChallenge> UserChallenges { get; private set; } = new List<UserChallenge>();

    // Private constructor for EF Core
    private Challenge() { }

    public static Challenge Create(
        string name,
        string description,
        ChallengeType type,
        int targetValue,
        int xpReward,
        DateTime startDate,
        DateTime endDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        if (targetValue < 0)
        {
            throw new ArgumentException("Target value cannot be negative.", nameof(targetValue));
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        }

        return new Challenge
        {
            Name = name,
            Description = description,
            Type = type,
            TargetValue = targetValue,
            XPReward = xpReward,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
