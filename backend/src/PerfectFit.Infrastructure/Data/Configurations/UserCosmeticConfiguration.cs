using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class UserCosmeticConfiguration : IEntityTypeConfiguration<UserCosmetic>
{
    public void Configure(EntityTypeBuilder<UserCosmetic> builder)
    {
        builder.ToTable("user_cosmetics");

        builder.HasKey(uc => uc.Id);

        builder.Property(uc => uc.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(uc => uc.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(uc => uc.CosmeticId)
            .HasColumnName("cosmetic_id")
            .IsRequired();

        builder.Property(uc => uc.ObtainedAt)
            .HasColumnName("obtained_at")
            .IsRequired();

        builder.Property(uc => uc.ObtainedFrom)
            .HasColumnName("obtained_from")
            .IsRequired();

        // Unique composite index - user can only own a cosmetic once
        builder.HasIndex(uc => new { uc.UserId, uc.CosmeticId })
            .IsUnique()
            .HasDatabaseName("ix_user_cosmetics_user_cosmetic");

        // Index for finding user's cosmetics by type (via join with cosmetics table)
        builder.HasIndex(uc => uc.UserId)
            .HasDatabaseName("ix_user_cosmetics_user");
    }
}
