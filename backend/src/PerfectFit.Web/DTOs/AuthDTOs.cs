namespace PerfectFit.Web.DTOs;

/// <summary>
/// Response returned after successful authentication.
/// </summary>
public record LoginResponseDto(
    string Token,
    UserDto User
);

/// <summary>
/// Represents a user in auth responses.
/// </summary>
public record UserDto(
    int Id,
    string DisplayName,
    string Username,
    string? Avatar,
    string? Email,
    string Provider,
    int HighScore,
    int GamesPlayed
);

/// <summary>
/// Request to refresh an expired token.
/// </summary>
public record RefreshTokenRequestDto(string Token);

/// <summary>
/// OAuth callback state for security.
/// </summary>
public record OAuthStateDto(string ReturnUrl, string Nonce);

/// <summary>
/// Request to update user profile.
/// </summary>
public record UpdateProfileRequest(string? Username, string? Avatar);

/// <summary>
/// Response from profile update operation.
/// </summary>
public record UpdateProfileResponse(
    bool Success,
    string? ErrorMessage,
    string? SuggestedUsername,
    UserProfileResponse? Profile
);

/// <summary>
/// User profile data in responses.
/// </summary>
public record UserProfileResponse(int Id, string Username, string? Avatar);
