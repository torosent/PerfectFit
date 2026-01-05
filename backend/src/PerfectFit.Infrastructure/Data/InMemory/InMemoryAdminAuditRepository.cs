using System.Collections.Concurrent;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.Infrastructure.Data.InMemory;

/// <summary>
/// In-memory implementation of IAdminAuditRepository for testing without a database.
/// </summary>
public class InMemoryAdminAuditRepository : IAdminAuditRepository
{
    private readonly ConcurrentDictionary<Guid, AdminAuditLog> _logs = new();

    public Task AddAsync(AdminAuditLog log, CancellationToken cancellationToken = default)
    {
        _logs[log.Id] = log;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<AdminAuditLog>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var logs = _logs.Values
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        return Task.FromResult(logs);
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_logs.Count);
    }
}
