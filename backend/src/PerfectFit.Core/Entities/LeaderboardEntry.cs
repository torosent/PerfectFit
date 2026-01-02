namespace PerfectFit.Core.Entities;

public class LeaderboardEntry
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int Score { get; private set; }
    public int LinesCleared { get; private set; }
    public int MaxCombo { get; private set; }
    public DateTime AchievedAt { get; private set; }
    public Guid GameSessionId { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;
    public GameSession GameSession { get; private set; } = null!;

    // Private constructor for EF Core
    private LeaderboardEntry() { }

    public static LeaderboardEntry Create(int userId, int score, int linesCleared, int maxCombo, Guid gameSessionId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(score, nameof(score));
        ArgumentOutOfRangeException.ThrowIfNegative(linesCleared, nameof(linesCleared));
        ArgumentOutOfRangeException.ThrowIfNegative(maxCombo, nameof(maxCombo));
        
        if (gameSessionId == Guid.Empty)
        {
            throw new ArgumentException("Game session ID cannot be empty.", nameof(gameSessionId));
        }

        return new LeaderboardEntry
        {
            UserId = userId,
            Score = score,
            LinesCleared = linesCleared,
            MaxCombo = maxCombo,
            GameSessionId = gameSessionId,
            AchievedAt = DateTime.UtcNow
        };
    }
}
