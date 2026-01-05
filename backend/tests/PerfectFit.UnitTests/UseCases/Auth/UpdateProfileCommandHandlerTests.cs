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
    private readonly Mock<IDisplayNameValidationService> _displayNameValidationServiceMock;
    private readonly UpdateProfileCommandHandler _sut;

    public UpdateProfileCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _displayNameValidationServiceMock = new Mock<IDisplayNameValidationService>();
        _sut = new UpdateProfileCommandHandler(
            _userRepositoryMock.Object,
            _displayNameValidationServiceMock.Object);
    }

    private static User CreateTestUser(int id = 1)
    {
        var user = User.Create("external-123", "test@test.com", "Test_User", AuthProvider.Google);
        // Use reflection to set Id since it's private
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, id);
        return user;
    }

    [Fact]
    public async Task Handle_UpdatesDisplayName_WhenDisplayNameAvailableAndClean()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: "NewDisplayName", Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _displayNameValidationServiceMock
            .Setup(x => x.ValidateAsync("NewDisplayName", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DisplayNameValidationResult.Success());

        _userRepositoryMock
            .Setup(x => x.IsDisplayNameTakenAsync("NewDisplayName", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.UpdatedProfile.Should().NotBeNull();
        result.UpdatedProfile!.DisplayName.Should().Be("NewDisplayName");

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenDisplayNameTaken()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: "TakenName1", Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _displayNameValidationServiceMock
            .Setup(x => x.ValidateAsync("TakenName1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DisplayNameValidationResult.Success());

        _userRepositoryMock
            .Setup(x => x.IsDisplayNameTakenAsync("TakenName1", 1, It.IsAny<CancellationToken>()))
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
    public async Task Handle_ReturnsError_WhenDisplayNameProfane()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: "BadWord123", Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _displayNameValidationServiceMock
            .Setup(x => x.ValidateAsync("BadWord123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DisplayNameValidationResult.Failure("Display name contains inappropriate content."));

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
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: "ValidName1", Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _displayNameValidationServiceMock
            .Setup(x => x.ValidateAsync("ValidName1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DisplayNameValidationResult.FailureWithSuggestion(
                "Profanity check unavailable. Please try a different display name or use the suggested one.",
                "Player_ABC123"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.SuggestedDisplayName.Should().Be("Player_ABC123");
        result.UpdatedProfile.Should().BeNull();

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdatesAvatar_WhenProvided()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: null, Avatar: "🎮");

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
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: "NewName123", Avatar: "🚀");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _displayNameValidationServiceMock
            .Setup(x => x.ValidateAsync("NewName123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DisplayNameValidationResult.Success());

        _userRepositoryMock
            .Setup(x => x.IsDisplayNameTakenAsync("NewName123", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.UpdatedProfile.Should().NotBeNull();
        result.UpdatedProfile!.DisplayName.Should().Be("NewName123");
        result.UpdatedProfile!.Avatar.Should().Be("🚀");

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenUserNotFound()
    {
        // Arrange
        var command = new UpdateProfileCommand(UserId: 999, DisplayName: "NewName123", Avatar: null);

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
        var originalDisplayName = user.DisplayName;
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: null, Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.UpdatedProfile.Should().NotBeNull();
        result.UpdatedProfile!.DisplayName.Should().Be(originalDisplayName);

        // Should not update when nothing changed
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _displayNameValidationServiceMock.Verify(x => x.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenDisplayNameFormatInvalid()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: "ab", Avatar: null); // Too short

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _displayNameValidationServiceMock
            .Setup(x => x.ValidateAsync("ab", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DisplayNameValidationResult.Failure("Display name must be between 3 and 20 characters."));

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
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: null, Avatar: "");

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

    [Fact]
    public async Task Handle_ReturnsError_WhenAvatarInvalid()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: null, Avatar: "💩"); // Not in valid list

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid avatar");
        result.UpdatedProfile.Should().BeNull();

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdatesAvatar_WhenAvatarValid()
    {
        // Arrange
        var user = CreateTestUser(1);
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: null, Avatar: "🦊"); // Valid emoji from list

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.UpdatedProfile.Should().NotBeNull();
        result.UpdatedProfile!.Avatar.Should().Be("🦊");

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenDisplayNameCooldownActive()
    {
        // Arrange
        var user = CreateTestUser(1);
        // Set LastDisplayNameChangeAt to 3 days ago (cooldown is 7 days)
        typeof(User).GetProperty(nameof(User.LastDisplayNameChangeAt))!.SetValue(user, DateTime.UtcNow.AddDays(-3));
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: "NewDisplayName", Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("change your display name again");
        result.CooldownRemainingTime.Should().NotBeNull();
        result.CooldownRemainingTime!.Value.Days.Should().BeGreaterThanOrEqualTo(3); // ~4 days remaining
        result.UpdatedProfile.Should().BeNull();

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _displayNameValidationServiceMock.Verify(x => x.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AllowsDisplayNameChange_WhenCooldownExpired()
    {
        // Arrange
        var user = CreateTestUser(1);
        // Set LastDisplayNameChangeAt to 8 days ago (cooldown is 7 days, so expired)
        typeof(User).GetProperty(nameof(User.LastDisplayNameChangeAt))!.SetValue(user, DateTime.UtcNow.AddDays(-8));
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: "NewDisplayName", Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _displayNameValidationServiceMock
            .Setup(x => x.ValidateAsync("NewDisplayName", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DisplayNameValidationResult.Success());

        _userRepositoryMock
            .Setup(x => x.IsDisplayNameTakenAsync("NewDisplayName", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.CooldownRemainingTime.Should().BeNull();
        result.UpdatedProfile.Should().NotBeNull();
        result.UpdatedProfile!.DisplayName.Should().Be("NewDisplayName");

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AllowsFirstDisplayNameChange_WithNoCooldown()
    {
        // Arrange
        var user = CreateTestUser(1);
        // LastDisplayNameChangeAt is null by default (first change ever)
        user.LastDisplayNameChangeAt.Should().BeNull(); // Verify precondition
        var command = new UpdateProfileCommand(UserId: 1, DisplayName: "FirstDisplayName", Avatar: null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _displayNameValidationServiceMock
            .Setup(x => x.ValidateAsync("FirstDisplayName", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DisplayNameValidationResult.Success());

        _userRepositoryMock
            .Setup(x => x.IsDisplayNameTakenAsync("FirstDisplayName", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.CooldownRemainingTime.Should().BeNull();
        result.UpdatedProfile.Should().NotBeNull();
        result.UpdatedProfile!.DisplayName.Should().Be("FirstDisplayName");

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}
