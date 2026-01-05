using Microsoft.EntityFrameworkCore;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Infrastructure.Data;

namespace PerfectFit.Infrastructure.Data.Repositories;

public class GameSessionRepository : IGameSessionRepository
{
    private readonly AppDbContext _context;

    public GameSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GameSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == id, cancellationToken);
    }

    public async Task<GameSession> AddAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        _context.GameSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task UpdateAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        _context.GameSessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<GameSession>> GetActiveSessionsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.GameSessions
            .Where(gs => gs.UserId == userId && gs.Status == GameStatus.Playing)
            .ToListAsync(cancellationToken);
    }
}
