using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;
using PerfectFit.UseCases.Gamification.Commands;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Commands;

public class UpdateStreakCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IStreakService> _streakServiceMock;
    private readonly UpdateStreakCommandHandler _handler;

    public UpdateStreakCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _streakServiceMock = new Mock<IStreakService>();
        _handler = new UpdateStreakCommandHandler(_userRepositoryMock.Object, _streakServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUser_UpdatesStreakSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;
        var gameEndTime = DateTimeOffset.UtcNow;
        var user = CreateUser(userIntId, userId);
        var expectedResult = new StreakResult(true, 5, 10, false, false);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _streakServiceMock
            .Setup(x => x.UpdateStreakAsync(It.IsAny<User>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new UpdateStreakCommand(userId, gameEndTime);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        _streakServiceMock.Verify(x => x.UpdateStreakAsync(It.IsAny<User>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailureResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameEndTime = DateTimeOffset.UtcNow;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new UpdateStreakCommand(userId, gameEndTime);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
        _streakServiceMock.Verify(x => x.UpdateStreakAsync(It.IsAny<User>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_StreakBroken_ReturnsResultWithStreakBroken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;
        var gameEndTime = DateTimeOffset.UtcNow;
        var user = CreateUser(userIntId, userId);
        var expectedResult = new StreakResult(true, 1, 10, true, false);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _streakServiceMock
            .Setup(x => x.UpdateStreakAsync(It.IsAny<User>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new UpdateStreakCommand(userId, gameEndTime);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.StreakBroken.Should().BeTrue();
        result.NewStreak.Should().Be(1);
    }

    [Fact]
    public async Task Handle_StreakSavedWithFreeze_ReturnsResultWithUsedFreeze()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;
        var gameEndTime = DateTimeOffset.UtcNow;
        var user = CreateUser(userIntId, userId);
        var expectedResult = new StreakResult(true, 6, 10, false, true);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _streakServiceMock
            .Setup(x => x.UpdateStreakAsync(It.IsAny<User>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new UpdateStreakCommand(userId, gameEndTime);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.UsedFreeze.Should().BeTrue();
        result.StreakBroken.Should().BeFalse();
    }

    private static User CreateUser(int id, Guid externalGuid)
    {
        var user = User.Create(externalGuid.ToString(), "test@test.com", "TestUser", AuthProvider.Local);
        SetProperty(user, "Id", id);
        return user;
    }

    private static void SetProperty<T>(User user, string propertyName, T value)
    {
        var property = typeof(User).GetProperty(propertyName);
        if (property != null)
        {
            var backingField = typeof(User).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            backingField?.SetValue(user, value);
        }
    }
}
