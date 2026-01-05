namespace PerfectFit.Core.Configuration;

/// <summary>
/// Configuration settings for admin bootstrap functionality.
/// Specifies email addresses that should automatically be granted admin role on login.
/// </summary>
public class AdminSettings
{
    public const string SectionName = "Admin";

    /// <summary>
    /// List of email addresses that should be automatically granted admin role.
    /// Email matching is case-insensitive.
    /// </summary>
    public List<string> Emails { get; set; } = new();
}
