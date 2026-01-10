using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for challenge templates.
/// These templates are used by background jobs to generate daily/weekly challenges.
/// </summary>
public static class ChallengeSeedData
{
    /// <summary>
    /// Gets all daily challenge templates.
    /// </summary>
    public static IReadOnlyList<ChallengeTemplate> GetDailyTemplates()
    {
        return new List<ChallengeTemplate>
        {
            ChallengeTemplate.Create(
                name: "Daily Grind",
                description: "Complete 3 games today",
                type: ChallengeType.Daily,
                targetValue: 3,
                xpReward: 50),

            ChallengeTemplate.Create(
                name: "Win Streak",
                description: "Win 2 games today",
                type: ChallengeType.Daily,
                targetValue: 2,
                xpReward: 75),

            ChallengeTemplate.Create(
                name: "Score Hunter",
                description: "Score 500 total points today",
                type: ChallengeType.Daily,
                targetValue: 500,
                xpReward: 60),

            ChallengeTemplate.Create(
                name: "Precision Player",
                description: "Complete 2 games with 90%+ accuracy",
                type: ChallengeType.Daily,
                targetValue: 2,
                xpReward: 100),

            ChallengeTemplate.Create(
                name: "Quick Play",
                description: "Complete 5 games today",
                type: ChallengeType.Daily,
                targetValue: 5,
                xpReward: 80),

            ChallengeTemplate.Create(
                name: "High Score",
                description: "Score at least 200 points in a single game",
                type: ChallengeType.Daily,
                targetValue: 200,
                xpReward: 75),

            ChallengeTemplate.Create(
                name: "Flawless",
                description: "Complete a game with 100% accuracy",
                type: ChallengeType.Daily,
                targetValue: 1,
                xpReward: 150),

            ChallengeTemplate.Create(
                name: "Speed Run",
                description: "Complete a game in under 90 seconds",
                type: ChallengeType.Daily,
                targetValue: 1,
                xpReward: 100),

            ChallengeTemplate.Create(
                name: "Consistent Player",
                description: "Score 100+ points in 3 consecutive games",
                type: ChallengeType.Daily,
                targetValue: 3,
                xpReward: 90),

            ChallengeTemplate.Create(
                name: "Marathon",
                description: "Play for a total of 15 minutes today",
                type: ChallengeType.Daily,
                targetValue: 15,
                xpReward: 70),
        };
    }

    /// <summary>
    /// Gets all weekly challenge templates.
    /// </summary>
    public static IReadOnlyList<ChallengeTemplate> GetWeeklyTemplates()
    {
        return new List<ChallengeTemplate>
        {
            ChallengeTemplate.Create(
                name: "Weekly Warrior",
                description: "Win 25 games this week",
                type: ChallengeType.Weekly,
                targetValue: 25,
                xpReward: 300),

            ChallengeTemplate.Create(
                name: "Point Accumulator",
                description: "Score 5000 total points this week",
                type: ChallengeType.Weekly,
                targetValue: 5000,
                xpReward: 350),

            ChallengeTemplate.Create(
                name: "Dedication",
                description: "Play every day this week (7 days)",
                type: ChallengeType.Weekly,
                targetValue: 7,
                xpReward: 400),

            ChallengeTemplate.Create(
                name: "Perfectionist",
                description: "Complete 5 games with 95%+ accuracy",
                type: ChallengeType.Weekly,
                targetValue: 5,
                xpReward: 350),

            ChallengeTemplate.Create(
                name: "Game Master",
                description: "Complete 50 games this week",
                type: ChallengeType.Weekly,
                targetValue: 50,
                xpReward: 400),

            ChallengeTemplate.Create(
                name: "Speed Demon Weekly",
                description: "Complete 10 games in under 2 minutes each",
                type: ChallengeType.Weekly,
                targetValue: 10,
                xpReward: 350),

            ChallengeTemplate.Create(
                name: "High Achiever",
                description: "Score 300+ points in 10 games",
                type: ChallengeType.Weekly,
                targetValue: 10,
                xpReward: 375),

            ChallengeTemplate.Create(
                name: "Endurance",
                description: "Play for a total of 2 hours this week",
                type: ChallengeType.Weekly,
                targetValue: 120,
                xpReward: 300),

            ChallengeTemplate.Create(
                name: "Win Streak Master",
                description: "Win 5 games in a row",
                type: ChallengeType.Weekly,
                targetValue: 5,
                xpReward: 500),

            ChallengeTemplate.Create(
                name: "Perfect Week",
                description: "Complete all daily challenges this week",
                type: ChallengeType.Weekly,
                targetValue: 7,
                xpReward: 600),
        };
    }

    /// <summary>
    /// Gets all challenge templates (both daily and weekly).
    /// </summary>
    public static IReadOnlyList<ChallengeTemplate> GetAllTemplates()
    {
        var templates = new List<ChallengeTemplate>();
        templates.AddRange(GetDailyTemplates());
        templates.AddRange(GetWeeklyTemplates());
        return templates;
    }
}
