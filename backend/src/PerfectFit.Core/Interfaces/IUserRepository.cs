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
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetDeletedUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> BulkSoftDeleteByProviderAsync(AuthProvider provider, CancellationToken cancellationToken = default);
    
    // Gamification methods
    Task<IReadOnlyList<User>> GetUsersWithActiveStreaksAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atomically claims the streak notification slot for a user if not already notified within cooldown period.
    /// Uses database-level WHERE clause to prevent race conditions between multiple instances.
    /// </summary>
    /// <param name="userId">The user ID to claim notification for.</param>
    /// <param name="cooldownHours">Hours that must pass since last notification before allowing new one.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the claim was successful (row was updated), false if already claimed by another instance.</returns>
    Task<bool> TryClaimStreakNotificationAsync(int userId, int cooldownHours, CancellationToken cancellationToken = default);
}
