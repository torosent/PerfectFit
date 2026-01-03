using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Options;
using Moq;
using PerfectFit.Core.Configuration;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Auth.Commands;

namespace PerfectFit.UnitTests.Auth;

public class OAuthLoginCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IOptions<AdminSettings>> _adminSettingsMock;
    private readonly OAuthLoginCommandHandler _sut;

    public OAuthLoginCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _adminSettingsMock = new Mock<IOptions<AdminSettings>>();
        _adminSettingsMock.Setup(x => x.Value).Returns(new AdminSettings { Emails = new List<string>() });
        _sut = new OAuthLoginCommandHandler(_userRepositoryMock.Object, _jwtServiceMock.Object, _adminSettingsMock.Object);
    }

    private OAuthLoginCommandHandler CreateHandlerWithAdminEmails(List<string> adminEmails)
    {
        var adminSettingsMock = new Mock<IOptions<AdminSettings>>();
        adminSettingsMock.Setup(x => x.Value).Returns(new AdminSettings { Emails = adminEmails });
        return new OAuthLoginCommandHandler(_userRepositoryMock.Object, _jwtServiceMock.Object, adminSettingsMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_New_User_When_Not_Exists()
    {
        // Arrange
        var command = new OAuthLoginCommand(
            ExternalId: "google-new-user-123",
            Email: "newuser@gmail.com",
            DisplayName: "New User",
            Provider: AuthProvider.Google
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .ReturnsAsync((User user, CancellationToken _) => user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("test-jwt-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test-jwt-token");
        result.User.Should().NotBeNull();
        result.User.ExternalId.Should().Be(command.ExternalId);
        result.User.Email.Should().Be(command.Email);
        result.User.DisplayName.Should().Be(command.DisplayName);
        result.User.Provider.Should().Be(command.Provider);

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Existing_User_When_Found()
    {
        // Arrange
        var existingUser = User.Create("google-existing-123", "existing@gmail.com", "Existing User", AuthProvider.Google);

        var command = new OAuthLoginCommand(
            ExternalId: "google-existing-123",
            Email: "existing@gmail.com",
            DisplayName: "Existing User",
            Provider: AuthProvider.Google
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(existingUser))
            .Returns("existing-user-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("existing-user-token");
        result.User.ExternalId.Should().Be(existingUser.ExternalId);

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(x => x.UpdateAsync(existingUser, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Update_LastLogin_For_Existing_User()
    {
        // Arrange
        var existingUser = User.Create("microsoft-123", "user@outlook.com", "MS User", AuthProvider.Microsoft);

        var command = new OAuthLoginCommand(
            ExternalId: "microsoft-123",
            Email: "user@outlook.com",
            DisplayName: "MS User",
            Provider: AuthProvider.Microsoft
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("token");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<User>(u => u.LastLoginAt != null),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Work_With_Null_Email()
    {
        // Arrange
        var command = new OAuthLoginCommand(
            ExternalId: "facebook-123",
            Email: null,
            DisplayName: "Facebook User",
            Provider: AuthProvider.Facebook
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("facebook-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Email.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Generate_Token_With_User()
    {
        // Arrange
        var command = new OAuthLoginCommand(
            ExternalId: "google-token-test",
            Email: "token@test.com",
            DisplayName: "Token Test",
            Provider: AuthProvider.Google
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("generated-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        _jwtServiceMock.Verify(x => x.GenerateToken(It.Is<User>(u =>
            u.ExternalId == command.ExternalId &&
            u.Provider == command.Provider)),
            Times.Once);
    }

    [Theory]
    [InlineData(AuthProvider.Google)]
    [InlineData(AuthProvider.Facebook)]
    [InlineData(AuthProvider.Microsoft)]
    [InlineData(AuthProvider.Guest)]
    public async Task Handle_Should_Support_All_Providers(AuthProvider provider)
    {
        // Arrange
        var command = new OAuthLoginCommand(
            ExternalId: $"{provider}-user-id",
            Email: $"user@{provider}.com",
            DisplayName: $"{provider} User",
            Provider: provider
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns($"{provider}-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Provider.Should().Be(provider);
    }

    [Fact]
    public async Task Handle_Should_Set_Admin_Role_For_Configured_Admin_Email_On_New_User()
    {
        // Arrange
        var adminEmail = "admin@test.com";
        var handler = CreateHandlerWithAdminEmails(new List<string> { adminEmail });

        var command = new OAuthLoginCommand(
            ExternalId: "google-admin-123",
            Email: adminEmail,
            DisplayName: "Admin User",
            Provider: AuthProvider.Google
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .ReturnsAsync((User user, CancellationToken _) => user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("admin-token");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task Handle_Should_Set_Admin_Role_CaseInsensitive()
    {
        // Arrange
        var adminEmail = "Admin@Test.Com";
        var handler = CreateHandlerWithAdminEmails(new List<string> { adminEmail });

        var command = new OAuthLoginCommand(
            ExternalId: "google-admin-456",
            Email: "admin@test.com", // Different case
            DisplayName: "Admin User",
            Provider: AuthProvider.Google
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .ReturnsAsync((User user, CancellationToken _) => user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("admin-token");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task Handle_Should_Promote_Existing_User_To_Admin_If_Configured()
    {
        // Arrange
        var adminEmail = "promoted@test.com";
        var handler = CreateHandlerWithAdminEmails(new List<string> { adminEmail });

        // Existing user with regular role
        var existingUser = User.Create("google-promote-123", adminEmail, "To Be Promoted", AuthProvider.Google, UserRole.User);

        var command = new OAuthLoginCommand(
            ExternalId: "google-promote-123",
            Email: adminEmail,
            DisplayName: "To Be Promoted",
            Provider: AuthProvider.Google
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("promoted-token");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        existingUser.Role.Should().Be(UserRole.Admin);
        _userRepositoryMock.Verify(x => x.UpdateAsync(existingUser, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Not_Demote_Admin_If_Removed_From_Config()
    {
        // Arrange - Handler without any admin emails configured
        var handler = CreateHandlerWithAdminEmails(new List<string>());

        // Existing admin user whose email is no longer in config
        var existingAdmin = User.Create("google-demote-123", "was-admin@test.com", "Was Admin", AuthProvider.Google, UserRole.Admin);

        var command = new OAuthLoginCommand(
            ExternalId: "google-demote-123",
            Email: "was-admin@test.com",
            DisplayName: "Was Admin",
            Provider: AuthProvider.Google
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAdmin);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("still-admin-token");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert - Should NOT be demoted for safety
        existingAdmin.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task Handle_Should_Not_Set_Admin_For_Non_Configured_Email()
    {
        // Arrange
        var handler = CreateHandlerWithAdminEmails(new List<string> { "admin@test.com" });

        var command = new OAuthLoginCommand(
            ExternalId: "google-regular-123",
            Email: "regular@test.com", // Not an admin email
            DisplayName: "Regular User",
            Provider: AuthProvider.Google
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .ReturnsAsync((User user, CancellationToken _) => user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("regular-token");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Role.Should().Be(UserRole.User);
    }
}
