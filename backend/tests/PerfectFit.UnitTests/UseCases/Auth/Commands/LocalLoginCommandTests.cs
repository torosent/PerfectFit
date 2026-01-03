using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Identity;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Auth.Commands;

namespace PerfectFit.UnitTests.UseCases.Auth.Commands;

public class LocalLoginCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly LocalLoginCommandHandler _sut;

    public LocalLoginCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();

        _sut = new LocalLoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object);
    }

    [Fact]
    public async Task LocalLoginCommand_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetPasswordHash("hashed_password");
        user.MarkEmailAsVerified();

        var command = new LocalLoginCommand(
            Email: "test@example.com",
            Password: "Password123");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, "hashed_password"))
            .Returns(true);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(user))
            .Returns("valid_jwt_token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().Be("valid_jwt_token");
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task LocalLoginCommand_WrongPassword_ReturnsFailure()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetPasswordHash("hashed_password");
        user.MarkEmailAsVerified();

        var command = new LocalLoginCommand(
            Email: "test@example.com",
            Password: "WrongPassword123");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, "hashed_password"))
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid email or password");
        result.Token.Should().BeNull();
    }

    [Fact]
    public async Task LocalLoginCommand_NonExistentUser_ReturnsFailure()
    {
        // Arrange
        var command = new LocalLoginCommand(
            Email: "nonexistent@example.com",
            Password: "Password123");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid email or password");
        result.Token.Should().BeNull();
    }

    [Fact]
    public async Task LocalLoginCommand_UnverifiedEmail_ReturnsFailure()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetPasswordHash("hashed_password");
        // Email NOT verified

        var command = new LocalLoginCommand(
            Email: "test@example.com",
            Password: "Password123");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, "hashed_password"))
            .Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("verify your email");
        result.Token.Should().BeNull();
    }

    [Fact]
    public async Task LocalLoginCommand_OAuthUser_ReturnsFailure()
    {
        // Arrange - User registered with Google OAuth
        var user = User.Create("google-123", "test@example.com", "Test User", AuthProvider.Google);

        var command = new LocalLoginCommand(
            Email: "test@example.com",
            Password: "Password123");

        // GetByExternalIdAsync with AuthProvider.Local returns null for OAuth users
        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid email or password");
        result.Token.Should().BeNull();
    }

    [Fact]
    public async Task LocalLoginCommand_UpdatesLastLogin()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetPasswordHash("hashed_password");
        user.MarkEmailAsVerified();

        var command = new LocalLoginCommand(
            Email: "test@example.com",
            Password: "Password123");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, "hashed_password"))
            .Returns(true);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(user))
            .Returns("valid_jwt_token");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<User>(u => u.LastLoginAt != null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LocalLoginCommand_EmptyEmail_ReturnsFailure()
    {
        // Arrange
        var command = new LocalLoginCommand(
            Email: "",
            Password: "Password123");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("email");
    }

    [Fact]
    public async Task LocalLoginCommand_EmptyPassword_ReturnsFailure()
    {
        // Arrange
        var command = new LocalLoginCommand(
            Email: "test@example.com",
            Password: "");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("password");
    }
}
