using PerfectFit.Core.Entities;

namespace PerfectFit.Core.Interfaces;

public interface IGameSessionRepository
{
    Task<GameSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GameSession> AddAsync(GameSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(GameSession session, CancellationToken cancellationToken = default);
    Task<IEnumerable<GameSession>> GetActiveSessionsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
