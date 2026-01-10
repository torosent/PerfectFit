using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.Infrastructure.Jobs;

/// <summary>
/// Background job that rotates weekly challenges.
/// Runs weekly on Monday at midnight UTC to deactivate expired challenges and create new ones from templates.
/// </summary>
public class WeeklyChallengeRotationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeeklyChallengeRotationJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public WeeklyChallengeRotationJob(
        IServiceScopeFactory scopeFactory,
        ILogger<WeeklyChallengeRotationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Weekly challenge rotation job started");

        // Run once on startup to ensure challenges exist
        try
        {
            await ExecuteRotationAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in initial weekly challenge rotation");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Calculate time until next Monday midnight UTC
                var now = DateTime.UtcNow;
                var daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
                var rotationGracePeriod = TimeSpan.FromMinutes(5);
                if (daysUntilMonday == 0 && now.TimeOfDay > rotationGracePeriod)
                {
                    daysUntilMonday = 7; // Next Monday
                }

                var nextMonday = now.Date.AddDays(daysUntilMonday);
                var delay = nextMonday - now;

                // If we're very close to Monday midnight, run immediately
                if (delay.TotalMinutes < 5)
                {
                    await ExecuteRotationAsync(stoppingToken);
                    delay = TimeSpan.FromHours(1); // Wait an hour before next check
                }

                // Don't wait more than a day at a time to allow for checks
                var maxDelay = TimeSpan.FromDays(1);
                if (delay > maxDelay)
                {
                    delay = maxDelay;
                }

                await Task.Delay(delay, stoppingToken);

                // Check if it's Monday (in case we waited less than full duration)
                if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday)
                {
                    await ExecuteRotationAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in weekly challenge rotation job");
                // Wait before retrying
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        _logger.LogInformation("Weekly challenge rotation job stopped");
    }

    /// <summary>
    /// Executes the weekly challenge rotation logic.
    /// Public for testing purposes.
    /// </summary>
    public async Task ExecuteRotationAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing weekly challenge rotation");

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IGamificationRepository>();

        try
        {
            // Get all active weekly challenges
            var activeChallenges = await repository.GetActiveChallengesAsync(ChallengeType.Weekly, cancellationToken);
            var now = DateTime.UtcNow;
            var deactivatedCount = 0;

            // Deactivate expired challenges
            foreach (var challenge in activeChallenges)
            {
                if (challenge.EndDate < now)
                {
                    challenge.Deactivate();
                    await repository.UpdateChallengeAsync(challenge, cancellationToken);
                    deactivatedCount++;
                    _logger.LogInformation("Deactivated expired weekly challenge: {ChallengeName} (ID: {ChallengeId})",
                        challenge.Name, challenge.Id);
                }
            }

            // Get remaining active challenges after deactivation
            var remainingActive = activeChallenges.Count(c => c.IsActive && c.EndDate >= now);

            // Create new challenges from templates if no active challenges exist
            if (remainingActive == 0)
            {
                var templates = await repository.GetChallengeTemplatesAsync(ChallengeType.Weekly, cancellationToken);

                foreach (var template in templates.Where(t => t.IsActive))
                {
                    // Start at Monday midnight UTC
                    var startDate = GetThisWeekMonday(now);
                    var endDate = startDate.AddDays(7); // End next Monday

                    var challenge = Challenge.CreateFromTemplate(template, startDate, endDate);

                    try
                    {
                        await repository.AddChallengeAsync(challenge, cancellationToken);
                        _logger.LogInformation("Created new weekly challenge from template: {ChallengeName} (TemplateId: {TemplateId})",
                            template.Name, template.Id);
                    }
                    catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
                    {
                        // Another instance already created this challenge - this is expected in multi-instance deployments
                        _logger.LogDebug("Weekly challenge already exists (created by another instance): {ChallengeName} (TemplateId: {TemplateId})",
                            template.Name, template.Id);
                    }
                }
            }

            _logger.LogInformation(
                "Weekly challenge rotation completed. Deactivated: {Deactivated}, Active remaining: {Active}",
                deactivatedCount, remainingActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute weekly challenge rotation");
            throw;
        }
    }

    private static DateTime GetThisWeekMonday(DateTime date)
    {
        var daysFromMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.Date.AddDays(-daysFromMonday);
    }

    /// <summary>
    /// Checks if the exception is a unique constraint violation (duplicate key).
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // PostgreSQL unique violation error code is 23505
        // SQL Server is 2601/2627
        var innerException = ex.InnerException;
        if (innerException == null) return false;

        var message = innerException.Message;
        return message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("UNIQUE constraint", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("23505"); // PostgreSQL error code
    }
}
