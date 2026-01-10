using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for cosmetics.
/// </summary>
public static class CosmeticSeedData
{
    // Cosmetic codes for consistent referencing
    public static class Codes
    {
        // Board Themes
        public const string ThemeClassic = "theme_classic";
        public const string ThemeOcean = "theme_ocean";
        public const string ThemeForest = "theme_forest";
        public const string ThemeSunset = "theme_sunset";
        public const string ThemeNightSky = "theme_night_sky";
        public const string ThemeGalaxy = "theme_galaxy";
        public const string ThemeNeon = "theme_neon";
        public const string ThemeAurora = "theme_aurora";

        // Avatar Frames
        public const string FrameBronze = "frame_bronze";
        public const string FrameSilver = "frame_silver";
        public const string FrameGold = "frame_gold";
        public const string FrameDiamond = "frame_diamond";
        public const string FrameChampion = "frame_champion";
        public const string FrameFire = "frame_fire";
        public const string FrameIce = "frame_ice";

        // Badges
        public const string BadgeRookie = "badge_rookie";
        public const string BadgePro = "badge_pro";
        public const string BadgeElite = "badge_elite";
        public const string BadgeLegend = "badge_legend";
        public const string BadgeGrandmaster = "badge_grandmaster";
        public const string BadgeStreakMaster = "badge_streak_master";
        public const string BadgeSeasonChampion = "badge_season_champion";
        public const string BadgeChallengeChampion = "badge_challenge_champion";
    }

    /// <summary>
    /// Gets all predefined cosmetics for the system.
    /// </summary>
    public static IReadOnlyList<Cosmetic> GetCosmetics()
    {
        var cosmetics = new List<Cosmetic>();

        // Board Themes
        cosmetics.AddRange(GetBoardThemes());

        // Avatar Frames
        cosmetics.AddRange(GetAvatarFrames());

        // Badges
        cosmetics.AddRange(GetBadges());

        return cosmetics;
    }

    private static IEnumerable<Cosmetic> GetBoardThemes()
    {
        return new List<Cosmetic>
        {
            Cosmetic.Create(
                code: Codes.ThemeClassic,
                name: "Classic",
                description: "The classic board theme - clean and simple",
                type: CosmeticType.BoardTheme,
                assetUrl: "/themes/classic.json",
                previewUrl: "/themes/previews/classic.png",
                rarity: CosmeticRarity.Common,
                isDefault: true),

            Cosmetic.Create(
                code: Codes.ThemeOcean,
                name: "Ocean",
                description: "Deep blue waters with wave patterns",
                type: CosmeticType.BoardTheme,
                assetUrl: "/themes/ocean.json",
                previewUrl: "/themes/previews/ocean.png",
                rarity: CosmeticRarity.Common,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.ThemeForest,
                name: "Forest",
                description: "Lush green forest with natural elements",
                type: CosmeticType.BoardTheme,
                assetUrl: "/themes/forest.json",
                previewUrl: "/themes/previews/forest.png",
                rarity: CosmeticRarity.Common,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.ThemeSunset,
                name: "Sunset",
                description: "Warm orange and pink sunset gradients",
                type: CosmeticType.BoardTheme,
                assetUrl: "/themes/sunset.json",
                previewUrl: "/themes/previews/sunset.png",
                rarity: CosmeticRarity.Rare,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.ThemeNightSky,
                name: "Night Sky",
                description: "Dark theme with stars and constellations",
                type: CosmeticType.BoardTheme,
                assetUrl: "/themes/night-sky.json",
                previewUrl: "/themes/previews/night-sky.png",
                rarity: CosmeticRarity.Rare,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.ThemeGalaxy,
                name: "Galaxy",
                description: "Cosmic theme with nebulae and galaxies",
                type: CosmeticType.BoardTheme,
                assetUrl: "/themes/galaxy.json",
                previewUrl: "/themes/previews/galaxy.png",
                rarity: CosmeticRarity.Epic,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.ThemeNeon,
                name: "Neon",
                description: "Vibrant neon colors with glow effects",
                type: CosmeticType.BoardTheme,
                assetUrl: "/themes/neon.json",
                previewUrl: "/themes/previews/neon.png",
                rarity: CosmeticRarity.Epic,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.ThemeAurora,
                name: "Aurora",
                description: "Northern lights dancing across the board",
                type: CosmeticType.BoardTheme,
                assetUrl: "/themes/aurora.json",
                previewUrl: "/themes/previews/aurora.png",
                rarity: CosmeticRarity.Legendary,
                isDefault: false),
        };
    }

