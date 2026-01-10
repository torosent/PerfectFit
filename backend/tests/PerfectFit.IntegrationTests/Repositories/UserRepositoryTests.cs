using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Infrastructure.Data.Repositories;

namespace PerfectFit.IntegrationTests.Repositories;

public class UserRepositoryTests : RepositoryTestBase
{
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _repository = new UserRepository(DbContext);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistUser()
    {
        // Arrange
        var user = User.Create("google-123", "test@example.com", "Test User", AuthProvider.Google);

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        result.Id.Should().BeGreaterThan(0);

        var savedUser = await DbContext.Users.FindAsync(result.Id);
        savedUser.Should().NotBeNull();
        savedUser!.ExternalId.Should().Be("google-123");
        savedUser.Email.Should().Be("test@example.com");
        savedUser.DisplayName.Should().Be("Test User");
        savedUser.Provider.Should().Be(AuthProvider.Google);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = User.Create("facebook-456", "user@example.com", "Facebook User", AuthProvider.Facebook);
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ExternalId.Should().Be("facebook-456");
        result.DisplayName.Should().Be("Facebook User");
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByExternalIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = User.Create("ms-789", "ms@example.com", "MS User", AuthProvider.Microsoft);
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.GetByExternalIdAsync("ms-789", AuthProvider.Microsoft);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("MS User");
    }

    [Fact]
    public async Task GetByExternalIdAsync_WhenProviderMismatch_ShouldReturnNull()
    {
        // Arrange
        var user = User.Create("external-id", "user@example.com", "User", AuthProvider.Google);
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.GetByExternalIdAsync("external-id", AuthProvider.Facebook);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByExternalIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByExternalIdAsync("non-existent", AuthProvider.Google);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        var user = User.Create("update-test", "test@example.com", "Original Name", AuthProvider.Google);
        await _repository.AddAsync(user);

        user.UpdateHighScore(500);
        user.IncrementGamesPlayed();

        // Act
        await _repository.UpdateAsync(user);

        // Assert
        var updatedUser = await DbContext.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.HighScore.Should().Be(500);
        updatedUser.GamesPlayed.Should().Be(1);
    }

