using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("achievements");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.Category)
            .HasColumnName("category")
            .IsRequired();

        builder.Property(a => a.IconUrl)
            .HasColumnName("icon_url")
            .HasMaxLength(500);

        builder.Property(a => a.UnlockCondition)
            .HasColumnName("unlock_condition")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(a => a.RewardType)
            .HasColumnName("reward_type")
            .IsRequired();

        builder.Property(a => a.RewardValue)
            .HasColumnName("reward_value")
            .IsRequired();

        builder.Property(a => a.IsSecret)
            .HasColumnName("is_secret")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0)
            .IsRequired();

        // Unique index on Name
        builder.HasIndex(a => a.Name)
            .IsUnique()
            .HasDatabaseName("ix_achievements_name");

        // Index on Category for filtering
        builder.HasIndex(a => a.Category)
            .HasDatabaseName("ix_achievements_category");

        // Navigation
        builder.HasMany(a => a.UserAchievements)
            .WithOne(ua => ua.Achievement)
            .HasForeignKey(ua => ua.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
