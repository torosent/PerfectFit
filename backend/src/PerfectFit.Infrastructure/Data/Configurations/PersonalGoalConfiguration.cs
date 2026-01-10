using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class PersonalGoalConfiguration : IEntityTypeConfiguration<PersonalGoal>
{
    public void Configure(EntityTypeBuilder<PersonalGoal> builder)
    {
        builder.ToTable("personal_goals");

        builder.HasKey(pg => pg.Id);

        builder.Property(pg => pg.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(pg => pg.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(pg => pg.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(pg => pg.TargetValue)
            .HasColumnName("target_value")
            .IsRequired();

        builder.Property(pg => pg.CurrentValue)
            .HasColumnName("current_value")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(pg => pg.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(pg => pg.IsCompleted)
            .HasColumnName("is_completed")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(pg => pg.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(pg => pg.ExpiresAt)
            .HasColumnName("expires_at");

        // Ignore computed property
        builder.Ignore(pg => pg.IsExpired);

        // Index on UserId for finding user's goals
        builder.HasIndex(pg => pg.UserId)
            .HasDatabaseName("ix_personal_goals_user");

        // Index for finding active goals
        builder.HasIndex(pg => new { pg.UserId, pg.IsCompleted })
            .HasDatabaseName("ix_personal_goals_user_completed");
    }
}
