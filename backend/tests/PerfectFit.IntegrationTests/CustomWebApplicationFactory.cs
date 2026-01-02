using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using PerfectFit.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace PerfectFit.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration tests that uses in-memory database.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;
    
    // Use the same JWT secret as in appsettings.json
    private const string JwtSecret = "your-256-bit-secret-key-here-minimum-32-characters-long-for-security";

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

    /// <summary>
    /// Creates an HTTP client with a valid JWT token for the specified user.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(int userId, string displayName, string provider)
    {
        var client = CreateClient();
        var token = GenerateTestJwtToken(userId, displayName, provider);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string GenerateTestJwtToken(int userId, string displayName, string provider)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, displayName),
            new Claim("provider", provider)
        };

        var token = new JwtSecurityToken(
            issuer: "PerfectFit",
            audience: "PerfectFit",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
