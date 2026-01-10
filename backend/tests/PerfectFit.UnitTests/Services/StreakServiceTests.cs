using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;
using System.Reflection;

namespace PerfectFit.UnitTests.Services;

public class StreakServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly StreakService _service;

    public StreakServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _service = new StreakService(_userRepositoryMock.Object);
    }

    #region UpdateStreakAsync Tests

    [Fact]
    public async Task UpdateStreak_FirstGame_StartsStreak()
    {
        // Arrange
        var user = CreateUser();
        var gameEndTime = new DateTimeOffset(2025, 1, 9, 15, 0, 0, TimeSpan.Zero);

        // Act
        var result = await _service.UpdateStreakAsync(user, gameEndTime);

        // Assert
        result.Success.Should().BeTrue();
        result.NewStreak.Should().Be(1);
        result.StreakBroken.Should().BeFalse();
        result.UsedFreeze.Should().BeFalse();
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStreak_SameDay_NoDoubleCount()
    {
        // Arrange
        var user = CreateUser(currentStreak: 5, lastPlayedDate: new DateTime(2025, 1, 9));
        var gameEndTime = new DateTimeOffset(2025, 1, 9, 18, 0, 0, TimeSpan.Zero); // Same day

        // Act
        var result = await _service.UpdateStreakAsync(user, gameEndTime);

        // Assert
        result.Success.Should().BeTrue();
        result.NewStreak.Should().Be(5); // Unchanged
        result.StreakBroken.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateStreak_NextDay_IncrementsStreak()
    {
        // Arrange
        var user = CreateUser(currentStreak: 5, lastPlayedDate: new DateTime(2025, 1, 8));
        var gameEndTime = new DateTimeOffset(2025, 1, 9, 15, 0, 0, TimeSpan.Zero); // Next day

        // Act
        var result = await _service.UpdateStreakAsync(user, gameEndTime);

        // Assert
        result.Success.Should().BeTrue();
        result.NewStreak.Should().Be(6);
        result.StreakBroken.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateStreak_MissedDay_BreaksStreak()
    {
        // Arrange
        var user = CreateUser(currentStreak: 5, lastPlayedDate: new DateTime(2025, 1, 7));
        var gameEndTime = new DateTimeOffset(2025, 1, 9, 15, 0, 0, TimeSpan.Zero); // Skipped a day

        // Act
        var result = await _service.UpdateStreakAsync(user, gameEndTime);

        // Assert
        result.Success.Should().BeTrue();
        result.NewStreak.Should().Be(1);
        result.StreakBroken.Should().BeTrue();
        result.UsedFreeze.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateStreak_MissedDay_WithFreeze_SavesStreak()
    {
        // Arrange
        var user = CreateUser(currentStreak: 5, lastPlayedDate: new DateTime(2025, 1, 7), freezeTokens: 1);
        var gameEndTime = new DateTimeOffset(2025, 1, 9, 15, 0, 0, TimeSpan.Zero); // Skipped a day

        // Act
        var result = await _service.UpdateStreakAsync(user, gameEndTime);

        // Assert
        result.Success.Should().BeTrue();
        // Freeze covers the missed day (Jan 8) -> streak becomes 6
        // Then playing today (Jan 9) -> streak becomes 7
        result.NewStreak.Should().Be(7);
        result.StreakBroken.Should().BeFalse();
        result.UsedFreeze.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateStreak_MissedMultipleDays_BreaksStreakEvenWithFreeze()
    {
        // Arrange - Missed 2 days, only 1 freeze token
        var user = CreateUser(currentStreak: 5, lastPlayedDate: new DateTime(2025, 1, 6), freezeTokens: 1);
        var gameEndTime = new DateTimeOffset(2025, 1, 9, 15, 0, 0, TimeSpan.Zero); // Skipped 2 days

        // Act
        var result = await _service.UpdateStreakAsync(user, gameEndTime);

        // Assert
        result.Success.Should().BeTrue();
        result.NewStreak.Should().Be(1);
        result.StreakBroken.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateStreak_WithTimezone_CalculatesCorrectDay()
    {
        // Arrange - User in US Pacific (UTC-8)
        var user = CreateUser(currentStreak: 5, lastPlayedDate: new DateTime(2025, 1, 8), timezone: "America/Los_Angeles");
        // Game ended at 2025-01-09 05:00 UTC = 2025-01-08 21:00 Pacific (still same day in user's timezone)
        var gameEndTime = new DateTimeOffset(2025, 1, 9, 5, 0, 0, TimeSpan.Zero);

        // Act
        var result = await _service.UpdateStreakAsync(user, gameEndTime);

        // Assert
        result.Success.Should().BeTrue();
        result.NewStreak.Should().Be(5); // Same day in user's timezone, no increment
    }

    [Fact]
    public async Task UpdateStreak_UpdatesLongestStreak()
    {
        // Arrange
        var user = CreateUser(currentStreak: 9, longestStreak: 9, lastPlayedDate: new DateTime(2025, 1, 8));
        var gameEndTime = new DateTimeOffset(2025, 1, 9, 15, 0, 0, TimeSpan.Zero);

        // Act
        var result = await _service.UpdateStreakAsync(user, gameEndTime);

        // Assert
        result.Success.Should().BeTrue();
        result.NewStreak.Should().Be(10);
        result.LongestStreak.Should().Be(10);
    }

    #endregion

    #region UseStreakFreezeAsync Tests

    [Fact]
    public async Task UseStreakFreeze_Success_DecrementsTokens()
    {
        // Arrange
        var user = CreateUser(freezeTokens: 3);

        // Act
        var result = await _service.UseStreakFreezeAsync(user);

        // Assert
        result.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UseStreakFreeze_NoTokens_ReturnsFalse()
    {
        // Arrange
        var user = CreateUser(freezeTokens: 0);

        // Act
        var result = await _service.UseStreakFreezeAsync(user);

        // Assert
        result.Should().BeFalse();
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region IsStreakAtRisk Tests

    [Fact]
    public void IsStreakAtRisk_LastHour_ReturnsTrue()
    {
        // Arrange
        var user = CreateUser(currentStreak: 5, lastPlayedDate: new DateTime(2025, 1, 8));
        // Current time is 23:30 on Jan 8 (30 minutes until reset)
        var currentTime = new DateTimeOffset(2025, 1, 8, 23, 30, 0, TimeSpan.Zero);

        // Act
        var result = _service.IsStreakAtRisk(user, currentTime);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsStreakAtRisk_MoreThanOneHour_ReturnsFalse()
    {
        // Arrange
        var user = CreateUser(currentStreak: 5, lastPlayedDate: new DateTime(2025, 1, 8));
        // Current time is 20:00 on Jan 8 (4 hours until reset)
        var currentTime = new DateTimeOffset(2025, 1, 8, 20, 0, 0, TimeSpan.Zero);

        // Act
        var result = _service.IsStreakAtRisk(user, currentTime);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsStreakAtRisk_NoStreak_ReturnsFalse()
    {
        // Arrange
        var user = CreateUser(currentStreak: 0);
        var currentTime = new DateTimeOffset(2025, 1, 8, 23, 30, 0, TimeSpan.Zero);

        // Act
        var result = _service.IsStreakAtRisk(user, currentTime);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetStreakResetTime Tests

    [Fact]
    public void GetStreakResetTime_RespectsTimezone()
    {
        // Arrange - User in US Pacific (UTC-8)
        var user = CreateUser(timezone: "America/Los_Angeles");
        var currentTime = new DateTimeOffset(2025, 1, 9, 15, 0, 0, TimeSpan.Zero); // 7:00 AM Pacific

        // Act
        var resetTime = _service.GetStreakResetTime(user, currentTime);

        // Assert
        // Reset should be at midnight Pacific = 08:00 UTC on Jan 10
        resetTime.Should().Be(new DateTimeOffset(2025, 1, 10, 8, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void GetStreakResetTime_DefaultTimezone_UsesUtc()
    {
        // Arrange - User with no timezone set
        var user = CreateUser(timezone: null);
        var currentTime = new DateTimeOffset(2025, 1, 9, 15, 0, 0, TimeSpan.Zero);

        // Act
        var resetTime = _service.GetStreakResetTime(user, currentTime);

        // Assert
        // Reset should be at midnight UTC on Jan 10
        resetTime.Should().Be(new DateTimeOffset(2025, 1, 10, 0, 0, 0, TimeSpan.Zero));
    }

    #endregion

    #region Helper Methods

    private static User CreateUser(
        int id = 1,
        int currentStreak = 0,
        int longestStreak = 0,
        int freezeTokens = 0,
        DateTime? lastPlayedDate = null,
        string? timezone = null)
    {
        var user = User.Create("ext_123", "test@test.com", "TestUser", AuthProvider.Local);

        // Use reflection to set private properties
        SetProperty(user, "Id", id);
        SetProperty(user, "CurrentStreak", currentStreak);
        SetProperty(user, "LongestStreak", longestStreak);
        SetProperty(user, "StreakFreezeTokens", freezeTokens);
        SetProperty(user, "LastPlayedDate", lastPlayedDate);
        SetProperty(user, "Timezone", timezone);

        return user;
    }

    private static void SetProperty<T>(User user, string propertyName, T value)
    {
        var property = typeof(User).GetProperty(propertyName);
        if (property != null)
        {
            var backingField = typeof(User).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            backingField?.SetValue(user, value);
        }
    }

    #endregion
}
