using PerfectFit.Core.Entities;

namespace PerfectFit.Core.Identity;

/// <summary>
/// Interface for email verification operations.
/// </summary>
public interface IEmailVerificationService
{
    /// <summary>
    /// Generates a secure random verification token.
    /// </summary>
    /// <returns>A Base64Url-safe token string.</returns>
    string GenerateVerificationToken();

    /// <summary>
    /// Checks if a token is valid for the user (matches and not expired).
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="token">The token to validate.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    bool IsTokenValid(User user, string token);

    /// <summary>
    /// Sets a verification token on the user with a 24-hour expiry.
    /// </summary>
    /// <param name="user">The user to set the token for.</param>
    void SetVerificationToken(User user);

    /// <summary>
    /// Clears the token and marks the user's email as verified.
    /// </summary>
    /// <param name="user">The user to mark as verified.</param>
    void MarkEmailVerified(User user);
}
