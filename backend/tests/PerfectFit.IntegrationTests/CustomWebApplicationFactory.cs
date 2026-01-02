using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PerfectFit.Infrastructure.Data;

namespace PerfectFit.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration tests that uses in-memory database.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;

    public CustomWebApplicationFactory()
    {
        _databaseName = "TestDb_" + Guid.NewGuid();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Remove ALL DbContext-related registrations
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true
            ).ToList();

            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            // Re-register AppDbContext with in-memory provider
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            }, ServiceLifetime.Scoped);
        });
    }
}
