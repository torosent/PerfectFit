using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.ToTable("game_sessions");

        builder.HasKey(gs => gs.Id);

        builder.Property(gs => gs.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(gs => gs.UserId)
            .HasColumnName("user_id");

        builder.Property(gs => gs.BoardState)
            .HasColumnName("board_state")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(gs => gs.CurrentPieces)
            .HasColumnName("current_pieces")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(gs => gs.PieceBagState)
            .HasColumnName("piece_bag_state")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(gs => gs.Score)
            .HasColumnName("score")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(gs => gs.Combo)
            .HasColumnName("combo")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(gs => gs.LinesCleared)
            .HasColumnName("lines_cleared")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(gs => gs.MaxCombo)
            .HasColumnName("max_combo")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(gs => gs.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(gs => gs.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(gs => gs.EndedAt)
            .HasColumnName("ended_at");

        builder.Property(gs => gs.LastActivityAt)
            .HasColumnName("last_activity_at")
            .IsRequired();

        // Index on UserId for user's game history
        builder.HasIndex(gs => gs.UserId)
            .HasDatabaseName("ix_game_sessions_user_id");

        // Index on Status for active games queries
        builder.HasIndex(gs => gs.Status)
            .HasDatabaseName("ix_game_sessions_status");

        // Composite index for finding user's active sessions
        builder.HasIndex(gs => new { gs.UserId, gs.Status })
            .HasDatabaseName("ix_game_sessions_user_id_status");

        // Navigation
        builder.HasOne(gs => gs.User)
            .WithMany(u => u.GameSessions)
            .HasForeignKey(gs => gs.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
