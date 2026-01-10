using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> builder)
    {
        builder.ToTable("seasons");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Number)
            .HasColumnName("number")
            .IsRequired();

        builder.Property(s => s.Theme)
            .HasColumnName("theme")
            .HasMaxLength(100);

        builder.Property(s => s.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(s => s.EndDate)
            .HasColumnName("end_date")
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        // Unique index on season number
        builder.HasIndex(s => s.Number)
            .IsUnique()
            .HasDatabaseName("ix_seasons_number");

        // Index on IsActive for finding current season
        builder.HasIndex(s => s.IsActive)
            .HasDatabaseName("ix_seasons_active");

        // Navigation
        builder.HasMany(s => s.Rewards)
            .WithOne(sr => sr.Season)
            .HasForeignKey(sr => sr.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
