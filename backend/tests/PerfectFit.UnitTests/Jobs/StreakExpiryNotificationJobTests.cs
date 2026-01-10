using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;
using PerfectFit.Infrastructure.Jobs;

namespace PerfectFit.UnitTests.Jobs;

public class StreakExpiryNotificationJobTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IStreakService> _streakServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<StreakExpiryNotificationJob>> _loggerMock;
    private readonly StreakExpiryNotificationJob _job;

    public StreakExpiryNotificationJobTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _streakServiceMock = new Mock<IStreakService>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<StreakExpiryNotificationJob>>();

        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUserRepository)))
            .Returns(_userRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IStreakService)))
            .Returns(_streakServiceMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IEmailService)))
            .Returns(_emailServiceMock.Object);

        _job = new StreakExpiryNotificationJob(_scopeFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_FindsUsersWithExpiringStreaks()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var userWithExpiringStreak = CreateUser(1, currentStreak: 7);
        var userWithSafeStreak = CreateUser(2, currentStreak: 5);
        var userWithNoStreak = CreateUser(3, currentStreak: 0);

        _userRepositoryMock.Setup(r => r.GetUsersWithActiveStreaksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { userWithExpiringStreak, userWithSafeStreak, userWithNoStreak });

        // User 1's streak resets in 3 hours (within notification window 2-4 hours)
        _streakServiceMock.Setup(s => s.GetStreakResetTime(userWithExpiringStreak, It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(3));
        // User 2's streak resets in 20 hours (outside notification window)
        _streakServiceMock.Setup(s => s.GetStreakResetTime(userWithSafeStreak, It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(20));
        // User 3 has no streak
        _streakServiceMock.Setup(s => s.GetStreakResetTime(userWithNoStreak, It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(24));

        // Setup atomic claim - succeeds for user 1
        _userRepositoryMock.Setup(r => r.TryClaimStreakNotificationAsync(1, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _emailServiceMock.Setup(e => e.SendStreakExpiryNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await _job.ExecuteNotificationAsync(CancellationToken.None);

        // Assert - Only user with expiring streak (within 2-4 hour window) should be notified
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            "test1@test.com",
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            "test2@test.com",
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_RespectsNotificationWindow_2To4Hours()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var userResetsIn2_5Hours = CreateUser(1, currentStreak: 5);
        var userResetsIn3Hours = CreateUser(2, currentStreak: 5);
        var userResetsIn5Hours = CreateUser(3, currentStreak: 5);
        var userResetsIn1Hour = CreateUser(4, currentStreak: 5);

        var users = new List<User> { userResetsIn2_5Hours, userResetsIn3Hours, userResetsIn5Hours, userResetsIn1Hour };
        _userRepositoryMock.Setup(r => r.GetUsersWithActiveStreaksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Setup reset times using It.Is<User> with ID matching - notification window is 2-4 hours before expiry
        _streakServiceMock.Setup(s => s.GetStreakResetTime(It.Is<User>(u => u.Id == 1), It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(2.5));  // Within window
        _streakServiceMock.Setup(s => s.GetStreakResetTime(It.Is<User>(u => u.Id == 2), It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(3));  // Within window
        _streakServiceMock.Setup(s => s.GetStreakResetTime(It.Is<User>(u => u.Id == 3), It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(5));  // Outside window (too far)
        _streakServiceMock.Setup(s => s.GetStreakResetTime(It.Is<User>(u => u.Id == 4), It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(1));  // Outside window (too close)

        // Setup atomic claims - succeed for users within window
        _userRepositoryMock.Setup(r => r.TryClaimStreakNotificationAsync(1, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.TryClaimStreakNotificationAsync(2, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _emailServiceMock.Setup(e => e.SendStreakExpiryNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await _job.ExecuteNotificationAsync(CancellationToken.None);

        // Assert - Users 1, 2 within 2-4 hour window, users 3 and 4 outside
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            "test1@test.com",
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            "test2@test.com",
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            "test3@test.com",
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            "test4@test.com",
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsUsersWithNoEmail()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var userWithEmail = CreateUser(1, currentStreak: 5);
        var userWithoutEmail = CreateUser(2, currentStreak: 5, email: null);

        _userRepositoryMock.Setup(r => r.GetUsersWithActiveStreaksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { userWithEmail, userWithoutEmail });

        _streakServiceMock.Setup(s => s.GetStreakResetTime(It.IsAny<User>(), It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(3));

        // Setup atomic claim for user with email
        _userRepositoryMock.Setup(r => r.TryClaimStreakNotificationAsync(1, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _emailServiceMock.Setup(e => e.SendStreakExpiryNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await _job.ExecuteNotificationAsync(CancellationToken.None);

        // Assert - Only user with email should receive notification
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LogsNotificationsSent()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var user = CreateUser(1, currentStreak: 10);

        _userRepositoryMock.Setup(r => r.GetUsersWithActiveStreaksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });
        _streakServiceMock.Setup(s => s.GetStreakResetTime(user, It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(3));

        // Setup atomic claim
        _userRepositoryMock.Setup(r => r.TryClaimStreakNotificationAsync(1, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _emailServiceMock.Setup(e => e.SendStreakExpiryNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await _job.ExecuteNotificationAsync(CancellationToken.None);

        // Assert - Verify notification was sent
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyUserList()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetUsersWithActiveStreaksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        await _job.ExecuteNotificationAsync(CancellationToken.None);

        // Assert - No notifications sent
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsUsersAlreadyNotifiedWithin24Hours()
    {
        // Arrange - User was notified 12 hours ago (within 24 hour deduplication window)
        var now = DateTimeOffset.UtcNow;
        var recentlyNotifiedUser = CreateUser(1, currentStreak: 5, lastNotificationSentAt: DateTime.UtcNow.AddHours(-12));
        var notRecentlyNotifiedUser = CreateUser(2, currentStreak: 5, lastNotificationSentAt: DateTime.UtcNow.AddHours(-25));
        var neverNotifiedUser = CreateUser(3, currentStreak: 5, lastNotificationSentAt: null);

        _userRepositoryMock.Setup(r => r.GetUsersWithActiveStreaksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { recentlyNotifiedUser, notRecentlyNotifiedUser, neverNotifiedUser });

        _streakServiceMock.Setup(s => s.GetStreakResetTime(It.IsAny<User>(), It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(3));

        // Setup atomic claims - user 1's claim fails (already notified), users 2 and 3 succeed
        _userRepositoryMock.Setup(r => r.TryClaimStreakNotificationAsync(1, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // Already notified
        _userRepositoryMock.Setup(r => r.TryClaimStreakNotificationAsync(2, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.TryClaimStreakNotificationAsync(3, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _emailServiceMock.Setup(e => e.SendStreakExpiryNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await _job.ExecuteNotificationAsync(CancellationToken.None);

        // Assert
        // User 1 - recently notified, should be skipped via early filter
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            "test1@test.com",
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Never);

        // User 2 - notified > 24 hours ago, should receive notification
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            "test2@test.com",
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Once);

        // User 3 - never notified, should receive notification
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            "test3@test.com",
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UsesAtomicClaimBeforeSendingEmail()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var user = CreateUser(1, currentStreak: 5);

        _userRepositoryMock.Setup(r => r.GetUsersWithActiveStreaksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        _streakServiceMock.Setup(s => s.GetStreakResetTime(user, It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(3));

        // Setup atomic claim to succeed
        _userRepositoryMock.Setup(r => r.TryClaimStreakNotificationAsync(1, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _emailServiceMock.Setup(e => e.SendStreakExpiryNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await _job.ExecuteNotificationAsync(CancellationToken.None);

        // Assert - Verify atomic claim was called and email was sent
        _userRepositoryMock.Verify(r => r.TryClaimStreakNotificationAsync(
            1, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsEmailWhenAtomicClaimFails()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var user = CreateUser(1, currentStreak: 5);

        _userRepositoryMock.Setup(r => r.GetUsersWithActiveStreaksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        _streakServiceMock.Setup(s => s.GetStreakResetTime(user, It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(3));

        // Setup atomic claim to fail (another instance already claimed)
        _userRepositoryMock.Setup(r => r.TryClaimStreakNotificationAsync(1, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _job.ExecuteNotificationAsync(CancellationToken.None);

        // Assert - Email should NOT be sent when claim fails
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotSendEmailIfAtomicClaimFails_EmailException()
    {
        // Arrange - Claim succeeds but email throws
        var now = DateTimeOffset.UtcNow;
        var user = CreateUser(1, currentStreak: 5);

        _userRepositoryMock.Setup(r => r.GetUsersWithActiveStreaksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        _streakServiceMock.Setup(s => s.GetStreakResetTime(user, It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(3));

        // Setup atomic claim to succeed
        _userRepositoryMock.Setup(r => r.TryClaimStreakNotificationAsync(1, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _emailServiceMock.Setup(e => e.SendStreakExpiryNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Email service unavailable"));

        // Act
        await _job.ExecuteNotificationAsync(CancellationToken.None);

        // Assert - Claim was made but email failed - claim is already recorded in DB by the atomic method
        _userRepositoryMock.Verify(r => r.TryClaimStreakNotificationAsync(
            1, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_AtomicClaimPreventsMultipleEmailsForSameUser()
    {
        // Arrange - Simulate scenario where two instances try to notify same user
        var now = DateTimeOffset.UtcNow;
        var user = CreateUser(1, currentStreak: 5);

        _userRepositoryMock.Setup(r => r.GetUsersWithActiveStreaksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        _streakServiceMock.Setup(s => s.GetStreakResetTime(user, It.IsAny<DateTimeOffset>()))
            .Returns(now.AddHours(3));

        // First call succeeds, second fails (simulating race condition)
        var callCount = 0;
        _userRepositoryMock.Setup(r => r.TryClaimStreakNotificationAsync(1, StreakExpiryNotificationJob.NotificationCooldownHours, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ++callCount == 1);

        _emailServiceMock.Setup(e => e.SendStreakExpiryNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act - Execute twice (simulating two instances)
        await _job.ExecuteNotificationAsync(CancellationToken.None);
        await _job.ExecuteNotificationAsync(CancellationToken.None);

        // Assert - Email should only be sent once
        _emailServiceMock.Verify(e => e.SendStreakExpiryNotificationAsync(
            "test1@test.com",
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void NotificationCooldownHours_Is24()
    {
        // Assert - Verify the deduplication window is 24 hours
        StreakExpiryNotificationJob.NotificationCooldownHours.Should().Be(24);
    }

    #region Helper Methods

    private static User CreateUser(int id, int currentStreak = 0, string? email = "default", DateTime? lastNotificationSentAt = null)
    {
        var actualEmail = email == "default" ? $"test{id}@test.com" : email;
        var user = User.Create($"ext_{id}", actualEmail, $"User{id}", AuthProvider.Local);
        SetProperty(user, "Id", id);
        SetProperty(user, "CurrentStreak", currentStreak);
        if (lastNotificationSentAt.HasValue)
        {
            SetProperty(user, "LastStreakNotificationSentAt", lastNotificationSentAt);
        }
        return user;
    }

    private static void SetProperty<T, TValue>(T obj, string propertyName, TValue value) where T : class
    {
        var backingField = typeof(T).GetField($"<{propertyName}>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        backingField?.SetValue(obj, value);
    }

    #endregion
}
