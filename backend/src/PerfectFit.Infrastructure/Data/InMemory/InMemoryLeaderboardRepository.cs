using System.Collections.Concurrent;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.Infrastructure.Data.InMemory;

/// <summary>
/// In-memory implementation of ILeaderboardRepository for testing without a database.
/// </summary>
public class InMemoryLeaderboardRepository : ILeaderboardRepository
{
    private readonly ConcurrentDictionary<int, LeaderboardEntry> _entries = new();
    private readonly IUserRepository _userRepository;
    private int _nextId = 1;
    private readonly object _idLock = new();

    public InMemoryLeaderboardRepository(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<LeaderboardEntry>> GetTopScoresAsync(int count, CancellationToken cancellationToken = default)
    {
        var entries = _entries.Values
            .OrderByDescending(le => le.Score)
            .Take(count)
            .ToList();

        // Load user data for navigation property simulation
        foreach (var entry in entries)
        {
            var user = await _userRepository.GetByIdAsync(entry.UserId, cancellationToken);
            if (user != null)
            {
                // Use reflection to set the User navigation property
                var userProperty = typeof(LeaderboardEntry).GetProperty(nameof(LeaderboardEntry.User));
                userProperty?.SetValue(entry, user);
            }
        }

        return entries;
    }

    public Task<LeaderboardEntry?> GetUserBestScoreAsync(int userId, CancellationToken cancellationToken = default)
    {
        var entry = _entries.Values
            .Where(le => le.UserId == userId)
            .OrderByDescending(le => le.Score)
            .FirstOrDefault();
        return Task.FromResult(entry);
    }

    public Task<int> GetUserRankAsync(int userId, CancellationToken cancellationToken = default)
    {
        var userBestScore = _entries.Values
            .Where(le => le.UserId == userId)
            .Select(le => le.Score)
            .DefaultIfEmpty(0)
            .Max();

        if (userBestScore == 0)
        {
            return Task.FromResult(0);
        }

        // Count how many unique users have a higher best score
        var usersWithHigherScore = _entries.Values
            .GroupBy(le => le.UserId)
            .Select(g => g.Max(le => le.Score))
            .Count(maxScore => maxScore > userBestScore);

        return Task.FromResult(usersWithHigherScore + 1);
    }

    public Task<int> GetScoreRankAsync(int score, CancellationToken cancellationToken = default)
    {
        var entriesWithHigherScore = _entries.Values.Count(le => le.Score > score);
        return Task.FromResult(entriesWithHigherScore + 1);
    }

    public Task<LeaderboardEntry> AddAsync(LeaderboardEntry entry, CancellationToken cancellationToken = default)
    {
        lock (_idLock)
        {
            // Use reflection to set the Id since it's private
            var idProperty = typeof(LeaderboardEntry).GetProperty(nameof(LeaderboardEntry.Id));
            idProperty?.SetValue(entry, _nextId);
            _entries[_nextId] = entry;
            _nextId++;
        }
        return Task.FromResult(entry);
    }

    public Task<bool> ExistsByGameSessionIdAsync(Guid gameSessionId, CancellationToken cancellationToken = default)
    {
        var exists = _entries.Values.Any(le => le.GameSessionId == gameSessionId);
        return Task.FromResult(exists);
    }

    public Task<int> GetUserEntryCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        var count = _entries.Values.Count(le => le.UserId == userId);
        return Task.FromResult(count);
    }
}
