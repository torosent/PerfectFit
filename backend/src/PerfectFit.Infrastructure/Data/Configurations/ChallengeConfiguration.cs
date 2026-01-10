using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class ChallengeConfiguration : IEntityTypeConfiguration<Challenge>
{
    public void Configure(EntityTypeBuilder<Challenge> builder)
    {
        builder.ToTable("challenges");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(c => c.TargetValue)
            .HasColumnName("target_value")
            .IsRequired();

        builder.Property(c => c.XPReward)
            .HasColumnName("xp_reward")
            .IsRequired();

        builder.Property(c => c.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(c => c.EndDate)
            .HasColumnName("end_date")
            .IsRequired();

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        // Index on Type for filtering daily/weekly
        builder.HasIndex(c => c.Type)
            .HasDatabaseName("ix_challenges_type");

        // Index on IsActive and date range for finding current challenges
        builder.HasIndex(c => new { c.IsActive, c.StartDate, c.EndDate })
            .HasDatabaseName("ix_challenges_active_dates");

        // Navigation
        builder.HasMany(c => c.UserChallenges)
            .WithOne(uc => uc.Challenge)
            .HasForeignKey(uc => uc.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
