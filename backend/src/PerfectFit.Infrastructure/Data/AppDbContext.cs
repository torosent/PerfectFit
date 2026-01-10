using Microsoft.EntityFrameworkCore;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<LeaderboardEntry> LeaderboardEntries => Set<LeaderboardEntry>();
    public DbSet<AdminAuditLog> AdminAuditLogs => Set<AdminAuditLog>();

    // Gamification entities
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<UserChallenge> UserChallenges => Set<UserChallenge>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<SeasonReward> SeasonRewards => Set<SeasonReward>();
    public DbSet<Cosmetic> Cosmetics => Set<Cosmetic>();
    public DbSet<UserCosmetic> UserCosmetics => Set<UserCosmetic>();
    public DbSet<PersonalGoal> PersonalGoals => Set<PersonalGoal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
