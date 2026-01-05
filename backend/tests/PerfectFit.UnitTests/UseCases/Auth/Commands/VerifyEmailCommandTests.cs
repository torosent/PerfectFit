using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Identity;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Auth.Commands;

namespace PerfectFit.UnitTests.UseCases.Auth.Commands;

public class VerifyEmailCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmailVerificationService> _emailVerificationServiceMock;
    private readonly VerifyEmailCommandHandler _sut;

    public VerifyEmailCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailVerificationServiceMock = new Mock<IEmailVerificationService>();

        _sut = new VerifyEmailCommandHandler(
            _userRepositoryMock.Object,
            _emailVerificationServiceMock.Object);
    }

    [Fact]
    public async Task VerifyEmailCommand_ValidToken_MarksVerified()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetEmailVerificationToken("valid_token", DateTime.UtcNow.AddHours(24));

        var command = new VerifyEmailCommand(
            Email: "test@example.com",
            Token: "valid_token");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _emailVerificationServiceMock
            .Setup(x => x.IsTokenValid(user, command.Token))
            .Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("verified");
        _emailVerificationServiceMock.Verify(x => x.MarkEmailVerified(user), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailCommand_InvalidToken_ReturnsFailure()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetEmailVerificationToken("valid_token", DateTime.UtcNow.AddHours(24));

        var command = new VerifyEmailCommand(
            Email: "test@example.com",
            Token: "invalid_token");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _emailVerificationServiceMock
            .Setup(x => x.IsTokenValid(user, command.Token))
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid");
        _emailVerificationServiceMock.Verify(x => x.MarkEmailVerified(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task VerifyEmailCommand_ExpiredToken_ReturnsFailure()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetEmailVerificationToken("expired_token", DateTime.UtcNow.AddHours(-1)); // Expired

        var command = new VerifyEmailCommand(
            Email: "test@example.com",
            Token: "expired_token");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _emailVerificationServiceMock
            .Setup(x => x.IsTokenValid(user, command.Token))
            .Returns(false); // EmailVerificationService returns false for expired tokens

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
        (result.ErrorMessage!.Contains("Invalid") || result.ErrorMessage.Contains("expired")).Should().BeTrue();
        _emailVerificationServiceMock.Verify(x => x.MarkEmailVerified(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task VerifyEmailCommand_NonExistentUser_ReturnsFailure()
    {
        // Arrange
        var command = new VerifyEmailCommand(
            Email: "nonexistent@example.com",
            Token: "some_token");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid");
        _emailVerificationServiceMock.Verify(x => x.MarkEmailVerified(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task VerifyEmailCommand_AlreadyVerified_ReturnsSuccess()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.MarkEmailAsVerified(); // Already verified

        var command = new VerifyEmailCommand(
            Email: "test@example.com",
            Token: "any_token");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("already verified");
    }

    [Fact]
    public async Task VerifyEmailCommand_EmptyEmail_ReturnsFailure()
    {
        // Arrange
        var command = new VerifyEmailCommand(
            Email: "",
            Token: "some_token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("email");
    }

    [Fact]
    public async Task VerifyEmailCommand_EmptyToken_ReturnsFailure()
    {
        // Arrange
        var command = new VerifyEmailCommand(
            Email: "test@example.com",
            Token: "");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("token");
    }
}
