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
        var displayName = "Test_User";
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
        var user = User.Create("external-id", null, "Display_Name", AuthProvider.Facebook);

        // Assert
        user.Email.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyExternalId_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => User.Create("", "test@example.com", "Test123", AuthProvider.Google);

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
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);

        // Act
        user.UpdateHighScore(100);

        // Assert
        user.HighScore.Should().Be(100);
    }

    [Fact]
    public void UpdateHighScore_WhenNewScoreIsLower_ShouldNotUpdateHighScore()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);
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
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);
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
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);

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
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);

        // Act
        user.UpdateLastLogin();

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void User_SetDisplayName_UpdatesDisplayName_WhenValid()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);
        var newDisplayName = "NewUser_123";

        // Act
        user.SetDisplayName(newDisplayName);

        // Assert
        user.DisplayName.Should().Be(newDisplayName);
    }

    [Fact]
    public void User_SetDisplayName_ThrowsException_WhenInvalidFormat()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);

        // Act
        var act = () => user.SetDisplayName("ab"); // too short

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Display name*");
    }

    [Fact]
    public void User_SetDisplayName_ThrowsException_WhenContainsInvalidChars()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);

        // Act
        var act = () => user.SetDisplayName("user@name");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*alphanumeric*");
    }

    [Fact]
    public void User_SetAvatar_UpdatesAvatar()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);
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
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);
        user.SetAvatar("🎮");

        // Act
        user.SetAvatar(null);

        // Assert
        user.Avatar.Should().BeNull();
    }

    [Fact]
    public void User_Create_AvatarIsNull()
    {
        // Arrange & Act
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);

        // Assert
        user.Avatar.Should().BeNull();
    }

    [Fact]
    public void User_SetDisplayName_UpdatesLastDisplayNameChangeAt()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);
        user.LastDisplayNameChangeAt.Should().BeNull(); // Should be null initially

        // Act
        user.SetDisplayName("NewUser_123");

        // Assert
        user.LastDisplayNameChangeAt.Should().NotBeNull();
        user.LastDisplayNameChangeAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void User_Create_LastDisplayNameChangeAtIsNull()
    {
        // Arrange & Act
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);

        // Assert
        user.LastDisplayNameChangeAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithRole_ShouldSetRole()
    {
        // Arrange
        var externalId = "google-12345";
        var email = "admin@example.com";
        var displayName = "Admin_User";
        var provider = AuthProvider.Google;
        var role = UserRole.Admin;

        // Act
        var user = User.Create(externalId, email, displayName, provider, role);

        // Assert
        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void Create_DefaultRole_ShouldBeUser()
    {
        // Arrange & Act
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);

        // Assert
        user.Role.Should().Be(UserRole.User);
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedAndTimestamp()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);
        user.IsDeleted.Should().BeFalse();
        user.DeletedAt.Should().BeNull();

        // Act
        user.SoftDelete();

        // Assert
        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().NotBeNull();
        user.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldHaveIsDeletedFalseByDefault()
    {
        // Arrange & Act
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);

        // Assert
        user.IsDeleted.Should().BeFalse();
        user.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void SetRole_ShouldUpdateRole()
    {
        // Arrange
        var user = User.Create("external-id", "test@example.com", "Test_User", AuthProvider.Google);
        user.Role.Should().Be(UserRole.User); // Default role

        // Act
        user.SetRole(UserRole.Admin);

        // Assert
        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void SetRole_ShouldAllowChangingFromAdminToUser()
    {
        // Arrange
        var user = User.Create("external-id", "admin@example.com", "Admin_User", AuthProvider.Google, UserRole.Admin);
        user.Role.Should().Be(UserRole.Admin);

        // Act
        user.SetRole(UserRole.User);

        // Assert
        user.Role.Should().Be(UserRole.User);
    }
}
