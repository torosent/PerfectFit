using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class LeaderboardEntryConfiguration : IEntityTypeConfiguration<LeaderboardEntry>
{
    public void Configure(EntityTypeBuilder<LeaderboardEntry> builder)
    {
        builder.ToTable("leaderboard_entries");

        builder.HasKey(le => le.Id);

        builder.Property(le => le.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(le => le.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(le => le.Score)
            .HasColumnName("score")
            .IsRequired();

        builder.Property(le => le.LinesCleared)
            .HasColumnName("lines_cleared")
            .IsRequired();

        builder.Property(le => le.MaxCombo)
            .HasColumnName("max_combo")
            .IsRequired();

        builder.Property(le => le.AchievedAt)
            .HasColumnName("achieved_at")
            .IsRequired();

        builder.Property(le => le.GameSessionId)
            .HasColumnName("game_session_id")
            .IsRequired();

        // Index on Score descending for leaderboard queries
        builder.HasIndex(le => le.Score)
            .IsDescending()
            .HasDatabaseName("ix_leaderboard_entries_score");

        // Index on UserId for user's best score lookups
        builder.HasIndex(le => le.UserId)
            .HasDatabaseName("ix_leaderboard_entries_user_id");

        // Composite index for user's best score
        builder.HasIndex(le => new { le.UserId, le.Score })
            .IsDescending(false, true)
            .HasDatabaseName("ix_leaderboard_entries_user_id_score");

        // Navigation to User
        builder.HasOne(le => le.User)
            .WithMany()
            .HasForeignKey(le => le.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation to GameSession
        builder.HasOne(le => le.GameSession)
            .WithMany()
            .HasForeignKey(le => le.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // UNIQUE constraint on GameSessionId to prevent duplicate submissions (defense in depth)
        builder.HasIndex(le => le.GameSessionId)
            .IsUnique()
            .HasDatabaseName("ix_leaderboard_entries_game_session_id_unique");

        // Global query filter to exclude entries from soft-deleted users
        // This matches the User entity's IsDeleted filter to avoid EF Core warning about mismatched filters
        builder.HasQueryFilter(le => !le.User.IsDeleted);
    }
}
