using System.Text.RegularExpressions;

namespace PerfectFit.Core.Services;

/// <summary>
/// Static utility class for validating display name format.
/// Profanity checking is handled separately by IProfanityChecker.
/// </summary>
public static partial class DisplayNameValidator
{
    public const int MinLength = 3;
    public const int MaxLength = 20;

    // Regex pattern: alphanumeric and underscore only
    [GeneratedRegex(@"^[A-Za-z0-9_]+$")]
    private static partial Regex ValidDisplayNamePattern();

    /// <summary>
    /// Checks if the display name format is valid (3-20 chars, alphanumeric + underscore).
    /// </summary>
    public static bool IsValidFormat(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
            return false;

        if (displayName.Length < MinLength || displayName.Length > MaxLength)
            return false;

        return ValidDisplayNamePattern().IsMatch(displayName);
    }

    /// <summary>
    /// Validates display name format only (not profanity).
    /// Use IDisplayNameValidationService.ValidateAsync for full validation including profanity check.
    /// </summary>
    public static DisplayNameValidationResult ValidateFormat(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
        {
            return DisplayNameValidationResult.Failure("Display name cannot be empty.");
        }

        if (displayName.Length < MinLength || displayName.Length > MaxLength)
        {
            return DisplayNameValidationResult.Failure(
                $"Display name must be between {MinLength} and {MaxLength} characters.");
        }

        if (!ValidDisplayNamePattern().IsMatch(displayName))
        {
            return DisplayNameValidationResult.Failure(
                "Display name can only contain alphanumeric characters and underscores.");
        }

        return DisplayNameValidationResult.Success();
    }

    /// <summary>
    /// Generates a random display name in the format Player_XXXXXX where X is alphanumeric.
    /// </summary>
    public static string GenerateRandomDisplayName()
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
/// Result of display name validation.
/// </summary>
public class DisplayNameValidationResult
{
    public bool IsValid { get; }
    public string? ErrorMessage { get; }
    public string? SuggestedDisplayName { get; }

    private DisplayNameValidationResult(bool isValid, string? errorMessage, string? suggestedDisplayName = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        SuggestedDisplayName = suggestedDisplayName;
    }

    public static DisplayNameValidationResult Success() => new(true, null);
    public static DisplayNameValidationResult Failure(string errorMessage) => new(false, errorMessage);
    public static DisplayNameValidationResult FailureWithSuggestion(string errorMessage, string suggestedDisplayName)
        => new(false, errorMessage, suggestedDisplayName);
}
