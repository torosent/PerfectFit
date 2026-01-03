using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;

namespace PerfectFit.Infrastructure.Services;

/// <summary>
/// Username validation service that combines format validation and profanity checking via PurgoMalum API.
/// </summary>
public class UsernameValidationService : IUsernameValidationService
{
    private readonly IProfanityChecker _profanityChecker;

    public UsernameValidationService(IProfanityChecker profanityChecker)
    {
        _profanityChecker = profanityChecker;
    }

    /// <summary>
    /// Validates a username including format and profanity check.
    /// If profanity check fails (API unavailable), generates a random username instead.
    /// </summary>
    public async Task<UsernameValidationResult> ValidateAsync(string username, CancellationToken cancellationToken = default)
    {
        // First, validate format
        var formatResult = UsernameValidator.ValidateFormat(username);
        if (!formatResult.IsValid)
        {
            return formatResult;
        }

        // Then check profanity via API
        var containsProfanity = await _profanityChecker.ContainsProfanityAsync(username, cancellationToken);

        if (containsProfanity == null)
        {
            // API call failed - generate a random username instead for safety
            var suggestedUsername = UsernameValidator.GenerateRandomUsername();
            return UsernameValidationResult.FailureWithSuggestion(
                "Unable to validate username. Please use the suggested username or try again later.",
                suggestedUsername);
        }

        if (containsProfanity == true)
        {
            return UsernameValidationResult.Failure("Username contains inappropriate language.");
        }

        return UsernameValidationResult.Success();
    }
}
