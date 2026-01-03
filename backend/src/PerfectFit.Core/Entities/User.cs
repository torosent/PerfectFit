using PerfectFit.Core.Enums;
using PerfectFit.Core.Services;

namespace PerfectFit.Core.Entities;

public class User
{
    public int Id { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string? Avatar { get; private set; }
    public AuthProvider Provider { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int HighScore { get; private set; }
    public int GamesPlayed { get; private set; }
    public DateTime? LastUsernameChangeAt { get; private set; }

    // Navigation
    public ICollection<GameSession> GameSessions { get; private set; } = new List<GameSession>();

    // Private constructor for EF Core
    private User() { }

    public static User Create(string externalId, string? email, string displayName, AuthProvider provider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId, nameof(externalId));
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));

        return new User
        {
            ExternalId = externalId,
            Email = email,
            DisplayName = displayName,
            Username = UsernameValidator.GenerateRandomUsername(),
            Avatar = null,
            Provider = provider,
            CreatedAt = DateTime.UtcNow,
            HighScore = 0,
            GamesPlayed = 0,
            LastLoginAt = null
        };
    }

    /// <summary>
    /// Sets the username directly (use after async validation).
    /// </summary>
    /// <param name="username">The new username to set (must be pre-validated).</param>
    /// <exception cref="ArgumentException">Thrown when username format is invalid.</exception>
    public void SetUsername(string username)
    {
        // Only validate format here - profanity check should be done via IUsernameValidationService before calling this
        var formatResult = UsernameValidator.ValidateFormat(username);
        if (!formatResult.IsValid)
        {
            throw new ArgumentException($"Username validation failed: {formatResult.ErrorMessage}", nameof(username));
        }

        Username = username;
        LastUsernameChangeAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the avatar emoji.
    /// </summary>
    /// <param name="avatar">The avatar emoji or null to clear.</param>
    public void SetAvatar(string? avatar)
    {
        Avatar = avatar;
    }

    public void UpdateHighScore(int score)
    {
        if (score > HighScore)
        {
            HighScore = score;
        }
    }

    public void IncrementGamesPlayed()
    {
        GamesPlayed++;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}
