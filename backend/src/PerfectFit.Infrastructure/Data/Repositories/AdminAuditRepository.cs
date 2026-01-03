using Microsoft.EntityFrameworkCore;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.Infrastructure.Data.Repositories;

public class AdminAuditRepository : IAdminAuditRepository
{
    private readonly AppDbContext _context;

    public AdminAuditRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AdminAuditLog log, CancellationToken cancellationToken = default)
    {
        _context.AdminAuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<AdminAuditLog>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.AdminAuditLogs
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AdminAuditLogs.CountAsync(cancellationToken);
    }
}
