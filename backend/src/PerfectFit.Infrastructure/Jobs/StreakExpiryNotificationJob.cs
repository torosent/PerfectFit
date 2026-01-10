using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;

namespace PerfectFit.Infrastructure.Jobs;

/// <summary>
/// Background job that sends notifications to users whose streaks are about to expire.
/// Runs hourly to find users with streaks expiring in 2-4 hours.
/// </summary>
public class StreakExpiryNotificationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StreakExpiryNotificationJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    /// <summary>
    /// Users whose streaks expire within this time window will be notified.
    /// Window is 2-4 hours before expiry.
    /// </summary>
    private readonly TimeSpan _notificationWindowMin = TimeSpan.FromHours(2);
    private readonly TimeSpan _notificationWindowMax = TimeSpan.FromHours(4);

    /// <summary>
    /// Hours to wait before sending another notification to the same user.
    /// Prevents duplicate notifications across hourly runs.
    /// </summary>
    public const int NotificationCooldownHours = 24;

    public StreakExpiryNotificationJob(
        IServiceScopeFactory scopeFactory,
        ILogger<StreakExpiryNotificationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Streak expiry notification job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecuteNotificationAsync(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in streak expiry notification job");
                // Wait before retrying
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Streak expiry notification job stopped");
    }

    /// <summary>
    /// Executes the notification logic.
    /// Public for testing purposes.
    /// </summary>
    public async Task ExecuteNotificationAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking for users with expiring streaks");

        using var scope = _scopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var streakService = scope.ServiceProvider.GetRequiredService<IStreakService>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        try
        {
            var now = DateTimeOffset.UtcNow;
            var usersWithStreaks = await userRepository.GetUsersWithActiveStreaksAsync(cancellationToken);
            var notificationsSent = 0;

            foreach (var user in usersWithStreaks)
            {
                // Skip users without email
                if (string.IsNullOrEmpty(user.Email))
                {
                    continue;
                }

                // Skip users without an active streak
                if (user.CurrentStreak <= 0)
                {
                    continue;
                }


                // Get the streak reset time
                var resetTime = streakService.GetStreakResetTime(user, now);
                var timeUntilReset = resetTime - now;

                // Check if within notification window (2-4 hours before expiry)
                if (timeUntilReset >= _notificationWindowMin && timeUntilReset <= _notificationWindowMax)
                {
                    // Atomically claim the notification slot BEFORE sending email
                    // This prevents duplicate notifications from concurrent instances
                    var claimed = await userRepository.TryClaimStreakNotificationAsync(
                        user.Id, 
                        NotificationCooldownHours, 
                        cancellationToken);
                    
                    if (!claimed)
                    {
                        _logger.LogDebug(
                            "Skipping user {UserId} - notification already claimed by another instance or within cooldown",
                            user.Id);
                        continue;
                    }
                    
                    // Send notification (notification slot already claimed atomically)
                    _logger.LogInformation(
                        "Sending streak expiry notification to user {UserId} ({Email}). Streak: {Streak}, Expires in: {TimeUntilExpiry}",
                        user.Id, user.Email, user.CurrentStreak, timeUntilReset);

                    try
                    {
                        var hoursRemaining = (int)Math.Round(timeUntilReset.TotalHours);
                        var sent = await emailService.SendStreakExpiryNotificationAsync(
                            user.Email,
                            user.DisplayName,
                            user.CurrentStreak,
                            hoursRemaining);

                        if (sent)
                        {
                            notificationsSent++;
                        }
                        else
                        {
                            _logger.LogWarning("Email service returned false for user {UserId}", user.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send streak expiry notification to user {UserId}", user.Id);
                    }
                }
            }

            if (notificationsSent > 0)
            {
                _logger.LogInformation("Sent {Count} streak expiry notifications", notificationsSent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute streak expiry notifications");
            throw;
        }
    }
}
