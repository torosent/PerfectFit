using System.Collections.Concurrent;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.Infrastructure.Data.InMemory;

/// <summary>
/// In-memory implementation of IUserRepository for testing without a database.
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<int, User> _users = new();
    private int _nextId = 1;
    private readonly object _idLock = new();

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(id, out var user);
        if (user?.IsDeleted == true)
            return Task.FromResult<User?>(null);
        return Task.FromResult(user);
    }

    public Task<User?> GetByExternalIdAsync(string externalId, AuthProvider provider, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u =>
            u.ExternalId == externalId &&
            u.Provider == provider &&
            !u.IsDeleted);
        return Task.FromResult(user);
    }

    public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        lock (_idLock)
        {
            // Use reflection to set the Id since it's private
            var idProperty = typeof(User).GetProperty(nameof(User.Id));
            idProperty?.SetValue(user, _nextId);
            _users[_nextId] = user;
            _nextId++;
        }
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _users.TryRemove(user.Id, out _);
        return Task.CompletedTask;
    }

    public Task<bool> IsDisplayNameTakenAsync(string displayName, int excludeUserId, CancellationToken cancellationToken = default)
    {
        var taken = _users.Values.Any(u =>
            u.DisplayName == displayName &&
            u.Id != excludeUserId &&
            !u.IsDeleted);
        return Task.FromResult(taken);
    }

    public Task<IEnumerable<User>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var users = _users.Values
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        return Task.FromResult(users);
    }

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<User> users = _users.Values
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.Id)
            .ToList();
        return Task.FromResult(users);
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        var count = _users.Values.Count(u => !u.IsDeleted);
        return Task.FromResult(count);
    }

    public Task<IEnumerable<User>> GetDeletedUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var users = _users.Values
            .Where(u => u.IsDeleted)
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        return Task.FromResult(users);
    }

    public Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (_users.TryGetValue(id, out var user))
        {
            user.SoftDelete();
        }
        return Task.CompletedTask;
    }

    public Task<int> BulkSoftDeleteByProviderAsync(AuthProvider provider, CancellationToken cancellationToken = default)
    {
        var count = 0;
        foreach (var user in _users.Values.Where(u => u.Provider == provider && !u.IsDeleted))
        {
            user.SoftDelete();
            count++;
        }
        return Task.FromResult(count);
    }

    public Task<IReadOnlyList<User>> GetUsersWithActiveStreaksAsync(CancellationToken cancellationToken = default)
    {
        var users = _users.Values
            .Where(u => u.CurrentStreak > 0 && !u.IsDeleted)
            .ToList();
        return Task.FromResult<IReadOnlyList<User>>(users);
    }

    public Task<bool> TryClaimStreakNotificationAsync(int userId, int cooldownHours, CancellationToken cancellationToken = default)
    {
        var threshold = DateTime.UtcNow.AddHours(-cooldownHours);
        
        if (!_users.TryGetValue(userId, out var user))
        {
            return Task.FromResult(false);
        }
        
        // Check if already notified within cooldown using atomic compare-and-swap semantics
        // Note: ConcurrentDictionary doesn't provide true atomic updates, but this is sufficient for testing
        lock (_idLock)
        {
            if (user.LastStreakNotificationSentAt != null && user.LastStreakNotificationSentAt >= threshold)
            {
                return Task.FromResult(false);
            }
            
            user.RecordStreakNotificationSent();
            return Task.FromResult(true);
        }
    }
}
