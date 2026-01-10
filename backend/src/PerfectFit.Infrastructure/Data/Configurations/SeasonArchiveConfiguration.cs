using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class SeasonArchiveConfiguration : IEntityTypeConfiguration<SeasonArchive>
{
    public void Configure(EntityTypeBuilder<SeasonArchive> builder)
    {
        builder.ToTable("season_archives");

        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(sa => sa.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(sa => sa.SeasonId)
            .HasColumnName("season_id")
            .IsRequired();

        builder.Property(sa => sa.FinalXP)
            .HasColumnName("final_xp")
            .IsRequired();

        builder.Property(sa => sa.FinalTier)
            .HasColumnName("final_tier")
            .IsRequired();

        builder.Property(sa => sa.ArchivedAt)
            .HasColumnName("archived_at")
            .IsRequired();

        // Unique constraint to prevent duplicate archives for the same user and season
        // This prevents race conditions when multiple instances try to archive the same user
        builder.HasIndex(sa => new { sa.UserId, sa.SeasonId })
            .IsUnique()
            .HasDatabaseName("ix_season_archives_user_season_unique");

        // Navigation
        builder.HasOne(sa => sa.User)
            .WithMany()
            .HasForeignKey(sa => sa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sa => sa.Season)
            .WithMany()
            .HasForeignKey(sa => sa.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
