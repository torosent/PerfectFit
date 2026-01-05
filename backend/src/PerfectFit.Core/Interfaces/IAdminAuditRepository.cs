using PerfectFit.Core.Entities;

namespace PerfectFit.Core.Interfaces;

public interface IAdminAuditRepository
{
    Task AddAsync(AdminAuditLog log, CancellationToken cancellationToken = default);
    Task<IEnumerable<AdminAuditLog>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}
