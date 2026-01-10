using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.Infrastructure.Jobs;

/// <summary>
/// Background job that handles season transitions.
/// Runs daily to check if the current season has ended and activates the next season.
/// </summary>
public class SeasonTransitionJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SeasonTransitionJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public SeasonTransitionJob(
        IServiceScopeFactory scopeFactory,
        ILogger<SeasonTransitionJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Season transition job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecuteTransitionAsync(stoppingToken);

                // Check once per day at midnight UTC
                var now = DateTime.UtcNow;
                var nextCheck = now.Date.AddDays(1);
                var delay = nextCheck - now;

                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in season transition job");
                // Wait before retrying
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        _logger.LogInformation("Season transition job stopped");
    }

    /// <summary>
    /// Executes the season transition logic.
    /// Public for testing purposes.
    /// </summary>
    public async Task ExecuteTransitionAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking for season transition");

        using var scope = _scopeFactory.CreateScope();
        var gamificationRepository = scope.ServiceProvider.GetRequiredService<IGamificationRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        try
        {
            var now = DateTime.UtcNow;
            var currentSeason = await gamificationRepository.GetCurrentSeasonAsync(cancellationToken);

            // If there's an active season that hasn't ended, nothing to do
            if (currentSeason != null && currentSeason.EndDate >= now)
            {
                _logger.LogDebug("Current season {SeasonName} is still active until {EndDate}",
                    currentSeason.Name, currentSeason.EndDate);
                return;
            }

            var allSeasons = await gamificationRepository.GetAllSeasonsAsync(cancellationToken);

            // Find ended seasons that need to be deactivated
            var endedSeasons = allSeasons.Where(s => s.IsActive && s.EndDate < now).ToList();
            
            // Find the next season to activate
            var nextSeason = allSeasons
                .Where(s => !s.IsActive && s.StartDate <= now && s.EndDate >= now)
                .OrderBy(s => s.Number)
                .FirstOrDefault();

            // Archive user progress and reset for ended seasons
            if (endedSeasons.Any())
            {
                var users = await userRepository.GetAllAsync(cancellationToken);

                foreach (var endedSeason in endedSeasons)
                {
                    _logger.LogInformation("Season {SeasonName} has ended. Archiving user progress.", endedSeason.Name);

                    foreach (var user in users)
                    {
                        // Only archive if user had some progress
                        if (user.SeasonPassXP > 0 || user.CurrentSeasonTier > 0)
                        {
                            var archive = SeasonArchive.Create(
                                user.Id,
                                endedSeason.Id,
                                user.SeasonPassXP,
                                user.CurrentSeasonTier);

                            try
                            {
                                await gamificationRepository.AddSeasonArchiveAsync(archive, cancellationToken);
                                _logger.LogDebug("Archived progress for user {UserId}: XP={XP}, Tier={Tier}",
                                    user.Id, user.SeasonPassXP, user.CurrentSeasonTier);
                            }
                            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
                            {
                                // Archive already exists (created by another instance) - this is expected
                                _logger.LogDebug("Archive already exists for user {UserId} and season {SeasonId}",
                                    user.Id, endedSeason.Id);
                            }
                        }
                    }

                    // Deactivate the ended season
                    endedSeason.Deactivate();
                    await gamificationRepository.UpdateSeasonAsync(endedSeason, cancellationToken);
                    _logger.LogInformation("Deactivated season {SeasonName}", endedSeason.Name);
                }

                // Reset all users' season progress
                foreach (var user in users)
                {
                    if (user.SeasonPassXP > 0 || user.CurrentSeasonTier > 0)
                    {
                        user.ResetSeasonProgress();
                        await userRepository.UpdateAsync(user, cancellationToken);
                    }
                }

                _logger.LogInformation("Reset season progress for all users");
            }

            // Activate the next season
            if (nextSeason != null)
            {
                nextSeason.Activate();
                await gamificationRepository.UpdateSeasonAsync(nextSeason, cancellationToken);
                _logger.LogInformation("Activated new season: {SeasonName} (Number: {Number})",
                    nextSeason.Name, nextSeason.Number);
            }
            else if (endedSeasons.Any())
            {
                _logger.LogWarning("No upcoming season found to activate after season end");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute season transition");
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
