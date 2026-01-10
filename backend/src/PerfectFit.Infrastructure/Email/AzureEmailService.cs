using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PerfectFit.Core.Services;

namespace PerfectFit.Infrastructure.Email;

/// <summary>
/// Email service implementation using Azure Communication Services.
/// </summary>
public class AzureEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<AzureEmailService> _logger;
    private readonly EmailClient _emailClient;

    public AzureEmailService(
        IOptions<EmailSettings> settings,
        ILogger<AzureEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (!string.IsNullOrEmpty(_settings.ConnectionString))
        {
            _emailClient = new EmailClient(_settings.ConnectionString);
        }
        else
        {
            _logger.LogWarning("Email ConnectionString is not configured. Emails will not be sent.");
            _emailClient = null!;
        }
    }

    /// <inheritdoc />
    public virtual async Task<bool> SendVerificationEmailAsync(
        string toEmail,
        string displayName,
        string verificationUrl)
    {
        if (_emailClient is null)
        {
            _logger.LogWarning(
                "Email service not configured. Verification email to {Email} was not sent.",
                toEmail);
            return false;
        }

        try
        {
            var emailMessage = BuildEmailMessage(toEmail, displayName, verificationUrl);

            _logger.LogInformation(
                "Sending verification email to {Email}",
                toEmail);

            EmailSendOperation emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Completed,
                emailMessage);

            _logger.LogInformation(
                "Verification email sent successfully to {Email}. Operation ID: {OperationId}",
                toEmail,
                emailSendOperation.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send verification email to {Email}",
                toEmail);
            return false;
        }
    }

    /// <summary>
    /// Builds the verification URL from the frontend URL and token.
    /// </summary>
    protected string BuildVerificationUrl(string token)
    {
        var baseUrl = _settings.FrontendUrl.TrimEnd('/');
        return $"{baseUrl}/verify-email?token={token}";
    }

    /// <summary>
    /// Builds the email message with HTML and plain text content.
    /// </summary>
    protected EmailMessage BuildEmailMessage(
        string toEmail,
        string displayName,
        string verificationUrl)
    {
        var htmlContent = BuildHtmlContent(displayName, verificationUrl);
        var plainTextContent = BuildPlainTextContent(displayName, verificationUrl);

        var emailContent = new EmailContent(_settings.VerificationEmailSubject)
        {
            Html = htmlContent,
            PlainText = plainTextContent
        };

        var recipients = new EmailRecipients(new List<EmailAddress>
        {
            new EmailAddress(toEmail, displayName)
        });

        return new EmailMessage(
            senderAddress: _settings.SenderAddress,
            content: emailContent,
            recipients: recipients);
    }

    private static string BuildHtmlContent(string displayName, string verificationUrl)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Verify your email</title>
            </head>
            <body style="margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; background-color: #f5f5f5;">
                <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="background-color: #f5f5f5;">
                    <tr>
                        <td align="center" style="padding: 40px 20px;">
                            <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="max-width: 600px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);">
                                <!-- Header -->
                                <tr>
                                    <td style="padding: 40px 40px 20px 40px; text-align: center; background-color: #4f46e5; border-radius: 8px 8px 0 0;">
                                        <h1 style="margin: 0; color: #ffffff; font-size: 28px; font-weight: 700;">PerfectFit</h1>
                                    </td>
                                </tr>
                                
                                <!-- Content -->
                                <tr>
                                    <td style="padding: 40px;">
                                        <h2 style="margin: 0 0 20px 0; color: #1f2937; font-size: 24px; font-weight: 600;">Verify your email address</h2>
                                        
                                        <p style="margin: 0 0 20px 0; color: #4b5563; font-size: 16px;">
                                            Hi {displayName},
                                        </p>
                                        
                                        <p style="margin: 0 0 30px 0; color: #4b5563; font-size: 16px;">
                                            Thanks for signing up for PerfectFit! Please click the button below to verify your email address and activate your account.
                                        </p>
                                        
                                        <!-- CTA Button -->
                                        <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin: 0 auto 30px auto;">
                                            <tr>
                                                <td style="border-radius: 6px; background-color: #4f46e5;">
                                                    <a href="{verificationUrl}" target="_blank" style="display: inline-block; padding: 16px 32px; color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 600;">
                                                        Verify Email Address
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>
                                        
                                        <p style="margin: 0 0 20px 0; color: #6b7280; font-size: 14px;">
                                            Or copy and paste this link into your browser:
                                        </p>
                                        
                                        <p style="margin: 0 0 30px 0; color: #4f46e5; font-size: 14px; word-break: break-all;">
                                            <a href="{verificationUrl}" style="color: #4f46e5; text-decoration: underline;">{verificationUrl}</a>
                                        </p>
                                        
                                        <!-- Expiry Notice -->
                                        <div style="padding: 16px; background-color: #fef3c7; border-radius: 6px; margin-bottom: 20px;">
                                            <p style="margin: 0; color: #92400e; font-size: 14px;">
                                                ⏰ This verification link will expire in <strong>24 hours</strong>.
                                            </p>
                                        </div>
                                        
                                        <!-- Ignore Notice -->
                                        <p style="margin: 0; color: #9ca3af; font-size: 13px;">
                                            If you didn't create an account with PerfectFit, you can safely ignore this email.
                                        </p>
                                    </td>
                                </tr>
                                
                                <!-- Footer -->
                                <tr>
                                    <td style="padding: 30px 40px; background-color: #f9fafb; border-radius: 0 0 8px 8px; text-align: center;">
                                        <p style="margin: 0; color: #9ca3af; font-size: 12px;">
                                            © 2026 PerfectFit. All rights reserved.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }

    private static string BuildPlainTextContent(string displayName, string verificationUrl)
    {
        return $"""
            PerfectFit - Verify your email address
            ======================================

            Hi {displayName},

            Thanks for signing up for PerfectFit! Please click the link below to verify your email address and activate your account.

            Verify your email:
            {verificationUrl}

            ⏰ This verification link will expire in 24 hours.

            If you didn't create an account with PerfectFit, you can safely ignore this email.

            ---
            © 2026 PerfectFit. All rights reserved.
            """;
    }

    /// <inheritdoc />
    public virtual async Task<bool> SendStreakExpiryNotificationAsync(
        string toEmail,
        string displayName,
        int currentStreak,
        int hoursRemaining)
    {
        if (_emailClient is null)
        {
            _logger.LogWarning(
                "Email service not configured. Streak expiry notification to {Email} was not sent.",
                toEmail);
            return false;
        }

        try
        {
            var emailMessage = BuildStreakExpiryEmailMessage(toEmail, displayName, currentStreak, hoursRemaining);

            _logger.LogInformation(
                "Sending streak expiry notification to {Email}",
                toEmail);

            EmailSendOperation emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Completed,
                emailMessage);

            _logger.LogInformation(
                "Streak expiry notification sent successfully to {Email}. Operation ID: {OperationId}",
                toEmail,
                emailSendOperation.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send streak expiry notification to {Email}",
                toEmail);
            return false;
        }
    }

    /// <summary>
    /// Builds the streak expiry notification email message with HTML and plain text content.
    /// </summary>
    protected EmailMessage BuildStreakExpiryEmailMessage(
        string toEmail,
        string displayName,
        int currentStreak,
        int hoursRemaining)
    {
        var htmlContent = BuildStreakExpiryHtmlContent(displayName, currentStreak, hoursRemaining);
        var plainTextContent = BuildStreakExpiryPlainTextContent(displayName, currentStreak, hoursRemaining);

        var emailContent = new EmailContent($"🔥 Your {currentStreak}-day streak is about to expire!")
        {
            Html = htmlContent,
            PlainText = plainTextContent
        };

        var recipients = new EmailRecipients(new List<EmailAddress>
        {
            new EmailAddress(toEmail, displayName)
        });

        return new EmailMessage(
            senderAddress: _settings.SenderAddress,
            content: emailContent,
            recipients: recipients);
    }

    private static string BuildStreakExpiryHtmlContent(string displayName, int currentStreak, int hoursRemaining)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Your streak is about to expire!</title>
            </head>
            <body style="margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; background-color: #f5f5f5;">
                <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="background-color: #f5f5f5;">
                    <tr>
                        <td align="center" style="padding: 40px 20px;">
                            <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="max-width: 600px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);">
                                <!-- Header -->
                                <tr>
                                    <td style="padding: 40px 40px 20px 40px; text-align: center; background-color: #f97316; border-radius: 8px 8px 0 0;">
                                        <h1 style="margin: 0; color: #ffffff; font-size: 48px;">🔥</h1>
                                        <h2 style="margin: 10px 0 0 0; color: #ffffff; font-size: 24px; font-weight: 700;">Don't lose your streak!</h2>
                                    </td>
                                </tr>
                                
                                <!-- Content -->
                                <tr>
                                    <td style="padding: 40px;">
                                        <p style="margin: 0 0 20px 0; color: #4b5563; font-size: 16px;">
                                            Hi {displayName},
                                        </p>
                                        
                                        <p style="margin: 0 0 20px 0; color: #4b5563; font-size: 16px;">
                                            Your <strong style="color: #f97316;">{currentStreak}-day streak</strong> is about to expire in <strong>{hoursRemaining} hours</strong>!
                                        </p>
                                        
                                        <p style="margin: 0 0 30px 0; color: #4b5563; font-size: 16px;">
                                            Play a quick game now to keep your streak going and maintain your progress.
                                        </p>
                                        
                                        <!-- Streak Counter -->
                                        <div style="text-align: center; margin-bottom: 30px;">
                                            <div style="display: inline-block; padding: 20px 40px; background: linear-gradient(135deg, #f97316 0%, #ea580c 100%); border-radius: 12px;">
                                                <div style="color: #ffffff; font-size: 48px; font-weight: 700;">{currentStreak}</div>
                                                <div style="color: #ffffff; font-size: 14px; text-transform: uppercase; letter-spacing: 1px;">Day Streak</div>
                                            </div>
                                        </div>
                                        
                                        <!-- Urgency Notice -->
                                        <div style="padding: 16px; background-color: #fef3c7; border-radius: 6px; margin-bottom: 20px;">
                                            <p style="margin: 0; color: #92400e; font-size: 14px;">
                                                ⏰ Time remaining: <strong>{hoursRemaining} hours</strong>
                                            </p>
                                        </div>
                                        
                                        <p style="margin: 0; color: #9ca3af; font-size: 13px;">
                                            Keep up the great work! Consistency is key to improving your puzzle-solving skills.
                                        </p>
                                    </td>
                                </tr>
                                
                                <!-- Footer -->
                                <tr>
                                    <td style="padding: 30px 40px; background-color: #f9fafb; border-radius: 0 0 8px 8px; text-align: center;">
                                        <p style="margin: 0; color: #9ca3af; font-size: 12px;">
                                            © 2026 PerfectFit. All rights reserved.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }

    private static string BuildStreakExpiryPlainTextContent(string displayName, int currentStreak, int hoursRemaining)
    {
        return $"""
            PerfectFit - Your streak is about to expire!
            ============================================

            Hi {displayName},

            🔥 Your {currentStreak}-day streak is about to expire in {hoursRemaining} hours!

            Play a quick game now to keep your streak going and maintain your progress.

            ⏰ Time remaining: {hoursRemaining} hours

            Keep up the great work! Consistency is key to improving your puzzle-solving skills.

            ---
            © 2026 PerfectFit. All rights reserved.
            """;
    }
}
