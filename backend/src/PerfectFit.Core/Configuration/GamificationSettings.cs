namespace PerfectFit.Core.Configuration;

/// <summary>
/// Configuration settings for gamification features.
/// </summary>
public class GamificationSettings
{
    public const string SectionName = "Gamification";

    /// <summary>
    /// Whether to run gamification seed data on application startup.
    /// Should be true for development and initial setup, false for production.
    /// Default: false
    /// </summary>
    public bool SeedOnStartup { get; set; } = false;
}
