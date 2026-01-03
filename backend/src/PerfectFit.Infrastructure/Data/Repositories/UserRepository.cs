using Microsoft.EntityFrameworkCore;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Infrastructure.Data;

namespace PerfectFit.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByExternalIdAsync(string externalId, AuthProvider provider, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.ExternalId == externalId && u.Provider == provider, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsUsernameTakenAsync(string username, int excludeUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username && u.Id != excludeUserId, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetDeletedUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.IsDeleted)
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        
        if (user != null)
        {
            user.SoftDelete();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> BulkSoftDeleteByProviderAsync(AuthProvider provider, CancellationToken cancellationToken = default)
    {
        // Note: ExecuteUpdateAsync is the most efficient for production databases,
        // but falls back to entity-based update for in-memory provider compatibility
        try
        {
            return await _context.Users
                .Where(u => u.Provider == provider)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(u => u.IsDeleted, true)
                          .SetProperty(u => u.DeletedAt, DateTime.UtcNow),
                    cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // Fallback for in-memory database provider (used in tests)
            var users = await _context.Users
                .Where(u => u.Provider == provider)
                .ToListAsync(cancellationToken);
            
            foreach (var user in users)
            {
                user.SoftDelete();
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            return users.Count;
        }
    }
}
