using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(256);

        builder.Property(u => u.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.Avatar)
            .HasColumnName("avatar")
            .HasMaxLength(10);

        builder.Property(u => u.Provider)
            .HasColumnName("provider")
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(u => u.HighScore)
            .HasColumnName("high_score")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.GamesPlayed)
            .HasColumnName("games_played")
            .HasDefaultValue(0)
            .IsRequired();

        // Unique index on ExternalId + Provider for OAuth lookups
        builder.HasIndex(u => new { u.ExternalId, u.Provider })
            .IsUnique()
            .HasDatabaseName("ix_users_external_id_provider");

        // Unique index on Username
        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("ix_users_username");

        // Index on HighScore for leaderboard queries (descending)
        builder.HasIndex(u => u.HighScore)
            .IsDescending()
            .HasDatabaseName("ix_users_high_score");

        // Navigation
        builder.HasMany(u => u.GameSessions)
            .WithOne(gs => gs.User)
            .HasForeignKey(gs => gs.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
