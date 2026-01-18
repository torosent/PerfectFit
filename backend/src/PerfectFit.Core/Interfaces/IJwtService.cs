using PerfectFit.Core.Entities;
using System.Security.Claims;

namespace PerfectFit.Core.Interfaces;

/// <summary>
/// Interface for JWT token operations.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    string GenerateToken(User user);

    /// <summary>
    /// Validates a JWT token and returns the claims principal if valid.
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Validates a JWT token and returns the claims principal if valid.
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime);
}
