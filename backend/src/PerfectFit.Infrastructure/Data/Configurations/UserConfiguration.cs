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

        builder.Property(u => u.LastDisplayNameChangeAt)
            .HasColumnName("last_display_name_change_at");

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .IsRequired();

        builder.Property(u => u.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(256);

        builder.Property(u => u.EmailVerified)
            .HasColumnName("email_verified")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(u => u.EmailVerificationToken)
            .HasColumnName("email_verification_token")
            .HasMaxLength(64);

        builder.Property(u => u.EmailVerificationTokenExpiry)
            .HasColumnName("email_verification_token_expiry");

        builder.Property(u => u.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.LockoutEnd)
            .HasColumnName("lockout_end");

        // Gamification - Streak fields
        builder.Property(u => u.CurrentStreak)
            .HasColumnName("current_streak")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.LongestStreak)
            .HasColumnName("longest_streak")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.StreakFreezeTokens)
            .HasColumnName("streak_freeze_tokens")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.LastPlayedDate)
            .HasColumnName("last_played_date");

        builder.Property(u => u.Timezone)
            .HasColumnName("timezone")
            .HasMaxLength(64);

        // Gamification - Season pass fields
        builder.Property(u => u.SeasonPassXP)
            .HasColumnName("season_pass_xp")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.CurrentSeasonTier)
            .HasColumnName("current_season_tier")
            .HasDefaultValue(0)
            .IsRequired();

        // Gamification - Cosmetic fields
        builder.Property(u => u.EquippedBoardThemeId)
            .HasColumnName("equipped_board_theme_id");

        builder.Property(u => u.EquippedAvatarFrameId)
            .HasColumnName("equipped_avatar_frame_id");

        builder.Property(u => u.EquippedBadgeId)
            .HasColumnName("equipped_badge_id");

        builder.Property(u => u.LastStreakNotificationSentAt)
            .HasColumnName("last_streak_notification_sent_at");

        // Gamification - Achievement tracking fields
        builder.Property(u => u.TotalWins)
            .HasColumnName("total_wins")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.CurrentWinStreak)
            .HasColumnName("current_win_streak")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.PerfectGames)
            .HasColumnName("perfect_games")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.HighAccuracyGames)
            .HasColumnName("high_accuracy_games")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.FastGames)
            .HasColumnName("fast_games")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.NightGames)
            .HasColumnName("night_games")
            .HasDefaultValue(0)
            .IsRequired();

        // Global query filter for soft delete - excludes deleted users by default
        builder.HasQueryFilter(u => !u.IsDeleted);

        // Unique index on ExternalId + Provider for OAuth lookups
        builder.HasIndex(u => new { u.ExternalId, u.Provider })
            .IsUnique()
            .HasDatabaseName("ix_users_external_id_provider");

        // Unique index on DisplayName
        builder.HasIndex(u => u.DisplayName)
            .IsUnique()
            .HasDatabaseName("ix_users_display_name");

        // Index on HighScore for leaderboard queries (descending)
        builder.HasIndex(u => u.HighScore)
            .IsDescending()
            .HasDatabaseName("ix_users_high_score");

        // Navigation
        builder.HasMany(u => u.GameSessions)
            .WithOne(gs => gs.User)
            .HasForeignKey(gs => gs.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Gamification navigation properties
        builder.HasMany(u => u.UserAchievements)
            .WithOne(ua => ua.User)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserChallenges)
            .WithOne(uc => uc.User)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserCosmetics)
            .WithOne(uco => uco.User)
            .HasForeignKey(uco => uco.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.PersonalGoals)
            .WithOne(pg => pg.User)
            .HasForeignKey(pg => pg.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
