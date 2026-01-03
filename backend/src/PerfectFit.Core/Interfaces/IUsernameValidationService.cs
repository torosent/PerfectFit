using PerfectFit.Core.Services;

namespace PerfectFit.Core.Interfaces;

/// <summary>
/// Interface for username validation service that combines format validation and profanity checking.
/// </summary>
public interface IUsernameValidationService
{
    /// <summary>
    /// Validates a username including format and profanity check.
    /// If profanity check fails (API unavailable), generates a random username instead.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with optional suggested username if original was rejected or check failed.</returns>
    Task<UsernameValidationResult> ValidateAsync(string username, CancellationToken cancellationToken = default);
}
