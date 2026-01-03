namespace PerfectFit.Core.Identity;

/// <summary>
/// Interface for password hashing operations.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password.
    /// </summary>
    /// <param name="password">The plain text password to hash.</param>
    /// <returns>The hashed password.</returns>
    /// <exception cref="ArgumentException">Thrown when password is null, empty, or whitespace.</exception>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a plain text password against a hash.
    /// </summary>
    /// <param name="password">The plain text password to verify.</param>
    /// <param name="hash">The hash to verify against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    bool VerifyPassword(string password, string hash);
}
