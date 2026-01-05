namespace PerfectFit.Infrastructure.Email;

/// <summary>
/// Configuration settings for Azure Communication Services email.
/// </summary>
public class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>
    /// Azure Communication Services connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The sender email address (e.g., DoNotReply@xxx.azurecomm.net).
    /// </summary>
    public string SenderAddress { get; set; } = string.Empty;

    /// <summary>
    /// Subject line for verification emails.
    /// </summary>
    public string VerificationEmailSubject { get; set; } = "Verify your PerfectFit account";

    /// <summary>
    /// Frontend URL used for building verification links.
    /// </summary>
    public string FrontendUrl { get; set; } = string.Empty;
}
