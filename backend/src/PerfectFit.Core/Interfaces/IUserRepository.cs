using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByExternalIdAsync(string externalId, AuthProvider provider, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameTakenAsync(string username, int excludeUserId, CancellationToken cancellationToken = default);
}
