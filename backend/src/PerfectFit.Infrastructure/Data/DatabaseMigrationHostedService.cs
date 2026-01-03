using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PerfectFit.Infrastructure.Data;

/// <summary>
/// Background service that applies database migrations on application startup.
/// Includes retry logic for scenarios where the database might not be immediately available.
/// </summary>
public class DatabaseMigrationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrationHostedService> _logger;
    private readonly DatabaseMigrationSettings _settings;
    private readonly IHostApplicationLifetime _lifetime;

    public DatabaseMigrationHostedService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseMigrationHostedService> logger,
        IOptions<DatabaseMigrationSettings> settings,
        IHostApplicationLifetime lifetime)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = settings.Value;
        _lifetime = lifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_settings.RunMigrationsOnStartup)
        {
            _logger.LogInformation("Database migrations on startup are disabled");
            return;
        }

        _logger.LogInformation("Starting database migration check...");

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // Wait for database to be available with retries
            await WaitForDatabaseAsync(dbContext, cancellationToken);

            // Apply migrations
            await ApplyMigrationsAsync(dbContext, cancellationToken);
        }
        catch (Exception ex) when (!_settings.FailOnMigrationError)
        {
            _logger.LogError(ex, "Database migration failed but FailOnMigrationError is false. Application will continue.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Database migration failed. Stopping application.");
            _lifetime.StopApplication();
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task WaitForDatabaseAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var retryCount = 0;
        var maxRetries = _settings.ConnectionRetryCount;
        var retryDelay = TimeSpan.FromSeconds(_settings.ConnectionRetryDelaySeconds);

        while (retryCount < maxRetries)
        {
            try
            {
                if (await dbContext.Database.CanConnectAsync(cancellationToken))
                {
                    _logger.LogInformation("Successfully connected to database");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to database (attempt {Attempt}/{MaxAttempts})",
                    retryCount + 1, maxRetries);
            }

            retryCount++;
            if (retryCount < maxRetries)
            {
                _logger.LogInformation("Waiting {Delay} seconds before retry...", retryDelay.TotalSeconds);
                await Task.Delay(retryDelay, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Failed to connect to database after {maxRetries} attempts. " +
            "Ensure the database server is running and connection string is correct.");
    }

    private async Task ApplyMigrationsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_settings.MigrationTimeoutSeconds));

        try
        {
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cts.Token);
            var pendingList = pendingMigrations.ToList();

            if (!pendingList.Any())
            {
                _logger.LogInformation("Database is up-to-date, no migrations to apply");
                return;
            }

            _logger.LogInformation("Found {Count} pending migration(s): {Migrations}",
                pendingList.Count,
                string.Join(", ", pendingList));

            if (_settings.LogMigrationSql)
            {
                var script = dbContext.Database.GenerateCreateScript();
                _logger.LogDebug("Migration SQL:\n{Script}", script);
            }

            await dbContext.Database.MigrateAsync(cts.Token);

            _logger.LogInformation("Successfully applied {Count} database migration(s)", pendingList.Count);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"Database migration timed out after {_settings.MigrationTimeoutSeconds} seconds");
        }
    }
}
