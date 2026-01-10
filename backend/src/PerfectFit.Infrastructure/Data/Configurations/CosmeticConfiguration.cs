using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class CosmeticConfiguration : IEntityTypeConfiguration<Cosmetic>
{
    public void Configure(EntityTypeBuilder<Cosmetic> builder)
    {
        builder.ToTable("cosmetics");

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
            .HasMaxLength(500);

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(c => c.AssetUrl)
            .HasColumnName("asset_url")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.PreviewUrl)
            .HasColumnName("preview_url")
            .HasMaxLength(500);

        builder.Property(c => c.Rarity)
            .HasColumnName("rarity")
            .IsRequired();

        builder.Property(c => c.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false)
            .IsRequired();

        // Unique index on Name
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("ix_cosmetics_name");

        // Index on Type for filtering
        builder.HasIndex(c => c.Type)
            .HasDatabaseName("ix_cosmetics_type");

        // Index on IsDefault for finding default cosmetics
        builder.HasIndex(c => c.IsDefault)
            .HasDatabaseName("ix_cosmetics_is_default");

        // Navigation
        builder.HasMany(c => c.UserCosmetics)
            .WithOne(uc => uc.Cosmetic)
            .HasForeignKey(uc => uc.CosmeticId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
