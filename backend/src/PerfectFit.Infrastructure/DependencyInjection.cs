using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PerfectFit.Core.Identity;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;
using PerfectFit.Infrastructure.Data;
using PerfectFit.Infrastructure.Data.InMemory;
using PerfectFit.Infrastructure.Data.Repositories;
using PerfectFit.Infrastructure.Email;
using PerfectFit.Infrastructure.Identity;
using PerfectFit.Infrastructure.Services;

namespace PerfectFit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Check if we should use in-memory storage (no database)
        var useInMemory = configuration.GetValue<bool>("UseInMemoryStorage");

        if (useInMemory)
        {
            // Register in-memory repositories as singletons to persist data across requests
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            services.AddSingleton<IGameSessionRepository, InMemoryGameSessionRepository>();
            services.AddSingleton<IAdminAuditRepository, InMemoryAdminAuditRepository>();
            // LeaderboardRepository depends on UserRepository, so register it after
            services.AddSingleton<ILeaderboardRepository, InMemoryLeaderboardRepository>();
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });

            // Register database migration service
            services.AddScoped<DatabaseMigrationService>();

            // Configure database migration settings
            services.Configure<DatabaseMigrationSettings>(configuration.GetSection(DatabaseMigrationSettings.SectionName));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGameSessionRepository, GameSessionRepository>();
            services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
            services.AddScoped<IAdminAuditRepository, AdminAuditRepository>();
        }

        // Configure JWT settings and service
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtService, JwtService>();

        // Configure OAuth settings
        services.Configure<OAuthSettings>(configuration.GetSection(OAuthSettings.SectionName));

        // Configure email settings and service
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<IEmailService, AzureEmailService>();

        // Register domain services
        services.AddScoped<IScoreValidationService, ScoreValidationService>();

        // Register password hasher as singleton (stateless, thread-safe)
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        // Register email verification service as scoped
        services.AddScoped<IEmailVerificationService, EmailVerificationService>();

        // Register profanity checker with HttpClient
        services.AddHttpClient<IProfanityChecker, PurgoMalumProfanityChecker>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5); // Reasonable timeout for external API
        });

        // Register display name validation service
        services.AddScoped<IDisplayNameValidationService, DisplayNameValidationService>();

        return services;
    }

    /// <summary>
    /// Adds the database migration hosted service that runs migrations on startup.
    /// Call this method to enable automatic migrations with retry logic.
    /// </summary>
    public static IServiceCollection AddDatabaseMigration(this IServiceCollection services)
    {
        services.AddHostedService<DatabaseMigrationHostedService>();
        return services;
    }
}