    private static IEnumerable<Cosmetic> GetAvatarFrames()
    {
        return new List<Cosmetic>
        {
            Cosmetic.Create(
                code: Codes.FrameBronze,
                name: "Bronze Frame",
                description: "A simple bronze frame",
                type: CosmeticType.AvatarFrame,
                assetUrl: "/frames/bronze.svg",
                previewUrl: "/frames/previews/bronze.png",
                rarity: CosmeticRarity.Common,
                isDefault: true),

            Cosmetic.Create(
                code: Codes.FrameSilver,
                name: "Silver Frame",
                description: "A polished silver frame",
                type: CosmeticType.AvatarFrame,
                assetUrl: "/frames/silver.svg",
                previewUrl: "/frames/previews/silver.png",
                rarity: CosmeticRarity.Common,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.FrameGold,
                name: "Gold Frame",
                description: "A luxurious gold frame",
                type: CosmeticType.AvatarFrame,
                assetUrl: "/frames/gold.svg",
                previewUrl: "/frames/previews/gold.png",
                rarity: CosmeticRarity.Rare,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.FrameDiamond,
                name: "Diamond Frame",
                description: "A sparkling diamond-encrusted frame",
                type: CosmeticType.AvatarFrame,
                assetUrl: "/frames/diamond.svg",
                previewUrl: "/frames/previews/diamond.png",
                rarity: CosmeticRarity.Epic,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.FrameChampion,
                name: "Champion Frame",
                description: "Reserved for true champions",
                type: CosmeticType.AvatarFrame,
                assetUrl: "/frames/champion.svg",
                previewUrl: "/frames/previews/champion.png",
                rarity: CosmeticRarity.Legendary,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.FrameFire,
                name: "Fire Frame",
                description: "A frame wreathed in flames",
                type: CosmeticType.AvatarFrame,
                assetUrl: "/frames/fire.svg",
                previewUrl: "/frames/previews/fire.png",
                rarity: CosmeticRarity.Epic,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.FrameIce,
                name: "Ice Frame",
                description: "A crystalline ice frame",
                type: CosmeticType.AvatarFrame,
                assetUrl: "/frames/ice.svg",
                previewUrl: "/frames/previews/ice.png",
                rarity: CosmeticRarity.Epic,
                isDefault: false),
        };
    }

    private static IEnumerable<Cosmetic> GetBadges()
    {
        return new List<Cosmetic>
        {
            Cosmetic.Create(
                code: Codes.BadgeRookie,
                name: "Rookie",
                description: "Every champion starts somewhere",
                type: CosmeticType.Badge,
                assetUrl: "/badges/rookie.svg",
                previewUrl: "/badges/previews/rookie.png",
                rarity: CosmeticRarity.Common,
                isDefault: true),

            Cosmetic.Create(
                code: Codes.BadgePro,
                name: "Pro",
                description: "You're getting good at this",
                type: CosmeticType.Badge,
                assetUrl: "/badges/pro.svg",
                previewUrl: "/badges/previews/pro.png",
                rarity: CosmeticRarity.Common,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.BadgeElite,
                name: "Elite",
                description: "Top-tier performance",
                type: CosmeticType.Badge,
                assetUrl: "/badges/elite.svg",
                previewUrl: "/badges/previews/elite.png",
                rarity: CosmeticRarity.Rare,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.BadgeLegend,
                name: "Legend",
                description: "Your skills are legendary",
                type: CosmeticType.Badge,
                assetUrl: "/badges/legend.svg",
                previewUrl: "/badges/previews/legend.png",
                rarity: CosmeticRarity.Epic,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.BadgeGrandmaster,
                name: "Grandmaster",
                description: "The pinnacle of achievement",
                type: CosmeticType.Badge,
                assetUrl: "/badges/grandmaster.svg",
                previewUrl: "/badges/previews/grandmaster.png",
                rarity: CosmeticRarity.Legendary,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.BadgeStreakMaster,
                name: "Streak Master",
                description: "Awarded for exceptional streak dedication",
                type: CosmeticType.Badge,
                assetUrl: "/badges/streak-master.svg",
                previewUrl: "/badges/previews/streak-master.png",
                rarity: CosmeticRarity.Epic,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.BadgeSeasonChampion,
                name: "Season Champion",
                description: "Reached max tier in a season",
                type: CosmeticType.Badge,
                assetUrl: "/badges/season-champion.svg",
                previewUrl: "/badges/previews/season-champion.png",
                rarity: CosmeticRarity.Legendary,
                isDefault: false),

            Cosmetic.Create(
                code: Codes.BadgeChallengeChampion,
                name: "Challenge Champion",
                description: "Completed 100 challenges",
                type: CosmeticType.Badge,
                assetUrl: "/badges/challenge-champion.svg",
                previewUrl: "/badges/previews/challenge-champion.png",
                rarity: CosmeticRarity.Epic,
                isDefault: false),
        };
    }
}
