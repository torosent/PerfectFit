using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class SeasonRewardConfiguration : IEntityTypeConfiguration<SeasonReward>
{
    public void Configure(EntityTypeBuilder<SeasonReward> builder)
    {
        builder.ToTable("season_rewards");

        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(sr => sr.SeasonId)
            .HasColumnName("season_id")
            .IsRequired();

        builder.Property(sr => sr.Tier)
            .HasColumnName("tier")
            .IsRequired();

        builder.Property(sr => sr.RewardType)
            .HasColumnName("reward_type")
            .IsRequired();

        builder.Property(sr => sr.RewardValue)
            .HasColumnName("reward_value")
            .IsRequired();

        builder.Property(sr => sr.XPRequired)
            .HasColumnName("xp_required")
            .IsRequired();

        // Unique composite index - one reward per tier per season
        builder.HasIndex(sr => new { sr.SeasonId, sr.Tier })
            .IsUnique()
            .HasDatabaseName("ix_season_rewards_season_tier");
    }
}
