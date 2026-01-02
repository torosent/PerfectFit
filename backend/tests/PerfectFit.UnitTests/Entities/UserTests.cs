using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.UnitTests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var externalId = "google-12345";
        var email = "test@example.com";
        var displayName = "Test User";
        var provider = AuthProvider.Google;

        // Act
        var user = User.Create(externalId, email, displayName, provider);

        // Assert
        user.ExternalId.Should().Be(externalId);
        user.Email.Should().Be(email);
        user.DisplayName.Should().Be(displayName);
        user.Provider.Should().Be(provider);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.HighScore.Should().Be(0);
        user.GamesPlayed.Should().Be(0);
        user.LastLoginAt.Should().BeNull();
        user.GameSessions.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithNullEmail_ShouldAllowNullEmail()
    {
        // Arrange & Act
        var user = User.Create("external-id", null, "Display Name", AuthProvider.Apple);

        // Assert
        user.Email.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyExternalId_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => User.Create("", "test@example.com", "Test", AuthProvider.Google);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("externalId");
    }

    [Fact]
    public void Create_WithEmptyDisplayName_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => User.Create("external-id", "test@example.com", "", AuthProvider.Google);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("displayName");
    }

    [Fact]
    public void UpdateHighScore_WhenNewScoreIsHigher_ShouldUpdateHighScore()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);

        // Act
        user.UpdateHighScore(100);

        // Assert
        user.HighScore.Should().Be(100);
    }

    [Fact]
    public void UpdateHighScore_WhenNewScoreIsLower_ShouldNotUpdateHighScore()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);
        user.UpdateHighScore(100);

        // Act
        user.UpdateHighScore(50);

        // Assert
        user.HighScore.Should().Be(100);
    }

    [Fact]
    public void UpdateHighScore_WhenNewScoreIsEqual_ShouldNotChange()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);
        user.UpdateHighScore(100);

        // Act
        user.UpdateHighScore(100);

        // Assert
        user.HighScore.Should().Be(100);
    }

    [Fact]
    public void IncrementGamesPlayed_ShouldIncrementByOne()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);

        // Act
        user.IncrementGamesPlayed();
        user.IncrementGamesPlayed();

        // Assert
        user.GamesPlayed.Should().Be(2);
    }

    [Fact]
    public void UpdateLastLogin_ShouldSetLastLoginAt()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);

        // Act
        user.UpdateLastLogin();

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
