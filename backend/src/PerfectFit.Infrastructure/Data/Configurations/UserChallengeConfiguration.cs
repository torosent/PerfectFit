using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class UserChallengeConfiguration : IEntityTypeConfiguration<UserChallenge>
{
    public void Configure(EntityTypeBuilder<UserChallenge> builder)
    {
        builder.ToTable("user_challenges");

        builder.HasKey(uc => uc.Id);

        builder.Property(uc => uc.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(uc => uc.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(uc => uc.ChallengeId)
            .HasColumnName("challenge_id")
            .IsRequired();

        builder.Property(uc => uc.CurrentProgress)
            .HasColumnName("current_progress")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(uc => uc.IsCompleted)
            .HasColumnName("is_completed")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(uc => uc.CompletedAt)
            .HasColumnName("completed_at");

        // Unique composite index - user can only have one record per challenge
        builder.HasIndex(uc => new { uc.UserId, uc.ChallengeId })
            .IsUnique()
            .HasDatabaseName("ix_user_challenges_user_challenge");

        // Index for finding user's completed challenges
        builder.HasIndex(uc => new { uc.UserId, uc.IsCompleted })
            .HasDatabaseName("ix_user_challenges_user_completed");
    }
}
