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
