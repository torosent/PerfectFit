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
    string? Avatar,
    string? Email,
    string Provider,
    int HighScore,
    int GamesPlayed,
    string Role = "User"
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
public record UpdateProfileRequest(string? DisplayName, string? Avatar);

/// <summary>
/// Response from profile update operation.
/// </summary>
public record UpdateProfileResponse(
    bool Success,
    string? ErrorMessage,
    string? SuggestedDisplayName,
    UserProfileResponse? Profile
);

/// <summary>
/// User profile data in responses.
/// </summary>
public record UserProfileResponse(int Id, string DisplayName, string? Avatar);

/// <summary>
/// Request for local user registration.
/// </summary>
public record RegisterRequest(string Email, string Password, string DisplayName, string? Avatar = null);

/// <summary>
/// Request for local user login.
/// </summary>
public record LoginRequest(string Email, string Password);

/// <summary>
/// Request to verify email address.
/// </summary>
public record VerifyEmailRequest(string Email, string Token);

/// <summary>
/// Response from registration operation.
/// </summary>
public record RegisterResponse(bool Success, string? Message, string? ErrorMessage);

/// <summary>
/// Response from login operation.
/// </summary>
public record LocalLoginResponse(bool Success, string? Token, UserDto? User, string? ErrorMessage, DateTime? LockoutEnd = null);

/// <summary>
/// Response from email verification operation.
/// </summary>
public record VerifyEmailResponse(bool Success, string? Message, string? ErrorMessage);
