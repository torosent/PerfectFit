using System.Text.RegularExpressions;

namespace PerfectFit.Core.Services;

/// <summary>
/// Static utility class for validating username format.
/// Profanity checking is handled separately by IProfanityChecker.
/// </summary>
public static partial class UsernameValidator
{
    public const int MinLength = 3;
    public const int MaxLength = 20;

    // Regex pattern: alphanumeric and underscore only
    [GeneratedRegex(@"^[A-Za-z0-9_]+$")]
    private static partial Regex ValidUsernamePattern();

    /// <summary>
    /// Checks if the username format is valid (3-20 chars, alphanumeric + underscore).
    /// </summary>
    public static bool IsValidFormat(string username)
    {
        if (string.IsNullOrEmpty(username))
            return false;

        if (username.Length < MinLength || username.Length > MaxLength)
            return false;

        return ValidUsernamePattern().IsMatch(username);
    }

    /// <summary>
    /// Validates username format only (not profanity).
    /// Use IUsernameValidationService.ValidateAsync for full validation including profanity check.
    /// </summary>
    public static UsernameValidationResult ValidateFormat(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return UsernameValidationResult.Failure("Username cannot be empty.");
        }

        if (username.Length < MinLength || username.Length > MaxLength)
        {
            return UsernameValidationResult.Failure(
                $"Username must be between {MinLength} and {MaxLength} characters.");
        }

        if (!ValidUsernamePattern().IsMatch(username))
        {
            return UsernameValidationResult.Failure(
                "Username can only contain alphanumeric characters and underscores.");
        }

        return UsernameValidationResult.Success();
    }

    /// <summary>
    /// Generates a random username in the format Player_XXXXXX where X is alphanumeric.
    /// </summary>
    public static string GenerateRandomUsername()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var suffix = new char[6];

        for (int i = 0; i < 6; i++)
        {
            suffix[i] = chars[Random.Shared.Next(chars.Length)];
        }

        return $"Player_{new string(suffix)}";
    }
}

/// <summary>
/// Result of username validation.
/// </summary>
public class UsernameValidationResult
{
    public bool IsValid { get; }
    public string? ErrorMessage { get; }
    public string? SuggestedUsername { get; }

    private UsernameValidationResult(bool isValid, string? errorMessage, string? suggestedUsername = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        SuggestedUsername = suggestedUsername;
    }

    public static UsernameValidationResult Success() => new(true, null);
    public static UsernameValidationResult Failure(string errorMessage) => new(false, errorMessage);
    public static UsernameValidationResult FailureWithSuggestion(string errorMessage, string suggestedUsername)
        => new(false, errorMessage, suggestedUsername);
}
