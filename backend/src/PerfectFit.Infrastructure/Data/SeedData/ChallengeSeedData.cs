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
                xpReward: 50,
                goalType: ChallengeGoalType.GameCount),

            ChallengeTemplate.Create(
                name: "Warm Up",
                description: "Complete 1 game today",
                type: ChallengeType.Daily,
                targetValue: 1,
                xpReward: 25,
                goalType: ChallengeGoalType.GameCount),

            ChallengeTemplate.Create(
                name: "Score Hunter",
                description: "Score 500 total points today",
                type: ChallengeType.Daily,
                targetValue: 500,
                xpReward: 60,
                goalType: ChallengeGoalType.ScoreTotal),

            ChallengeTemplate.Create(
                name: "Point Collector",
                description: "Score 1000 total points today",
                type: ChallengeType.Daily,
                targetValue: 1000,
                xpReward: 100,
                goalType: ChallengeGoalType.ScoreTotal),

            ChallengeTemplate.Create(
                name: "Quick Play",
                description: "Complete 5 games today",
                type: ChallengeType.Daily,
                targetValue: 5,
                xpReward: 80,
                goalType: ChallengeGoalType.GameCount),

            ChallengeTemplate.Create(
                name: "High Scorer",
                description: "Score at least 200 points in a single game",
                type: ChallengeType.Daily,
                targetValue: 200,
                xpReward: 75,
                goalType: ChallengeGoalType.ScoreSingleGame),

            ChallengeTemplate.Create(
                name: "Triple Threat",
                description: "Score at least 100 points in 3 games",
                type: ChallengeType.Daily,
                targetValue: 3,
                xpReward: 90,
                goalType: ChallengeGoalType.GameCount),

            ChallengeTemplate.Create(
                name: "Score Master",
                description: "Score at least 300 points in a single game",
                type: ChallengeType.Daily,
                targetValue: 300,
                xpReward: 125,
                goalType: ChallengeGoalType.ScoreSingleGame),

            ChallengeTemplate.Create(
                name: "Dedicated Player",
                description: "Complete 7 games today",
                type: ChallengeType.Daily,
                targetValue: 7,
                xpReward: 120,
                goalType: ChallengeGoalType.GameCount),

            ChallengeTemplate.Create(
                name: "Big Score",
                description: "Score 2000 total points today",
                type: ChallengeType.Daily,
                targetValue: 2000,
                xpReward: 150,
                goalType: ChallengeGoalType.ScoreTotal),
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
                description: "Complete 25 games this week",
                type: ChallengeType.Weekly,
                targetValue: 25,
                xpReward: 300,
                goalType: ChallengeGoalType.GameCount),

            ChallengeTemplate.Create(
                name: "Point Accumulator",
                description: "Score 5000 total points this week",
                type: ChallengeType.Weekly,
                targetValue: 5000,
                xpReward: 350,
                goalType: ChallengeGoalType.ScoreTotal),

            ChallengeTemplate.Create(
                name: "Score Legend",
                description: "Score 10000 total points this week",
                type: ChallengeType.Weekly,
                targetValue: 10000,
                xpReward: 500,
                goalType: ChallengeGoalType.ScoreTotal),

            ChallengeTemplate.Create(
                name: "Regular Player",
                description: "Complete 15 games this week",
                type: ChallengeType.Weekly,
                targetValue: 15,
                xpReward: 200,
                goalType: ChallengeGoalType.GameCount),

            ChallengeTemplate.Create(
                name: "Game Master",
                description: "Complete 50 games this week",
                type: ChallengeType.Weekly,
                targetValue: 50,
                xpReward: 400,
                goalType: ChallengeGoalType.GameCount),

            ChallengeTemplate.Create(
                name: "Getting Started",
                description: "Complete 5 games this week",
                type: ChallengeType.Weekly,
                targetValue: 5,
                xpReward: 100,
                goalType: ChallengeGoalType.GameCount),

            ChallengeTemplate.Create(
                name: "High Achiever",
                description: "Score at least 300 points in 10 games this week",
                type: ChallengeType.Weekly,
                targetValue: 10,
                xpReward: 375,
                goalType: ChallengeGoalType.GameCount),

            ChallengeTemplate.Create(
                name: "Point Master",
                description: "Score 3000 total points this week",
                type: ChallengeType.Weekly,
                targetValue: 3000,
                xpReward: 250,
                goalType: ChallengeGoalType.ScoreTotal),

            ChallengeTemplate.Create(
                name: "Dedicated Gamer",
                description: "Complete 35 games this week",
                type: ChallengeType.Weekly,
                targetValue: 35,
                xpReward: 350,
                goalType: ChallengeGoalType.GameCount),

            ChallengeTemplate.Create(
                name: "Score Champion",
                description: "Score 7500 total points this week",
                type: ChallengeType.Weekly,
                targetValue: 7500,
                xpReward: 425,
                goalType: ChallengeGoalType.ScoreTotal),
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
