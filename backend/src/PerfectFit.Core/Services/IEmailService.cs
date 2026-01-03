namespace PerfectFit.Core.Services;

/// <summary>
/// Interface for sending emails to users.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a verification email to a newly registered user.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="displayName">The user's display name for personalization.</param>
    /// <param name="verificationUrl">The complete verification URL for the user to click.</param>
    /// <returns>True if the email was sent successfully, false otherwise.</returns>
    Task<bool> SendVerificationEmailAsync(
        string toEmail,
        string displayName,
        string verificationUrl);
}
