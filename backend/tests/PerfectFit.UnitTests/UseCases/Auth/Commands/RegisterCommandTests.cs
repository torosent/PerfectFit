using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using PerfectFit.Core.Configuration;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Identity;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Auth.Commands;

namespace PerfectFit.UnitTests.UseCases.Auth.Commands;

public class RegisterCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEmailVerificationService> _emailVerificationServiceMock;
    private readonly Mock<IOptions<AdminSettings>> _adminSettingsMock;
    private readonly RegisterCommandHandler _sut;

    public RegisterCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _emailVerificationServiceMock = new Mock<IEmailVerificationService>();
        _adminSettingsMock = new Mock<IOptions<AdminSettings>>();
        _adminSettingsMock.Setup(x => x.Value).Returns(new AdminSettings { Emails = new List<string>() });

        _sut = new RegisterCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _emailVerificationServiceMock.Object,
            _adminSettingsMock.Object);
    }

    [Fact]
    public async Task RegisterCommand_ValidInput_CreatesUser()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123",
            DisplayName: "Test User");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.HashPassword(command.Password))
            .Returns("hashed_password");

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .ReturnsAsync((User user, CancellationToken _) => user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("check your email");
        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be(command.Email);
        capturedUser.DisplayName.Should().Be(command.DisplayName);
        capturedUser.Provider.Should().Be(AuthProvider.Local);
        capturedUser.PasswordHash.Should().Be("hashed_password");
        capturedUser.EmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterCommand_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var existingUser = User.Create("test@example.com", "test@example.com", "Existing User", AuthProvider.Local);
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123",
            DisplayName: "Test User");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("email").And.Contain("already registered");
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterCommand_WeakPassword_MissingUppercase_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "password123", // No uppercase
            DisplayName: "Test User");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("uppercase");
    }

    [Fact]
    public async Task RegisterCommand_WeakPassword_MissingNumber_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "PasswordABC", // No number
            DisplayName: "Test User");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("number");
    }

    [Fact]
    public async Task RegisterCommand_WeakPassword_TooShort_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Pass1", // Less than 8 characters
            DisplayName: "Test User");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("8 characters");
    }

    [Fact]
    public async Task RegisterCommand_WeakPassword_MissingLowercase_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "PASSWORD123", // No lowercase
            DisplayName: "Test User");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("lowercase");
    }

    [Fact]
    public async Task RegisterCommand_InvalidEmail_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "invalid-email",
            Password: "Password123",
            DisplayName: "Test User");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("email").And.Contain("invalid");
    }

    [Fact]
    public async Task RegisterCommand_SetsEmailVerificationToken()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123",
            DisplayName: "Test User");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.HashPassword(command.Password))
            .Returns("hashed_password");

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .ReturnsAsync((User user, CancellationToken _) => user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _emailVerificationServiceMock.Verify(
            x => x.SetVerificationToken(It.IsAny<User>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterCommand_EmptyEmail_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "",
            Password: "Password123",
            DisplayName: "Test User");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("email");
    }

    [Fact]
    public async Task RegisterCommand_EmptyDisplayName_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123",
            DisplayName: "");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("display name");
    }
}
