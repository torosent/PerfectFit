using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.UnitTests.Entities;

public class UserGamificationTests
{
    private User CreateTestUser() =>
        User.Create("test-external-id", "test@example.com", "Test_User", AuthProvider.Local);

    #region Streak Tests

    [Fact]
    public void User_UpdateStreak_IncrementsStreak_WhenPlayedToday()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetTimezone("UTC");
        var today = DateTime.UtcNow.Date;

        // Act
        user.UpdateStreak(today);

        // Assert
        user.CurrentStreak.Should().Be(1);
        user.LastPlayedDate.Should().Be(today);
    }

    [Fact]
    public void User_UpdateStreak_IncrementsStreak_WhenPlayedConsecutiveDays()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetTimezone("UTC");
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var today = DateTime.UtcNow.Date;

        // Play yesterday first
        user.UpdateStreak(yesterday);
        user.CurrentStreak.Should().Be(1);

        // Act - play today
        user.UpdateStreak(today);

        // Assert
        user.CurrentStreak.Should().Be(2);
        user.LastPlayedDate.Should().Be(today);
    }

    [Fact]
    public void User_UpdateStreak_DoesNotIncrement_WhenPlayedSameDay()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetTimezone("UTC");
        var today = DateTime.UtcNow.Date;

        // Play today once
        user.UpdateStreak(today);
        user.CurrentStreak.Should().Be(1);

        // Act - play today again
        user.UpdateStreak(today);

        // Assert - streak should not increase
        user.CurrentStreak.Should().Be(1);
    }

    [Fact]
    public void User_UpdateStreak_ResetsWhenMissedDay()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetTimezone("UTC");
        var twoDaysAgo = DateTime.UtcNow.Date.AddDays(-2);
        var today = DateTime.UtcNow.Date;

        // Build a streak
        user.UpdateStreak(twoDaysAgo);

        // Act - skip a day and play today (missed yesterday)
        user.UpdateStreak(today);

        // Assert - streak should reset to 1
        user.CurrentStreak.Should().Be(1);
        user.LastPlayedDate.Should().Be(today);
    }

    [Fact]
    public void User_UpdateStreak_UpdatesLongestStreak()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetTimezone("UTC");
        var threeDaysAgo = DateTime.UtcNow.Date.AddDays(-3);
        var twoDaysAgo = DateTime.UtcNow.Date.AddDays(-2);
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        // Build a 3-day streak
        user.UpdateStreak(threeDaysAgo);
        user.UpdateStreak(twoDaysAgo);
        user.UpdateStreak(yesterday);

        // Assert
        user.CurrentStreak.Should().Be(3);
        user.LongestStreak.Should().Be(3);
    }

    #endregion

    #region Streak Freeze Tests

    [Fact]
    public void User_UseStreakFreeze_DecreasesTokens()
    {
        // Arrange
        var user = CreateTestUser();
        user.AddStreakFreezeTokens(2);
        user.StreakFreezeTokens.Should().Be(2);

        // Act
        var result = user.UseStreakFreeze();

        // Assert
        result.Should().BeTrue();
        user.StreakFreezeTokens.Should().Be(1);
    }

    [Fact]
    public void User_UseStreakFreeze_FailsWhenNoTokens()
    {
        // Arrange
        var user = CreateTestUser();
        user.StreakFreezeTokens.Should().Be(0);

        // Act
        var result = user.UseStreakFreeze();

        // Assert
        result.Should().BeFalse();
        user.StreakFreezeTokens.Should().Be(0);
    }

    [Fact]
    public void User_AddStreakFreezeTokens_IncreasesTokens()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.AddStreakFreezeTokens(3);

        // Assert
        user.StreakFreezeTokens.Should().Be(3);
    }

    [Fact]
    public void User_AddStreakFreezeTokens_AccumulatesTokens()
    {
        // Arrange
        var user = CreateTestUser();
        user.AddStreakFreezeTokens(2);

        // Act
        user.AddStreakFreezeTokens(3);

        // Assert
        user.StreakFreezeTokens.Should().Be(5);
    }

    #endregion

    #region Season XP Tests

    [Fact]
    public void User_AddSeasonXP_IncreasesXP()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.AddSeasonXP(100);

        // Assert
        user.SeasonPassXP.Should().Be(100);
    }

    [Fact]
    public void User_AddSeasonXP_UpdatesTier()
    {
        // Arrange
        var user = CreateTestUser();
        user.CurrentSeasonTier.Should().Be(0);

        // Act - add enough XP for tier 1 (assume 1000 XP per tier)
        user.AddSeasonXP(1000);

        // Assert
        user.SeasonPassXP.Should().Be(1000);
        user.CurrentSeasonTier.Should().Be(1);
    }

    [Fact]
    public void User_AddSeasonXP_UpdatesMultipleTiers()
    {
        // Arrange
        var user = CreateTestUser();

        // Act - add enough XP for 3 tiers
        user.AddSeasonXP(3500);

        // Assert
        user.SeasonPassXP.Should().Be(3500);
        user.CurrentSeasonTier.Should().Be(3);
    }

    [Fact]
    public void User_ResetSeasonProgress_ClearsXPAndTier()
    {
        // Arrange
        var user = CreateTestUser();
        user.AddSeasonXP(5000);
        user.CurrentSeasonTier.Should().BeGreaterThan(0);

        // Act
        user.ResetSeasonProgress();

        // Assert
        user.SeasonPassXP.Should().Be(0);
        user.CurrentSeasonTier.Should().Be(0);
    }

    #endregion

    #region Cosmetic Equipment Tests

    [Fact]
    public void User_EquipCosmetic_SetsCorrectField_BoardTheme()
    {
        // Arrange
        var user = CreateTestUser();
        var cosmeticId = 42;

        // Act
        user.EquipCosmetic(CosmeticType.BoardTheme, cosmeticId);

        // Assert
        user.EquippedBoardThemeId.Should().Be(cosmeticId);
    }

    [Fact]
    public void User_EquipCosmetic_SetsCorrectField_AvatarFrame()
    {
        // Arrange
        var user = CreateTestUser();
        var cosmeticId = 15;

        // Act
        user.EquipCosmetic(CosmeticType.AvatarFrame, cosmeticId);

        // Assert
        user.EquippedAvatarFrameId.Should().Be(cosmeticId);
    }

    [Fact]
    public void User_EquipCosmetic_SetsCorrectField_Badge()
    {
        // Arrange
        var user = CreateTestUser();
        var cosmeticId = 99;

        // Act
        user.EquipCosmetic(CosmeticType.Badge, cosmeticId);

        // Assert
        user.EquippedBadgeId.Should().Be(cosmeticId);
    }

    [Fact]
    public void User_EquipCosmetic_AllowsNull_ToUnequip()
    {
        // Arrange
        var user = CreateTestUser();
        user.EquipCosmetic(CosmeticType.BoardTheme, 42);
        user.EquippedBoardThemeId.Should().Be(42);

        // Act
        user.EquipCosmetic(CosmeticType.BoardTheme, null);

        // Assert
        user.EquippedBoardThemeId.Should().BeNull();
    }

    #endregion

    #region Initialization Tests

    [Fact]
    public void User_Create_InitializesGamificationFieldsToDefaults()
    {
        // Arrange & Act
        var user = CreateTestUser();

        // Assert
        user.CurrentStreak.Should().Be(0);
        user.LongestStreak.Should().Be(0);
        user.StreakFreezeTokens.Should().Be(0);
        user.LastPlayedDate.Should().BeNull();
        user.Timezone.Should().BeNull();
        user.SeasonPassXP.Should().Be(0);
        user.CurrentSeasonTier.Should().Be(0);
        user.EquippedBoardThemeId.Should().BeNull();
        user.EquippedAvatarFrameId.Should().BeNull();
        user.EquippedBadgeId.Should().BeNull();
    }

    [Fact]
    public void User_SetTimezone_SetsTimezone()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.SetTimezone("America/New_York");

        // Assert
        user.Timezone.Should().Be("America/New_York");
    }

    #endregion
}
