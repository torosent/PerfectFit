using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for seasons and their rewards.
/// </summary>
public static class SeasonSeedData
{
    // XP thresholds for each tier (tiers 1-10)
    // Tier 1: 100 XP, Tier 2: 250 XP, etc.
    private static readonly int[] TierThresholds = { 100, 250, 500, 800, 1200, 1700, 2300, 3000, 4000, 5000 };

    // Mapping of tier to cosmetic code for cosmetic rewards
    // These codes match the CosmeticSeedData.Codes constants
    private static readonly Dictionary<int, string> TierCosmeticCodes = new()
    {
        { 3, CosmeticSeedData.Codes.ThemeOcean },        // Tier 3: Ocean theme
        { 6, CosmeticSeedData.Codes.FrameSilver },       // Tier 6: Silver frame
        { 8, CosmeticSeedData.Codes.BadgeElite },        // Tier 8: Elite badge
        { 10, CosmeticSeedData.Codes.BadgeSeasonChampion } // Tier 10: Season Champion badge
    };

    /// <summary>
    /// Creates an initial season with the specified parameters.
    /// </summary>
    /// <param name="seasonNumber">The season number (1, 2, 3, etc.)</param>
    /// <param name="startDate">When the season starts.</param>
    /// <param name="durationDays">How long the season lasts in days (default 90).</param>
    public static Season CreateSeason(int seasonNumber, DateTime startDate, int durationDays = 90)
    {
        var seasonName = GetSeasonName(seasonNumber);
        var theme = GetSeasonTheme(seasonNumber);
        var endDate = startDate.AddDays(durationDays);

        return Season.Create(seasonName, seasonNumber, theme, startDate, endDate);
    }

    /// <summary>
    /// Creates rewards for a season using cosmetic codes for resolution.
    /// The cosmetic codes will be resolved to IDs at runtime.
    /// </summary>
    /// <param name="seasonId">The season's ID (will be set after season is saved).</param>
    /// <param name="cosmeticCodeResolver">Function that resolves cosmetic code to ID.</param>
    public static IReadOnlyList<SeasonReward> CreateSeasonRewards(int seasonId, Func<string, int>? cosmeticCodeResolver = null)
    {
        var rewards = new List<SeasonReward>();

        // Tier 1 (100 XP) - Streak Freeze
        rewards.Add(SeasonReward.Create(seasonId, 1, RewardType.StreakFreeze, 1, TierThresholds[0]));

        // Tier 2 (250 XP) - XP Boost
        rewards.Add(SeasonReward.Create(seasonId, 2, RewardType.XPBoost, 100, TierThresholds[1]));

        // Tier 3 (500 XP) - Cosmetic (Board Theme - Ocean)
        var tier3CosmeticId = GetCosmeticId(TierCosmeticCodes[3], cosmeticCodeResolver);
        rewards.Add(SeasonReward.Create(seasonId, 3, RewardType.Cosmetic, tier3CosmeticId, TierThresholds[2]));

        // Tier 4 (800 XP) - Streak Freeze x2
        rewards.Add(SeasonReward.Create(seasonId, 4, RewardType.StreakFreeze, 2, TierThresholds[3]));

        // Tier 5 (1200 XP) - XP Boost
        rewards.Add(SeasonReward.Create(seasonId, 5, RewardType.XPBoost, 200, TierThresholds[4]));

        // Tier 6 (1700 XP) - Cosmetic (Avatar Frame - Silver)
        var tier6CosmeticId = GetCosmeticId(TierCosmeticCodes[6], cosmeticCodeResolver);
        rewards.Add(SeasonReward.Create(seasonId, 6, RewardType.Cosmetic, tier6CosmeticId, TierThresholds[5]));

        // Tier 7 (2300 XP) - Streak Freeze x3
        rewards.Add(SeasonReward.Create(seasonId, 7, RewardType.StreakFreeze, 3, TierThresholds[6]));

        // Tier 8 (3000 XP) - Cosmetic (Badge - Elite)
        var tier8CosmeticId = GetCosmeticId(TierCosmeticCodes[8], cosmeticCodeResolver);
        rewards.Add(SeasonReward.Create(seasonId, 8, RewardType.Cosmetic, tier8CosmeticId, TierThresholds[7]));

        // Tier 9 (4000 XP) - XP Boost + Streak Freeze
        rewards.Add(SeasonReward.Create(seasonId, 9, RewardType.XPBoost, 500, TierThresholds[8]));

        // Tier 10 (5000 XP) - Legendary Cosmetic (Season Champion Badge)
        var tier10CosmeticId = GetCosmeticId(TierCosmeticCodes[10], cosmeticCodeResolver);
        rewards.Add(SeasonReward.Create(seasonId, 10, RewardType.Cosmetic, tier10CosmeticId, TierThresholds[9]));

        return rewards;
    }

