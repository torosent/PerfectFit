using PerfectFit.Core.Enums;

namespace PerfectFit.Core.Entities;

public class SeasonReward
{
    public int Id { get; private set; }
    public int SeasonId { get; private set; }
    public int Tier { get; private set; }
    public RewardType RewardType { get; private set; }
    public int RewardValue { get; private set; }
    public int XPRequired { get; private set; }

    // Navigation properties
    public Season? Season { get; private set; }

    // Private constructor for EF Core
    private SeasonReward() { }

    public static SeasonReward Create(
        int seasonId,
        int tier,
        RewardType rewardType,
        int rewardValue,
        int xpRequired)
    {
        if (tier < 1 || tier > 10)
        {
            throw new ArgumentException("Tier must be between 1 and 10.", nameof(tier));
        }

        if (xpRequired < 0)
        {
            throw new ArgumentException("XP required cannot be negative.", nameof(xpRequired));
        }

        return new SeasonReward
        {
            SeasonId = seasonId,
            Tier = tier,
            RewardType = rewardType,
            RewardValue = rewardValue,
            XPRequired = xpRequired
        };
    }
}
