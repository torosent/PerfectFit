using Microsoft.EntityFrameworkCore;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Interfaces;
using PerfectFit.Infrastructure.Data;

namespace PerfectFit.Infrastructure.Data.Repositories;

public class LeaderboardRepository : ILeaderboardRepository
{
    private readonly AppDbContext _context;

    public LeaderboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaderboardEntry>> GetTopScoresAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.LeaderboardEntries
            .OrderByDescending(le => le.Score)
            .Take(count)
            .Include(le => le.User)
            .ToListAsync(cancellationToken);
    }

    public async Task<LeaderboardEntry?> GetUserBestScoreAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.LeaderboardEntries
            .Where(le => le.UserId == userId)
            .OrderByDescending(le => le.Score)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetUserRankAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Get the user's best score
        var userBestScore = await _context.LeaderboardEntries
            .Where(le => le.UserId == userId)
            .MaxAsync(le => (int?)le.Score, cancellationToken);

        if (userBestScore == null)
        {
            return 0; // User has no scores
        }

        // Count how many unique users have a higher best score
        // Using Where + Count instead of CountAsync(predicate) for better EF Core translation
        var usersWithHigherScore = await _context.LeaderboardEntries
            .GroupBy(le => le.UserId)
            .Select(g => g.Max(le => le.Score))
            .Where(maxScore => maxScore > userBestScore)
            .CountAsync(cancellationToken);

        return usersWithHigherScore + 1;
    }

    public async Task<int> GetScoreRankAsync(int score, CancellationToken cancellationToken = default)
    {
        // Count how many entries have a higher score
        var entriesWithHigherScore = await _context.LeaderboardEntries
            .Where(le => le.Score > score)
            .CountAsync(cancellationToken);

        return entriesWithHigherScore + 1;
    }

    public async Task<LeaderboardEntry> AddAsync(LeaderboardEntry entry, CancellationToken cancellationToken = default)
    {
        _context.LeaderboardEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
        return entry;
    }

    public async Task<bool> ExistsByGameSessionIdAsync(Guid gameSessionId, CancellationToken cancellationToken = default)
    {
        return await _context.LeaderboardEntries
            .AnyAsync(le => le.GameSessionId == gameSessionId, cancellationToken);
    }

    public async Task<int> GetUserEntryCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.LeaderboardEntries
            .CountAsync(le => le.UserId == userId, cancellationToken);
    }
}
