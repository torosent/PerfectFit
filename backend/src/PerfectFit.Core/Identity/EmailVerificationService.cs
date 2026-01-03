using System.Security.Cryptography;
using PerfectFit.Core.Entities;

namespace PerfectFit.Core.Identity;

/// <summary>
/// Service for email verification token generation and validation.
/// </summary>
public class EmailVerificationService : IEmailVerificationService
{
    private const int TokenSizeInBytes = 32;
    private static readonly TimeSpan TokenExpiry = TimeSpan.FromHours(24);

    /// <inheritdoc />
    public string GenerateVerificationToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);
        return Base64UrlEncode(bytes);
    }

    /// <inheritdoc />
    public bool IsTokenValid(User user, string token)
    {
        if (string.IsNullOrEmpty(user.EmailVerificationToken))
        {
            return false;
        }

        if (user.EmailVerificationTokenExpiry == null || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            return false;
        }

        return string.Equals(user.EmailVerificationToken, token, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public void SetVerificationToken(User user)
    {
        var token = GenerateVerificationToken();
        var expiry = DateTime.UtcNow.Add(TokenExpiry);
        user.SetEmailVerificationToken(token, expiry);
    }

    /// <inheritdoc />
    public void MarkEmailVerified(User user)
    {
        user.MarkEmailAsVerified();
    }

    /// <summary>
    /// Converts bytes to a Base64Url-safe string (no +, /, or = characters).
    /// </summary>
    private static string Base64UrlEncode(byte[] bytes)
    {
        var base64 = Convert.ToBase64String(bytes);
        return base64
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
