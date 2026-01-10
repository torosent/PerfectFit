using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.UnitTests.Entities;

public class SeasonEntityTests
{
    #region Season Tests

    [Fact]
    public void Season_Create_SetsProperties()
    {
        // Arrange
        var name = "Winter Wonderland";
        var number = 1;
        var theme = "Winter";
        var startDate = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2026, 2, 28, 23, 59, 59, DateTimeKind.Utc);

        // Act
        var season = Season.Create(name, number, theme, startDate, endDate);

        // Assert
        season.Id.Should().Be(0); // Not set until persisted
        season.Name.Should().Be(name);
        season.Number.Should().Be(number);
        season.Theme.Should().Be(theme);
        season.StartDate.Should().Be(startDate);
        season.EndDate.Should().Be(endDate);
        season.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Season_Create_ThrowsWhenNameEmpty()
    {
        // Arrange & Act
        var act = () => Season.Create(
            "",
            1,
            "Theme",
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(3));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Season_Create_ThrowsWhenNumberInvalid()
    {
        // Arrange & Act
        var act = () => Season.Create(
            "Season Name",
            0,
            "Theme",
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(3));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("number");
    }

    [Fact]
    public void Season_Create_ThrowsWhenEndDateBeforeStartDate()
    {
        // Arrange & Act
        var act = () => Season.Create(
            "Season Name",
            1,
            "Theme",
            DateTime.UtcNow.AddMonths(3),
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("endDate");
    }

    [Fact]
    public void Season_Deactivate_SetsIsActiveFalse()
    {
        // Arrange
        var season = Season.Create(
            "Test Season",
            1,
            "Theme",
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(3));

        // Act
        season.Deactivate();

        // Assert
        season.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Season_Activate_SetsIsActiveTrue()
    {
        // Arrange
        var season = Season.Create(
            "Test Season",
            1,
            "Theme",
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(3));
        season.Deactivate();

        // Act
        season.Activate();

        // Assert
        season.IsActive.Should().BeTrue();
    }

    #endregion

    #region SeasonReward Tests

    [Fact]
    public void SeasonReward_Create_SetsProperties()
    {
        // Arrange
        var seasonId = 1;
        var tier = 5;
        var rewardType = RewardType.Cosmetic;
        var rewardValue = 42;
        var xpRequired = 5000;

        // Act
        var reward = SeasonReward.Create(seasonId, tier, rewardType, rewardValue, xpRequired);

        // Assert
        reward.Id.Should().Be(0); // Not set until persisted
        reward.SeasonId.Should().Be(seasonId);
        reward.Tier.Should().Be(tier);
        reward.RewardType.Should().Be(rewardType);
        reward.RewardValue.Should().Be(rewardValue);
        reward.XPRequired.Should().Be(xpRequired);
    }

    [Fact]
    public void SeasonReward_Create_ThrowsWhenTierOutOfRange_TooLow()
    {
        // Arrange & Act
        var act = () => SeasonReward.Create(1, 0, RewardType.Cosmetic, 10, 1000);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("tier");
    }

    [Fact]
    public void SeasonReward_Create_ThrowsWhenTierOutOfRange_TooHigh()
    {
        // Arrange & Act
        var act = () => SeasonReward.Create(1, 11, RewardType.Cosmetic, 10, 1000);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("tier");
    }

    [Fact]
    public void SeasonReward_Create_ThrowsWhenXPRequiredNegative()
    {
        // Arrange & Act
        var act = () => SeasonReward.Create(1, 1, RewardType.Cosmetic, 10, -100);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("xpRequired");
    }

    [Fact]
    public void SeasonReward_Create_AllowsStreakFreezeRewardType()
    {
        // Arrange & Act
        var reward = SeasonReward.Create(1, 3, RewardType.StreakFreeze, 2, 3000);

        // Assert
        reward.RewardType.Should().Be(RewardType.StreakFreeze);
        reward.RewardValue.Should().Be(2); // 2 streak freeze tokens
    }

    [Fact]
    public void SeasonReward_Create_AllowsXPBoostRewardType()
    {
        // Arrange & Act
        var reward = SeasonReward.Create(1, 7, RewardType.XPBoost, 50, 7000);

        // Assert
        reward.RewardType.Should().Be(RewardType.XPBoost);
        reward.RewardValue.Should().Be(50); // 50% XP boost or similar
    }

    #endregion
}
