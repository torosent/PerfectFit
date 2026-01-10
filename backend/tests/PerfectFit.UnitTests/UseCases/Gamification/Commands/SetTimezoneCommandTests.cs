using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Gamification.Commands;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Commands;

public class SetTimezoneCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly SetTimezoneCommandHandler _handler;

    public SetTimezoneCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new SetTimezoneCommandHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidTimezone_SetsSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;
        var timezone = "America/New_York";

        var user = CreateUser(userIntId, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new SetTimezoneCommand(userId, timezone);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<User>(u => u.Timezone == timezone),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var timezone = "America/New_York";

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new SetTimezoneCommand(userId, timezone);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Theory]
    [InlineData("America/New_York")]
    [InlineData("Europe/London")]
    [InlineData("Asia/Tokyo")]
    [InlineData("Australia/Sydney")]
    [InlineData("Pacific/Auckland")]
    public async Task Handle_VariousValidTimezones_SetsSuccessfully(string timezone)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new SetTimezoneCommand(userId, timezone);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidTimezone_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;
        var invalidTimezone = "Invalid/Timezone";

        var user = CreateUser(userIntId, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new SetTimezoneCommand(userId, invalidTimezone);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid timezone");
    }

    [Fact]
    public async Task Handle_EmptyTimezone_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;
        var emptyTimezone = "";

        var user = CreateUser(userIntId, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new SetTimezoneCommand(userId, emptyTimezone);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("required");
    }

    private static User CreateUser(int id, Guid externalGuid)
    {
        var user = User.Create(externalGuid.ToString(), "test@test.com", "TestUser", AuthProvider.Local);
        SetProperty(user, "Id", id);
        return user;
    }

    private static void SetProperty<T>(User user, string propertyName, T value)
    {
        var backingField = typeof(User).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(user, value);
    }
}