    [Fact]
    public async Task AddAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var user = User.Create("cancel-test", null, "Cancel Test", AuthProvider.Guest);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        // TaskCanceledException derives from OperationCanceledException
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _repository.AddAsync(user, cts.Token));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnNonDeletedUsers()
    {
        // Arrange
        var user1 = User.Create("user-1", "user1@example.com", "User 1", AuthProvider.Google);
        var user2 = User.Create("user-2", "user2@example.com", "User 2", AuthProvider.Google);
        var deletedUser = User.Create("deleted-user", "deleted@example.com", "Deleted User", AuthProvider.Google);
        deletedUser.SoftDelete();

        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);
        await _repository.AddAsync(deletedUser);

        // Act
        var result = await _repository.GetAllAsync(1, 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.ExternalId == "user-1");
        result.Should().Contain(u => u.ExternalId == "user-2");
        result.Should().NotContain(u => u.ExternalId == "deleted-user");
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 10; i++)
        {
            var user = User.Create($"user-{i}", $"user{i}@example.com", $"User {i}", AuthProvider.Google);
            await _repository.AddAsync(user);
        }

        // Act
        var page1 = await _repository.GetAllAsync(1, 3);
        var page2 = await _repository.GetAllAsync(2, 3);
        var page4 = await _repository.GetAllAsync(4, 3);

        // Assert
        page1.Should().HaveCount(3);
        page2.Should().HaveCount(3);
        page4.Should().HaveCount(1); // Only 1 remaining (10 total, 3*3=9, 1 left)
    }

    [Fact]
    public async Task GetCountAsync_ShouldReturnTotalCount()
    {
        // Arrange
        var user1 = User.Create("count-1", "count1@example.com", "Count User 1", AuthProvider.Google);
        var user2 = User.Create("count-2", "count2@example.com", "Count User 2", AuthProvider.Google);
        var deletedUser = User.Create("count-deleted", "deleted@example.com", "Deleted Count", AuthProvider.Google);
        deletedUser.SoftDelete();

        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);
        await _repository.AddAsync(deletedUser);

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        count.Should().Be(2); // Should not count deleted user
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldMarkUserAsDeleted()
    {
        // Arrange
        var user = User.Create("soft-delete-test", "softdelete@example.com", "Soft Delete User", AuthProvider.Google);
        await _repository.AddAsync(user);
        var userId = user.Id;

        // Act
        await _repository.SoftDeleteAsync(userId);

        // Assert - Use IgnoreQueryFilters to find the deleted user
        var deletedUser = await DbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        deletedUser.Should().NotBeNull();
        deletedUser!.IsDeleted.Should().BeTrue();
        deletedUser.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task BulkSoftDeleteByProviderAsync_ShouldDeleteMatchingUsers()
    {
        // Arrange
        var guestUser1 = User.Create("guest-1", null, "Guest 1", AuthProvider.Guest);
        var guestUser2 = User.Create("guest-2", null, "Guest 2", AuthProvider.Guest);
        var googleUser = User.Create("google-bulk", "google@example.com", "Google User", AuthProvider.Google);

        await _repository.AddAsync(guestUser1);
        await _repository.AddAsync(guestUser2);
        await _repository.AddAsync(googleUser);

        // Act
        var deletedCount = await _repository.BulkSoftDeleteByProviderAsync(AuthProvider.Guest);

        // Assert
        deletedCount.Should().Be(2);

        // Verify guest users are deleted
        var remainingGuestUsers = await DbContext.Users
            .Where(u => u.Provider == AuthProvider.Guest)
            .ToListAsync();
        remainingGuestUsers.Should().BeEmpty();

        // Verify Google user is not affected
        var googleUserAfter = await _repository.GetByExternalIdAsync("google-bulk", AuthProvider.Google);
        googleUserAfter.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDeletedUsersAsync_ShouldReturnOnlyDeletedUsers()
    {
        // Arrange
        var activeUser = User.Create("active-user", "active@example.com", "Active User", AuthProvider.Google);
        var deletedUser1 = User.Create("deleted-1", "deleted1@example.com", "Deleted User 1", AuthProvider.Google);
        var deletedUser2 = User.Create("deleted-2", "deleted2@example.com", "Deleted User 2", AuthProvider.Google);
        deletedUser1.SoftDelete();
        deletedUser2.SoftDelete();

        await _repository.AddAsync(activeUser);
        await _repository.AddAsync(deletedUser1);
        await _repository.AddAsync(deletedUser2);

        // Act
        var result = await _repository.GetDeletedUsersAsync(1, 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(u => u.IsDeleted);
        result.Should().Contain(u => u.ExternalId == "deleted-1");
        result.Should().Contain(u => u.ExternalId == "deleted-2");
    }

    [Fact]
    public async Task TryClaimStreakNotificationAsync_WhenNeverNotified_ShouldSucceed()
    {
        // Arrange
        var user = User.Create("claim-test-1", "claim@example.com", "Claim User", AuthProvider.Google);
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.TryClaimStreakNotificationAsync(user.Id, 24);

        // Assert
        result.Should().BeTrue();
        var updatedUser = await DbContext.Users.FindAsync(user.Id);
        updatedUser!.LastStreakNotificationSentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task TryClaimStreakNotificationAsync_WhenNotifiedOutsideCooldown_ShouldSucceed()
    {
        // Arrange
        var user = User.Create("claim-test-2", "claim2@example.com", "Claim User 2", AuthProvider.Google);
        await _repository.AddAsync(user);

        // Set last notification to 25 hours ago (outside 24-hour cooldown)
        SetProperty(user, "LastStreakNotificationSentAt", DateTime.UtcNow.AddHours(-25));
        await _repository.UpdateAsync(user);

        // Act
        var result = await _repository.TryClaimStreakNotificationAsync(user.Id, 24);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryClaimStreakNotificationAsync_WhenNotifiedWithinCooldown_ShouldFail()
    {
        // Arrange
        var user = User.Create("claim-test-3", "claim3@example.com", "Claim User 3", AuthProvider.Google);
        await _repository.AddAsync(user);

        // Set last notification to 12 hours ago (within 24-hour cooldown)
        SetProperty(user, "LastStreakNotificationSentAt", DateTime.UtcNow.AddHours(-12));
        await _repository.UpdateAsync(user);

        // Act
        var result = await _repository.TryClaimStreakNotificationAsync(user.Id, 24);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryClaimStreakNotificationAsync_WhenUserDoesNotExist_ShouldFail()
    {
        // Act
        var result = await _repository.TryClaimStreakNotificationAsync(999, 24);

        // Assert
        result.Should().BeFalse();
    }

    private static void SetProperty<T, TValue>(T obj, string propertyName, TValue value) where T : class
    {
        var backingField = typeof(T).GetField($"<{propertyName}>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        backingField?.SetValue(obj, value);
    }
}
