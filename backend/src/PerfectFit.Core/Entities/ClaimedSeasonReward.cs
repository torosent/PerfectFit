namespace PerfectFit.Core.Entities;

/// <summary>
/// Tracks which season rewards have been claimed by a user.
/// </summary>
public class ClaimedSeasonReward
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int SeasonRewardId { get; private set; }
    public DateTime ClaimedAt { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public SeasonReward? SeasonReward { get; private set; }

    // Private constructor for EF Core
    private ClaimedSeasonReward() { }

    public static ClaimedSeasonReward Create(int userId, int seasonRewardId)
    {
        return new ClaimedSeasonReward
        {
            UserId = userId,
            SeasonRewardId = seasonRewardId,
            ClaimedAt = DateTime.UtcNow
        };
    }
}
