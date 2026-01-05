using PerfectFit.Core.Entities;

namespace PerfectFit.Core.Interfaces;

public interface ILeaderboardRepository
{
    Task<IEnumerable<LeaderboardEntry>> GetTopScoresAsync(int count, CancellationToken cancellationToken = default);
    Task<LeaderboardEntry?> GetUserBestScoreAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> GetUserRankAsync(int userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the rank of a specific score on the leaderboard.
    /// </summary>
    Task<int> GetScoreRankAsync(int score, CancellationToken cancellationToken = default);
    
    Task<LeaderboardEntry> AddAsync(LeaderboardEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a leaderboard entry already exists for a game session.
    /// </summary>
    Task<bool> ExistsByGameSessionIdAsync(Guid gameSessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of leaderboard entries for a user.
    /// </summary>
    Task<int> GetUserEntryCountAsync(int userId, CancellationToken cancellationToken = default);
}
