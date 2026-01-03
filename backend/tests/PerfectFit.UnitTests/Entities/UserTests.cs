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

    [Fact]
    public void User_SetUsername_UpdatesUsername_WhenValid()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);
        var newUsername = "NewUser_123";

        // Act
        user.SetUsername(newUsername);

        // Assert
        user.Username.Should().Be(newUsername);
    }

    [Fact]
    public void User_SetUsername_ThrowsException_WhenInvalidFormat()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);

        // Act
        var act = () => user.SetUsername("ab"); // too short

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Username*");
    }

    [Fact]
    public void User_SetUsername_ThrowsException_WhenContainsInvalidChars()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);

        // Act
        var act = () => user.SetUsername("user@name");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*alphanumeric*");
    }

    [Fact]
    public void User_SetAvatar_UpdatesAvatar()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);
        var avatar = "🎮";

        // Act
        user.SetAvatar(avatar);

        // Assert
        user.Avatar.Should().Be(avatar);
    }

    [Fact]
    public void User_SetAvatar_AllowsNull()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);
        user.SetAvatar("🎮");

        // Act
        user.SetAvatar(null);

        // Assert
        user.Avatar.Should().BeNull();
    }

    [Fact]
    public void User_Create_GeneratesRandomUsername()
    {
        // Arrange & Act
        var user1 = User.Create("external-id-1", "test1@example.com", "Test User 1", AuthProvider.Google);
        var user2 = User.Create("external-id-2", "test2@example.com", "Test User 2", AuthProvider.Google);

        // Assert
        user1.Username.Should().StartWith("Player_");
        user2.Username.Should().StartWith("Player_");
        user1.Username.Should().HaveLength(13); // "Player_" (7) + 6 random chars
        user2.Username.Should().HaveLength(13);
        user1.Username.Should().NotBe(user2.Username); // Should be different
    }

    [Fact]
    public void User_Create_UsernameIsAlphanumeric()
    {
        // Arrange & Act
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);

        // Assert
        user.Username.Should().MatchRegex(@"^[A-Za-z0-9_]+$");
    }

    [Fact]
    public void User_Create_AvatarIsNull()
    {
        // Arrange & Act
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);

        // Assert
        user.Avatar.Should().BeNull();
    }

    [Fact]
    public void User_SetUsername_UpdatesLastUsernameChangeAt()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);
        user.LastUsernameChangeAt.Should().BeNull(); // Should be null initially

        // Act
        user.SetUsername("NewUser_123");

        // Assert
        user.LastUsernameChangeAt.Should().NotBeNull();
        user.LastUsernameChangeAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void User_Create_LastUsernameChangeAtIsNull()
    {
        // Arrange & Act
        var user = User.Create("external-id", "test@example.com", "Test User", AuthProvider.Google);

        // Assert
        user.LastUsernameChangeAt.Should().BeNull();
    }
}
