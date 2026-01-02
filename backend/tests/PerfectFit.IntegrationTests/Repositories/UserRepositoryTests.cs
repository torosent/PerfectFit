using FluentAssertions;
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
        var user = User.Create("apple-456", "user@example.com", "Apple User", AuthProvider.Apple);
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ExternalId.Should().Be("apple-456");
        result.DisplayName.Should().Be("Apple User");
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
        var result = await _repository.GetByExternalIdAsync("external-id", AuthProvider.Apple);

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
}
