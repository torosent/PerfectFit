using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.Infrastructure.Data.SeedData;

/// <summary>
/// Extension methods for seeding gamification data.
/// </summary>
public static class GamificationSeedDataExtensions
{
    /// <summary>
    /// Seeds gamification data including achievements, cosmetics, challenge templates, and initial season.
    /// This method is idempotent - it only adds data that doesn't already exist.
    /// </summary>
    public static async Task SeedGamificationDataAsync(
        this IServiceProvider serviceProvider,
        ILogger? logger = null)
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IGamificationRepository>();

        logger?.LogInformation("Starting gamification data seeding...");

        // Seed achievements
        await SeedAchievementsAsync(repository, logger);

        // Seed cosmetics
        await SeedCosmeticsAsync(repository, logger);

        // Seed challenge templates
        await SeedChallengeTemplatesAsync(repository, logger);

        // Seed initial season
        await SeedInitialSeasonAsync(repository, logger);

        logger?.LogInformation("Gamification data seeding completed");
    }

    private static async Task SeedAchievementsAsync(IGamificationRepository repository, ILogger? logger)
    {
        var existingAchievements = await repository.GetAllAchievementsAsync();
        if (existingAchievements.Any())
        {
            logger?.LogDebug("Achievements already exist, skipping seeding");
            return;
        }

        var achievements = AchievementSeedData.GetAchievements();
        logger?.LogInformation("Seeding {Count} achievements...", achievements.Count);

        // Note: The repository doesn't have a bulk add method, so we need to add individually
        // In a real implementation, you might want to add achievements through EF directly
        // For now, we'll skip this as the repository interface doesn't support adding achievements
        logger?.LogWarning("Achievement seeding requires direct database access - skipping via repository");
    }

    private static async Task SeedCosmeticsAsync(IGamificationRepository repository, ILogger? logger)
    {
        var existingCosmetics = await repository.GetAllCosmeticsAsync();
        if (existingCosmetics.Any())
        {
            logger?.LogDebug("Cosmetics already exist, skipping seeding");
            return;
        }

        var cosmetics = CosmeticSeedData.GetCosmetics();
        if (cosmetics == null || cosmetics.Count == 0)
        {
            logger?.LogError("CosmeticSeedData returned no cosmetics. Cannot proceed with gamification data seeding.");
            throw new InvalidOperationException("CosmeticSeedData returned no cosmetics. Season seeding requires cosmetics to be present.");
        }

        logger?.LogInformation("Seeding {Count} cosmetics...", cosmetics.Count);

        // Note: Similar to achievements, the current repository abstraction does not support creating cosmetics.
        // Because cosmetics are required for season rewards, we must fail fast instead of silently skipping seeding.
        logger?.LogError("Cosmetics are required for season rewards, but the repository does not support cosmetic seeding. " +
                         "Add cosmetics via a migration or extend IGamificationRepository to support cosmetic creation.");
        throw new NotSupportedException("Cosmetic seeding via IGamificationRepository is not supported, but cosmetics are required for season rewards.");
    }

    private static async Task SeedChallengeTemplatesAsync(IGamificationRepository repository, ILogger? logger)
    {
        var existingTemplates = await repository.GetChallengeTemplatesAsync();
        if (existingTemplates.Any())
        {
            logger?.LogDebug("Challenge templates already exist, skipping seeding");
            return;
        }

        var templates = ChallengeSeedData.GetAllTemplates();
        logger?.LogInformation("Seeding {Count} challenge templates...", templates.Count);

        foreach (var template in templates)
        {
            await repository.AddChallengeTemplateAsync(template);
        }

        logger?.LogInformation("Challenge templates seeded successfully");
    }

    private static async Task SeedInitialSeasonAsync(IGamificationRepository repository, ILogger? logger)
    {
        var currentSeason = await repository.GetCurrentSeasonAsync();
        if (currentSeason != null)
        {
            logger?.LogDebug("Active season already exists, skipping seeding");
            return;
        }

        var allSeasons = await repository.GetAllSeasonsAsync();
        if (allSeasons.Any())
        {
            logger?.LogDebug("Seasons already exist, skipping seeding");
            return;
        }

        logger?.LogInformation("Seeding initial season...");

        var (season, rewardsFactory) = SeasonSeedData.GetInitialSeason();
        await repository.AddSeasonAsync(season);

        // Build cosmetic code resolver from existing cosmetics
        var cosmetics = await repository.GetAllCosmeticsAsync();
        var cosmeticLookup = cosmetics.ToDictionary(c => c.Code, c => c.Id);
        int ResolveCosmeticCode(string code) => cosmeticLookup.TryGetValue(code, out var id) ? id : 0;

        // Now add rewards using the season's ID and cosmetic resolver
        var rewards = rewardsFactory(season.Id, ResolveCosmeticCode);
        foreach (var reward in rewards)
        {
            await repository.AddSeasonRewardAsync(reward);
        }

        logger?.LogInformation("Initial season '{SeasonName}' seeded with {RewardCount} rewards",
            season.Name, rewards.Count);
    }

    /// <summary>
    /// Seeds gamification data directly to the database context.
    /// Use this for EF Core migrations or direct database seeding.
    /// </summary>
    public static async Task SeedGamificationDataAsync(
        this AppDbContext context,
        ILogger? logger = null)
    {
        logger?.LogInformation("Starting direct database gamification seeding...");

        // Seed achievements
        if (!await context.Achievements.AnyAsync())
        {
            var achievements = AchievementSeedData.GetAchievements();
            context.Achievements.AddRange(achievements);
            await context.SaveChangesAsync();
            logger?.LogInformation("Seeded {Count} achievements", achievements.Count);
        }

        // Seed cosmetics
        if (!await context.Cosmetics.AnyAsync())
        {
            var cosmetics = CosmeticSeedData.GetCosmetics();
            context.Cosmetics.AddRange(cosmetics);
            await context.SaveChangesAsync();
            logger?.LogInformation("Seeded {Count} cosmetics", cosmetics.Count);
        }

        // Seed challenge templates
        if (!await context.ChallengeTemplates.AnyAsync())
        {
            var templates = ChallengeSeedData.GetAllTemplates();
            context.ChallengeTemplates.AddRange(templates);
            await context.SaveChangesAsync();
            logger?.LogInformation("Seeded {Count} challenge templates", templates.Count);
        }

        // Seed initial season
        if (!await context.Seasons.AnyAsync())
        {
            var (season, rewardsFactory) = SeasonSeedData.GetInitialSeason();
            context.Seasons.Add(season);
            await context.SaveChangesAsync();

            // Build cosmetic code resolver from cosmetics in database
            var cosmeticLookup = await context.Cosmetics.ToDictionaryAsync(c => c.Code, c => c.Id);
            int ResolveCosmeticCode(string code) => cosmeticLookup.TryGetValue(code, out var id) ? id : 0;

            var rewards = rewardsFactory(season.Id, ResolveCosmeticCode);
            context.SeasonRewards.AddRange(rewards);
            await context.SaveChangesAsync();
            logger?.LogInformation("Seeded season '{SeasonName}' with {RewardCount} rewards",
                season.Name, rewards.Count);
        }

        logger?.LogInformation("Direct database gamification seeding completed");
    }
}
