using System.Collections.Concurrent;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.Infrastructure.Data.InMemory;

/// <summary>
/// In-memory implementation of IGameSessionRepository for testing without a database.
/// </summary>
public class InMemoryGameSessionRepository : IGameSessionRepository
{
    private readonly ConcurrentDictionary<Guid, GameSession> _sessions = new();

    public Task<GameSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _sessions.TryGetValue(id, out var session);
        return Task.FromResult(session);
    }

    public Task<GameSession> AddAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        _sessions[session.Id] = session;
        return Task.FromResult(session);
    }

    public Task UpdateAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        _sessions[session.Id] = session;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<GameSession>> GetActiveSessionsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var sessions = _sessions.Values
            .Where(gs => gs.UserId == userId && gs.Status == GameStatus.Playing);
        return Task.FromResult(sessions);
    }
}
