using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.Infrastructure.Jobs;

/// <summary>
/// Background job that rotates daily challenges.
/// Runs daily at midnight UTC to deactivate expired challenges and create new ones from templates.
/// </summary>
public class DailyChallengeRotationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyChallengeRotationJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public DailyChallengeRotationJob(
        IServiceScopeFactory scopeFactory,
        ILogger<DailyChallengeRotationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily challenge rotation job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Calculate time until next midnight UTC
                var now = DateTime.UtcNow;
                var nextMidnight = now.Date.AddDays(1);
                var delay = nextMidnight - now;

                // If we're very close to midnight, run immediately
                if (delay.TotalMinutes < 5)
                {
                    await ExecuteRotationAsync(stoppingToken);
                    delay = TimeSpan.FromHours(1); // Wait an hour before next check
                }

                await Task.Delay(delay, stoppingToken);
                await ExecuteRotationAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in daily challenge rotation job");
                // Wait before retrying
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        _logger.LogInformation("Daily challenge rotation job stopped");
    }

    /// <summary>
    /// Executes the daily challenge rotation logic.
    /// Public for testing purposes.
    /// </summary>
    public async Task ExecuteRotationAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing daily challenge rotation");

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IGamificationRepository>();

        try
        {
            // Get all active daily challenges
            var activeChallenges = await repository.GetActiveChallengesAsync(ChallengeType.Daily, cancellationToken);
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
                    _logger.LogInformation("Deactivated expired daily challenge: {ChallengeName} (ID: {ChallengeId})",
                        challenge.Name, challenge.Id);
                }
            }

            // Get remaining active challenges after deactivation
            var remainingActive = activeChallenges.Count(c => c.IsActive && c.EndDate >= now);

            // Create new challenges from templates if needed
            if (remainingActive == 0)
            {
                var templates = await repository.GetChallengeTemplatesAsync(ChallengeType.Daily, cancellationToken);

                foreach (var template in templates.Where(t => t.IsActive))
                {
                    var startDate = now.Date; // Start at midnight UTC today
                    var endDate = startDate.AddDays(1); // End at midnight UTC tomorrow

                    var challenge = Challenge.CreateFromTemplate(template, startDate, endDate);

                    try
                    {
                        await repository.AddChallengeAsync(challenge, cancellationToken);
                        _logger.LogInformation("Created new daily challenge from template: {ChallengeName} (TemplateId: {TemplateId})",
                            template.Name, template.Id);
                    }
                    catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
                    {
                        // Another instance already created this challenge - this is expected in multi-instance deployments
                        _logger.LogDebug("Daily challenge already exists (created by another instance): {ChallengeName} (TemplateId: {TemplateId})",
                            template.Name, template.Id);
                    }
                }
            }

            _logger.LogInformation(
                "Daily challenge rotation completed. Deactivated: {Deactivated}, Active remaining: {Active}",
                deactivatedCount, remainingActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute daily challenge rotation");
            throw;
        }
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
