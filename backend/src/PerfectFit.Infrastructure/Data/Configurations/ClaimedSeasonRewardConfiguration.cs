using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class ClaimedSeasonRewardConfiguration : IEntityTypeConfiguration<ClaimedSeasonReward>
{
    public void Configure(EntityTypeBuilder<ClaimedSeasonReward> builder)
    {
        builder.ToTable("claimed_season_rewards");

        builder.HasKey(csr => csr.Id);

        builder.Property(csr => csr.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(csr => csr.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(csr => csr.SeasonRewardId)
            .HasColumnName("season_reward_id")
            .IsRequired();

        builder.Property(csr => csr.ClaimedAt)
            .HasColumnName("claimed_at")
            .IsRequired();

        // Configure relationship with User
        builder.HasOne(csr => csr.User)
            .WithMany()
            .HasForeignKey(csr => csr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with SeasonReward
        builder.HasOne(csr => csr.SeasonReward)
            .WithMany()
            .HasForeignKey(csr => csr.SeasonRewardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique composite index - user can only claim a reward once
        builder.HasIndex(csr => new { csr.UserId, csr.SeasonRewardId })
            .IsUnique()
            .HasDatabaseName("ix_claimed_season_rewards_user_reward");

        // Index for finding user's claimed rewards
        builder.HasIndex(csr => csr.UserId)
            .HasDatabaseName("ix_claimed_season_rewards_user");
    }
}
