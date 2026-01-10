using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.ToTable("user_achievements");

        builder.HasKey(ua => ua.Id);

        builder.Property(ua => ua.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(ua => ua.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ua => ua.AchievementId)
            .HasColumnName("achievement_id")
            .IsRequired();

        builder.Property(ua => ua.UnlockedAt)
            .HasColumnName("unlocked_at");

        builder.Property(ua => ua.Progress)
            .HasColumnName("progress")
            .HasDefaultValue(0)
            .IsRequired();

        // Ignore computed property
        builder.Ignore(ua => ua.IsUnlocked);

        // Unique composite index - user can only have one record per achievement
        builder.HasIndex(ua => new { ua.UserId, ua.AchievementId })
            .IsUnique()
            .HasDatabaseName("ix_user_achievements_user_achievement");

        // Index for finding unlocked achievements
        builder.HasIndex(ua => new { ua.UserId, ua.UnlockedAt })
            .HasDatabaseName("ix_user_achievements_user_unlocked");
    }
}
