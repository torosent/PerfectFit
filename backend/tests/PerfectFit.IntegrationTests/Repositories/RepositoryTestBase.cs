using Microsoft.EntityFrameworkCore;
using PerfectFit.Infrastructure.Data;

namespace PerfectFit.IntegrationTests.Repositories;

public abstract class RepositoryTestBase : IDisposable
{
    protected readonly AppDbContext DbContext;
    private bool _disposed;

    protected RepositoryTestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new AppDbContext(options);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DbContext.Dispose();
            }
            _disposed = true;
        }
    }
}
