namespace PerfectFit.Core.Entities;

/// <summary>
/// Archive of a user's season progress, created when a season ends.
/// </summary>
public class SeasonArchive
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int SeasonId { get; private set; }
    public int FinalXP { get; private set; }
    public int FinalTier { get; private set; }
    public DateTime ArchivedAt { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public Season? Season { get; private set; }

    // Private constructor for EF Core
    private SeasonArchive() { }

    public static SeasonArchive Create(
        int userId,
        int seasonId,
        int finalXP,
        int finalTier)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("User ID must be positive.", nameof(userId));
        }

        if (seasonId <= 0)
        {
            throw new ArgumentException("Season ID must be positive.", nameof(seasonId));
        }

        return new SeasonArchive
        {
            UserId = userId,
            SeasonId = seasonId,
            FinalXP = finalXP,
            FinalTier = finalTier,
            ArchivedAt = DateTime.UtcNow
        };
    }
}
