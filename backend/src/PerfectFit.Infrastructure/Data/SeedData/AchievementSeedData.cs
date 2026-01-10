using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for achievements.
/// </summary>
public static class AchievementSeedData
{
    /// <summary>
    /// Gets all predefined achievements for the system.
    /// </summary>
    public static IReadOnlyList<Achievement> GetAchievements()
    {
        return new List<Achievement>
        {
            // Games/Wins achievements
            Achievement.Create(
                name: "First Victory",
                description: "Win your first game",
                category: AchievementCategory.Games,
                iconUrl: "/icons/achievements/first-victory.svg",
                unlockCondition: """{"Type":"TotalWins","Value":1}""",
                rewardType: RewardType.XPBoost,
                rewardValue: 100,
                isSecret: false,
                displayOrder: 1),

            Achievement.Create(
                name: "Rising Star",
                description: "Win 10 games",
                category: AchievementCategory.Games,
                iconUrl: "/icons/achievements/rising-star.svg",
                unlockCondition: """{"Type":"TotalWins","Value":10}""",
                rewardType: RewardType.XPBoost,
                rewardValue: 250,
                isSecret: false,
                displayOrder: 2),

            Achievement.Create(
                name: "Veteran",
                description: "Win 50 games",
                category: AchievementCategory.Games,
                iconUrl: "/icons/achievements/veteran.svg",
                unlockCondition: """{"Type":"TotalWins","Value":50}""",
                rewardType: RewardType.XPBoost,
                rewardValue: 500,
                isSecret: false,
                displayOrder: 3),

            Achievement.Create(
                name: "Champion",
                description: "Win 100 games",
                category: AchievementCategory.Games,
                iconUrl: "/icons/achievements/champion.svg",
                unlockCondition: """{"Type":"TotalWins","Value":100}""",
                rewardType: RewardType.Cosmetic,
                rewardValue: 0,
                isSecret: false,
                displayOrder: 4,
                rewardCosmeticCode: CosmeticSeedData.Codes.BadgeGrandmaster),

            // Score achievements
            Achievement.Create(
                name: "Perfect Game",
                description: "Achieve a perfect score in a single game",
                category: AchievementCategory.Score,
                iconUrl: "/icons/achievements/perfect-game.svg",
                unlockCondition: """{"Type":"PerfectGames","Value":1}""",
                rewardType: RewardType.XPBoost,
                rewardValue: 500,
                isSecret: false,
                displayOrder: 10),

            Achievement.Create(
                name: "Speed Demon",
                description: "Complete a game in under 60 seconds with 90%+ accuracy",
                category: AchievementCategory.Score,
                iconUrl: "/icons/achievements/speed-demon.svg",
                unlockCondition: """{"Type":"GamesUnderTime","Value":1}""",
                rewardType: RewardType.XPBoost,
                rewardValue: 300,
                isSecret: false,
                displayOrder: 11),

            Achievement.Create(
                name: "Accuracy Master",
                description: "Complete 10 games with 95%+ accuracy",
                category: AchievementCategory.Score,
                iconUrl: "/icons/achievements/accuracy-master.svg",
                unlockCondition: """{"Type":"HighAccuracyGames","Value":10}""",
                rewardType: RewardType.XPBoost,
                rewardValue: 400,
                isSecret: false,
                displayOrder: 12),

            // Streak achievements
            Achievement.Create(
                name: "Week Warrior",
                description: "Maintain a 7-day streak",
                category: AchievementCategory.Streak,
                iconUrl: "/icons/achievements/week-warrior.svg",
                unlockCondition: """{"Type":"StreakDays","Value":7}""",
                rewardType: RewardType.StreakFreeze,
                rewardValue: 1,
                isSecret: false,
                displayOrder: 20),

            Achievement.Create(
                name: "Month Master",
                description: "Maintain a 30-day streak",
                category: AchievementCategory.Streak,
                iconUrl: "/icons/achievements/month-master.svg",
                unlockCondition: """{"Type":"StreakDays","Value":30}""",
                rewardType: RewardType.StreakFreeze,
                rewardValue: 3,
                isSecret: false,
                displayOrder: 21),

            Achievement.Create(
                name: "Century Club",
                description: "Maintain a 100-day streak",
                category: AchievementCategory.Streak,
                iconUrl: "/icons/achievements/century-club.svg",
                unlockCondition: """{"Type":"StreakDays","Value":100}""",
                rewardType: RewardType.Cosmetic,
                rewardValue: 0,
                isSecret: false,
                displayOrder: 22,
                rewardCosmeticCode: CosmeticSeedData.Codes.BadgeStreakMaster),

            // Challenge achievements
            Achievement.Create(
                name: "Challenge Accepted",
                description: "Complete your first challenge",
                category: AchievementCategory.Challenge,
                iconUrl: "/icons/achievements/challenge-accepted.svg",
                unlockCondition: """{"Type":"Challenges","Value":1}""",
                rewardType: RewardType.XPBoost,
                rewardValue: 50,
                isSecret: false,
                displayOrder: 30),

            Achievement.Create(
                name: "Challenge Hunter",
                description: "Complete 25 challenges",
                category: AchievementCategory.Challenge,
                iconUrl: "/icons/achievements/challenge-hunter.svg",
                unlockCondition: """{"Type":"Challenges","Value":25}""",
                rewardType: RewardType.XPBoost,
                rewardValue: 250,
                isSecret: false,
                displayOrder: 31),

            Achievement.Create(
                name: "Challenge Champion",
                description: "Complete 100 challenges",
                category: AchievementCategory.Challenge,
                iconUrl: "/icons/achievements/challenge-champion.svg",
                unlockCondition: """{"Type":"Challenges","Value":100}""",
                rewardType: RewardType.Cosmetic,
                rewardValue: 0,
                isSecret: false,
                displayOrder: 32,
                rewardCosmeticCode: CosmeticSeedData.Codes.BadgeChallengeChampion),

            // Special achievements
            Achievement.Create(
                name: "Season Champion",
                description: "Reach the maximum tier in a season",
                category: AchievementCategory.Special,
                iconUrl: "/icons/achievements/season-champion.svg",
                unlockCondition: """{"Type":"SeasonTier","Value":10}""",
                rewardType: RewardType.Cosmetic,
                rewardValue: 0,
                isSecret: false,
                displayOrder: 40,
                rewardCosmeticCode: CosmeticSeedData.Codes.BadgeSeasonChampion),

            Achievement.Create(
                name: "Collector",
                description: "Unlock all cosmetics in a category",
                category: AchievementCategory.Special,
                iconUrl: "/icons/achievements/collector.svg",
                unlockCondition: """{"Type":"CosmeticsInCategory","Value":8}""",
                rewardType: RewardType.XPBoost,
                rewardValue: 1000,
                isSecret: false,
                displayOrder: 41),

            // Secret achievements
            Achievement.Create(
                name: "Night Owl",
                description: "Play 10 games between midnight and 4 AM",
                category: AchievementCategory.Special,
                iconUrl: "/icons/achievements/night-owl.svg",
                unlockCondition: """{"Type":"NightGames","Value":10}""",
                rewardType: RewardType.XPBoost,
                rewardValue: 200,
                isSecret: true,
                displayOrder: 50),

            Achievement.Create(
                name: "Lucky Seven",
                description: "Win 7 games in a row",
                category: AchievementCategory.Games,
                iconUrl: "/icons/achievements/lucky-seven.svg",
                unlockCondition: """{"Type":"WinStreak","Value":7}""",
                rewardType: RewardType.XPBoost,
                rewardValue: 350,
                isSecret: true,
                displayOrder: 51),
        };
    }
}
