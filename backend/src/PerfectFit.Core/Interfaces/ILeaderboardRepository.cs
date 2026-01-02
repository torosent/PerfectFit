using PerfectFit.Core.Entities;

namespace PerfectFit.Core.Interfaces;

public interface ILeaderboardRepository
{
    Task<IEnumerable<LeaderboardEntry>> GetTopScoresAsync(int count, CancellationToken cancellationToken = default);
    Task<LeaderboardEntry?> GetUserBestScoreAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> GetUserRankAsync(int userId, CancellationToken cancellationToken = default);
    Task<LeaderboardEntry> AddAsync(LeaderboardEntry entry, CancellationToken cancellationToken = default);
}
