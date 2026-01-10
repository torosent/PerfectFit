namespace PerfectFit.Core.Entities;

public class Season
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Number { get; private set; }
    public string Theme { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public ICollection<SeasonReward> Rewards { get; private set; } = new List<SeasonReward>();

    // Private constructor for EF Core
    private Season() { }

    public static Season Create(
        string name,
        int number,
        string theme,
        DateTime startDate,
        DateTime endDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        if (number < 1)
        {
            throw new ArgumentException("Season number must be at least 1.", nameof(number));
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        }

        return new Season
        {
            Name = name,
            Number = number,
            Theme = theme,
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
