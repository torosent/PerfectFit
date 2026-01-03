using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;

namespace PerfectFit.Infrastructure.Services;

/// <summary>
/// Display name validation service that combines format validation and profanity checking via PurgoMalum API.
/// </summary>
public class DisplayNameValidationService : IDisplayNameValidationService
{
    private readonly IProfanityChecker _profanityChecker;

    public DisplayNameValidationService(IProfanityChecker profanityChecker)
    {
        _profanityChecker = profanityChecker;
    }

    /// <summary>
    /// Validates a display name including format and profanity check.
    /// If profanity check fails (API unavailable), generates a random display name instead.
    /// </summary>
    public async Task<DisplayNameValidationResult> ValidateAsync(string displayName, CancellationToken cancellationToken = default)
    {
        // First, validate format
        var formatResult = DisplayNameValidator.ValidateFormat(displayName);
        if (!formatResult.IsValid)
        {
            return formatResult;
        }

        // Then check profanity via API
        var containsProfanity = await _profanityChecker.ContainsProfanityAsync(displayName, cancellationToken);

        if (containsProfanity == null)
        {
            // API call failed - generate a random display name instead for safety
            var suggestedDisplayName = DisplayNameValidator.GenerateRandomDisplayName();
            return DisplayNameValidationResult.FailureWithSuggestion(
                "Unable to validate display name. Please use the suggested name or try again later.",
                suggestedDisplayName);
        }

        if (containsProfanity == true)
        {
            return DisplayNameValidationResult.Failure("Display name contains inappropriate language.");
        }

        return DisplayNameValidationResult.Success();
    }
}
