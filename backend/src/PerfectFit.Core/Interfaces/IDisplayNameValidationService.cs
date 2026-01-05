using PerfectFit.Core.Services;

namespace PerfectFit.Core.Interfaces;

/// <summary>
/// Interface for display name validation service that combines format validation and profanity checking.
/// </summary>
public interface IDisplayNameValidationService
{
    /// <summary>
    /// Validates a display name including format and profanity check.
    /// If profanity check fails (API unavailable), generates a random display name instead.
    /// </summary>
    /// <param name="displayName">The display name to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with optional suggested display name if original was rejected or check failed.</returns>
    Task<DisplayNameValidationResult> ValidateAsync(string displayName, CancellationToken cancellationToken = default);
}
