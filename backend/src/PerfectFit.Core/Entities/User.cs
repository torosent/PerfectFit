using PerfectFit.Core.Enums;

namespace PerfectFit.Core.Entities;

public class User
{
    public int Id { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public AuthProvider Provider { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int HighScore { get; private set; }
    public int GamesPlayed { get; private set; }

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
            Provider = provider,
            CreatedAt = DateTime.UtcNow,
            HighScore = 0,
            GamesPlayed = 0,
            LastLoginAt = null
        };
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
