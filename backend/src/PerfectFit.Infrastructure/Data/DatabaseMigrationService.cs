using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PerfectFit.Infrastructure.Data;

/// <summary>
/// Service for managing database migrations with various strategies.
/// </summary>
public class DatabaseMigrationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(AppDbContext context, ILogger<DatabaseMigrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets the list of pending migrations that haven't been applied yet.
    /// </summary>
    public async Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.GetPendingMigrationsAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the list of all migrations that have been applied to the database.
    /// </summary>
    public async Task<IEnumerable<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.GetAppliedMigrationsAsync(cancellationToken);
    }

    /// <summary>
    /// Applies all pending migrations to the database.
    /// </summary>
    /// <returns>True if migrations were applied, false if database was already up-to-date.</returns>
    public async Task<MigrationResult> MigrateAsync(CancellationToken cancellationToken = default)
    {
        var pendingMigrations = (await GetPendingMigrationsAsync(cancellationToken)).ToList();
        
        if (!pendingMigrations.Any())
        {
            _logger.LogInformation("Database is up-to-date, no migrations to apply");
            return new MigrationResult
            {
                Success = true,
                MigrationsApplied = Array.Empty<string>(),
                Message = "Database is up-to-date"
            };
        }

        _logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
            pendingMigrations.Count,
            string.Join(", ", pendingMigrations));

        try
        {
            await _context.Database.MigrateAsync(cancellationToken);
            
            _logger.LogInformation("Successfully applied {Count} migration(s)", pendingMigrations.Count);
            
            return new MigrationResult
            {
                Success = true,
                MigrationsApplied = pendingMigrations,
                Message = $"Successfully applied {pendingMigrations.Count} migration(s)"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply migrations");
            return new MigrationResult
            {
                Success = false,
                MigrationsApplied = Array.Empty<string>(),
                Message = $"Migration failed: {ex.Message}",
                Error = ex
            };
        }
    }

    /// <summary>
    /// Checks if the database can connect and is accessible.
    /// </summary>
    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Database.CanConnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to database");
            return false;
        }
    }

    /// <summary>
    /// Ensures the database exists. Creates it if it doesn't exist.
    /// Note: This should only be used in development. Use migrations for production.
    /// </summary>
    public async Task<bool> EnsureDatabaseExistsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.EnsureCreatedAsync(cancellationToken);
    }

    /// <summary>
    /// Generates a SQL script for all pending migrations without applying them.
    /// Useful for reviewing changes before applying or for manual deployment.
    /// </summary>
    public string GenerateMigrationScript()
    {
        return _context.Database.GenerateCreateScript();
    }

    /// <summary>
    /// Gets information about the current database state.
    /// </summary>
    public async Task<DatabaseStateInfo> GetDatabaseStateAsync(CancellationToken cancellationToken = default)
    {
        var canConnect = await CanConnectAsync(cancellationToken);
        var appliedMigrations = canConnect 
            ? (await GetAppliedMigrationsAsync(cancellationToken)).ToList() 
            : new List<string>();
        var pendingMigrations = canConnect 
            ? (await GetPendingMigrationsAsync(cancellationToken)).ToList() 
            : new List<string>();

        return new DatabaseStateInfo
        {
            CanConnect = canConnect,
            AppliedMigrations = appliedMigrations,
            PendingMigrations = pendingMigrations,
            IsUpToDate = canConnect && !pendingMigrations.Any(),
            DatabaseProvider = _context.Database.ProviderName ?? "Unknown"
        };
    }
}

/// <summary>
/// Result of a migration operation.
/// </summary>
public class MigrationResult
{
    public bool Success { get; init; }
    public IReadOnlyList<string> MigrationsApplied { get; init; } = Array.Empty<string>();
    public string Message { get; init; } = string.Empty;
    public Exception? Error { get; init; }
}

/// <summary>
/// Information about the current database state.
/// </summary>
public class DatabaseStateInfo
{
    public bool CanConnect { get; init; }
    public IReadOnlyList<string> AppliedMigrations { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> PendingMigrations { get; init; } = Array.Empty<string>();
    public bool IsUpToDate { get; init; }
    public string DatabaseProvider { get; init; } = string.Empty;
}
