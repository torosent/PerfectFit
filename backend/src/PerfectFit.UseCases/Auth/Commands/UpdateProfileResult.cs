namespace PerfectFit.UseCases.Auth.Commands;

/// <summary>
/// Result of a profile update operation.
/// </summary>
public record UpdateProfileResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? SuggestedUsername { get; init; }
    public UserProfileDto? UpdatedProfile { get; init; }

    public static UpdateProfileResult Succeeded(UserProfileDto profile) =>
        new() { Success = true, UpdatedProfile = profile };

    public static UpdateProfileResult Failed(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };

    public static UpdateProfileResult FailedWithSuggestion(string errorMessage, string suggestedUsername) =>
        new() { Success = false, ErrorMessage = errorMessage, SuggestedUsername = suggestedUsername };
}

/// <summary>
/// User profile data returned from profile operations.
/// </summary>
public record UserProfileDto(int Id, string Username, string? Avatar);