    /// <summary>
    /// Gets the cosmetic code for a specific tier (for tiers that reward cosmetics).
    /// </summary>
    /// <param name="tier">The tier number (1-10).</param>
    /// <returns>The cosmetic code or null if the tier doesn't reward a cosmetic.</returns>
    public static string? GetCosmeticCodeForTier(int tier)
    {
        return TierCosmeticCodes.TryGetValue(tier, out var code) ? code : null;
    }

    /// <summary>
    /// Gets the default initial season with rewards factory.
    /// The season starts from the provided date.
    /// </summary>
    public static (Season Season, Func<int, Func<string, int>?, IReadOnlyList<SeasonReward>> RewardsFactory) GetInitialSeason(DateTime? startDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.Date;
        var season = CreateSeason(1, start, 90);
        
        return (season, CreateSeasonRewards);
    }

    /// <summary>
    /// Creates multiple seasons for testing or initial setup.
    /// </summary>
    /// <param name="count">Number of seasons to create.</param>
    /// <param name="firstSeasonStart">When the first season starts.</param>
    /// <param name="durationDays">Duration of each season.</param>
    public static IReadOnlyList<Season> CreateMultipleSeasons(int count, DateTime firstSeasonStart, int durationDays = 90)
    {
        var seasons = new List<Season>();
        var currentStart = firstSeasonStart;

        for (int i = 1; i <= count; i++)
        {
            var season = CreateSeason(i, currentStart, durationDays);
            
            // Only activate the first season if it's current
            if (i > 1 || currentStart > DateTime.UtcNow)
            {
                season.Deactivate();
            }

            seasons.Add(season);
            currentStart = currentStart.AddDays(durationDays);
        }

        return seasons;
    }

    private static string GetSeasonName(int seasonNumber)
    {
        var suffix = seasonNumber switch
        {
            1 => "Origins",
            2 => "Ascension",
            3 => "Conquest",
            4 => "Legends",
            5 => "Champions",
            _ => $"Season {seasonNumber}"
        };

        return $"Season {seasonNumber}: {suffix}";
    }

    private static string GetSeasonTheme(int seasonNumber)
    {
        return seasonNumber switch
        {
            1 => "origins",
            2 => "fire",
            3 => "ice",
            4 => "galaxy",
            5 => "champions",
            _ => "default"
        };
    }

    /// <summary>
    /// Gets the cosmetic ID from a code using the resolver.
    /// Throws if the cosmetic code is not found to prevent invalid seed data.
    /// </summary>
    /// <param name="code">The cosmetic code to resolve.</param>
    /// <param name="cosmeticCodeResolver">Function that resolves cosmetic code to ID.</param>
    /// <returns>The cosmetic ID.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the cosmetic code is not found.</exception>
    private static int GetCosmeticId(string code, Func<string, int>? cosmeticCodeResolver)
    {
        if (cosmeticCodeResolver == null)
        {
            throw new InvalidOperationException($"Cosmetic code resolver is required to resolve cosmetic code '{code}'.");
        }

        var id = cosmeticCodeResolver(code);
        if (id <= 0)
        {
            throw new InvalidOperationException($"Cosmetic with code '{code}' not found. Ensure cosmetics are seeded first.");
        }

        return id;
    }
}
