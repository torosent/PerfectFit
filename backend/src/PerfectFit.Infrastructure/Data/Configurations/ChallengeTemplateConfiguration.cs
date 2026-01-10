using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class ChallengeTemplateConfiguration : IEntityTypeConfiguration<ChallengeTemplate>
{
    public void Configure(EntityTypeBuilder<ChallengeTemplate> builder)
    {
        builder.ToTable("challenge_templates");

        builder.HasKey(ct => ct.Id);

        builder.Property(ct => ct.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(ct => ct.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ct => ct.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(ct => ct.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(ct => ct.TargetValue)
            .HasColumnName("target_value")
            .IsRequired();

        builder.Property(ct => ct.XPReward)
            .HasColumnName("xp_reward")
            .IsRequired();

        builder.Property(ct => ct.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        // Index on Type for filtering daily/weekly templates
        builder.HasIndex(ct => ct.Type)
            .HasDatabaseName("ix_challenge_templates_type");

        // Index on IsActive for quickly finding active templates
        builder.HasIndex(ct => ct.IsActive)
            .HasDatabaseName("ix_challenge_templates_is_active");

        // Composite index on Type and IsActive for efficient filtering
        builder.HasIndex(ct => new { ct.Type, ct.IsActive })
            .HasDatabaseName("ix_challenge_templates_type_is_active");
    }
}
