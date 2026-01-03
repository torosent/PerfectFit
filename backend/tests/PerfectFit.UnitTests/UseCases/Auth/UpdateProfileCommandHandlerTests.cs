using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;
using PerfectFit.UseCases.Auth.Commands;

namespace PerfectFit.UnitTests.UseCases.Auth;

public class UpdateProfileCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUsernameValidationService> _usernameValidationServiceMock;
    private readonly UpdateProfileCommandHandler _sut;

    public UpdateProfileCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _usernameValidationServiceMock = new Mock<IUsernameValidationService>();
        _sut = new UpdateProfileCommandHandler(
            _userRepositoryMock.Object,
            _usernameValidationServiceMock.Object);
    }

    private static User CreateTestUser(int id = 1)
    {
        var user = User.Create("external-123", "test@test.com", "Test User", AuthProvider.Google);
        // Use reflection to set Id since it's private
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, id);
        return user;
    }

    [Fact]
    public async Task Handle_UpdatesUsername_WhenUsernameAvailableAndClean()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, Username: "NewUsername", Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _usernameValidationServiceMock
            .Setup(x => x.ValidateAsync("NewUsername", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UsernameValidationResult.Success());

        _userRepositoryMock
            .Setup(x => x.IsUsernameTakenAsync("NewUsername", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.UpdatedProfile.Should().NotBeNull();
        result.UpdatedProfile!.Username.Should().Be("NewUsername");

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenUsernameTaken()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, Username: "TakenName", Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _usernameValidationServiceMock
            .Setup(x => x.ValidateAsync("TakenName", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UsernameValidationResult.Success());

        _userRepositoryMock
            .Setup(x => x.IsUsernameTakenAsync("TakenName", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("taken");
        result.UpdatedProfile.Should().BeNull();

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenUsernameProfane()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, Username: "BadWord", Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _usernameValidationServiceMock
            .Setup(x => x.ValidateAsync("BadWord", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UsernameValidationResult.Failure("Username contains inappropriate content."));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("inappropriate");
        result.UpdatedProfile.Should().BeNull();

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsSuggestion_WhenProfanityCheckFails()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, Username: "ValidName", Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _usernameValidationServiceMock
            .Setup(x => x.ValidateAsync("ValidName", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UsernameValidationResult.FailureWithSuggestion(
                "Profanity check unavailable. Please try a different username or use the suggested one.",
                "Player_ABC123"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.SuggestedUsername.Should().Be("Player_ABC123");
        result.UpdatedProfile.Should().BeNull();

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdatesAvatar_WhenProvided()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, Username: null, Avatar: "🎮");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.UpdatedProfile.Should().NotBeNull();
        result.UpdatedProfile!.Avatar.Should().Be("🎮");

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdatesBoth_WhenBothProvided()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, Username: "NewName", Avatar: "🚀");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _usernameValidationServiceMock
            .Setup(x => x.ValidateAsync("NewName", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UsernameValidationResult.Success());

        _userRepositoryMock
            .Setup(x => x.IsUsernameTakenAsync("NewName", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.UpdatedProfile.Should().NotBeNull();
        result.UpdatedProfile!.Username.Should().Be("NewName");
        result.UpdatedProfile!.Avatar.Should().Be("🚀");

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenUserNotFound()
    {
        // Arrange
        var command = new UpdateProfileCommand(UserId: 999, Username: "NewName", Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
        result.UpdatedProfile.Should().BeNull();

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DoesNothing_WhenNothingProvided()
    {
        // Arrange
        var user = CreateTestUser(1);
        var originalUsername = user.Username;
        var command = new UpdateProfileCommand(UserId: 1, Username: null, Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.UpdatedProfile.Should().NotBeNull();
        result.UpdatedProfile!.Username.Should().Be(originalUsername);

        // Should not update when nothing changed
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _usernameValidationServiceMock.Verify(x => x.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenUsernameFormatInvalid()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, Username: "ab", Avatar: null); // Too short

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _usernameValidationServiceMock
            .Setup(x => x.ValidateAsync("ab", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UsernameValidationResult.Failure("Username must be between 3 and 20 characters."));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.UpdatedProfile.Should().BeNull();

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ClearsAvatar_WhenEmptyStringProvided()
    {
        // Arrange
        var user = CreateTestUser(1);
        user.SetAvatar("🎮"); // Set an initial avatar
        var command = new UpdateProfileCommand(UserId: 1, Username: null, Avatar: "");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.UpdatedProfile.Should().NotBeNull();
        result.UpdatedProfile!.Avatar.Should().BeNull();

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}
