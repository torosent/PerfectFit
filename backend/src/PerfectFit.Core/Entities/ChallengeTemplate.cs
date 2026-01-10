using PerfectFit.Core.Enums;

namespace PerfectFit.Core.Entities;

/// <summary>
/// Template for generating challenges. Used by background jobs to create daily/weekly challenges.
/// </summary>
public class ChallengeTemplate
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ChallengeType Type { get; private set; }
    public int TargetValue { get; private set; }
    public int XPReward { get; private set; }
    public bool IsActive { get; private set; }

    // Private constructor for EF Core
    private ChallengeTemplate() { }

    public static ChallengeTemplate Create(
        string name,
        string description,
        ChallengeType type,
        int targetValue,
        int xpReward)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        if (targetValue < 0)
        {
            throw new ArgumentException("Target value cannot be negative.", nameof(targetValue));
        }

        if (xpReward < 0)
        {
            throw new ArgumentException("XP reward cannot be negative.", nameof(xpReward));
        }

        return new ChallengeTemplate
        {
            Name = name,
            Description = description ?? string.Empty,
            Type = type,
            TargetValue = targetValue,
            XPReward = xpReward,
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

    /// <summary>
    /// Updates the challenge template properties.
    /// </summary>
    public void Update(
        string name,
        string description,
        ChallengeType type,
        int targetValue,
        int xpReward,
        bool isActive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        if (targetValue < 0)
        {
            throw new ArgumentException("Target value cannot be negative.", nameof(targetValue));
        }

        if (xpReward < 0)
        {
            throw new ArgumentException("XP reward cannot be negative.", nameof(xpReward));
        }

        Name = name;
        Description = description ?? string.Empty;
        Type = type;
        TargetValue = targetValue;
        XPReward = xpReward;
        IsActive = isActive;
    }
}
