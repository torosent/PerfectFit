using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;
using PerfectFit.Infrastructure.Data;
using PerfectFit.Infrastructure.Data.Repositories;
using PerfectFit.Infrastructure.Identity;
using PerfectFit.Infrastructure.Services;

namespace PerfectFit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGameSessionRepository, GameSessionRepository>();
        services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();

        // Configure JWT settings and service
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtService, JwtService>();

        // Configure OAuth settings
        services.Configure<OAuthSettings>(configuration.GetSection(OAuthSettings.SectionName));

        // Register domain services
        services.AddScoped<IScoreValidationService, ScoreValidationService>();

        return services;
    }
}
