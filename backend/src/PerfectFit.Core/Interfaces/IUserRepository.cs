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
    Task<bool> IsDisplayNameTakenAsync(string displayName, int excludeUserId, CancellationToken cancellationToken = default);
    
    // Admin methods
    Task<IEnumerable<User>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetDeletedUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> BulkSoftDeleteByProviderAsync(AuthProvider provider, CancellationToken cancellationToken = default);
}
