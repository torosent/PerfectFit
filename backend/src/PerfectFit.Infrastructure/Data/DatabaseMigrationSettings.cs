namespace PerfectFit.Infrastructure.Data;

/// <summary>
/// Configuration settings for database migration behavior.
/// </summary>
public class DatabaseMigrationSettings
{
    public const string SectionName = "DatabaseMigration";

    /// <summary>
    /// Whether to run migrations automatically on application startup.
    /// Default: true
    /// </summary>
    public bool RunMigrationsOnStartup { get; set; } = true;

    /// <summary>
    /// Whether to fail application startup if migrations fail.
    /// When false, the application will continue to start but may be in an inconsistent state.
    /// Default: true (recommended for production)
    /// </summary>
    public bool FailOnMigrationError { get; set; } = true;

    /// <summary>
    /// Maximum time to wait for migrations to complete.
    /// Default: 5 minutes
    /// </summary>
    public int MigrationTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Whether to log the SQL commands being executed during migration.
    /// Useful for debugging but should be disabled in production for security.
    /// Default: false
    /// </summary>
    public bool LogMigrationSql { get; set; } = false;

    /// <summary>
    /// Number of retry attempts for database connection before giving up.
    /// Useful when database might not be immediately available (e.g., in containers).
    /// Default: 5
    /// </summary>
    public int ConnectionRetryCount { get; set; } = 5;

    /// <summary>
    /// Delay between connection retry attempts in seconds.
    /// Default: 5 seconds
    /// </summary>
    public int ConnectionRetryDelaySeconds { get; set; } = 5;
}
