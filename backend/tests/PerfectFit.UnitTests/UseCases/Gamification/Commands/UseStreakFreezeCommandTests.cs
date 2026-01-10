using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Gamification.Commands;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Commands;

public class UseStreakFreezeCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IStreakService> _streakServiceMock;
    private readonly UseStreakFreezeCommandHandler _handler;

    public UseStreakFreezeCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _streakServiceMock = new Mock<IStreakService>();
        _handler = new UseStreakFreezeCommandHandler(_userRepositoryMock.Object, _streakServiceMock.Object);
    }

    [Fact]
    public async Task Handle_UserHasFreezeTokens_UsesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;
        var user = CreateUser(userIntId, userId, freezeTokens: 3);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _streakServiceMock
            .Setup(x => x.UseStreakFreezeAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UseStreakFreezeCommand(userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserHasNoFreezeTokens_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;
        var user = CreateUser(userIntId, userId, freezeTokens: 0);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _streakServiceMock
            .Setup(x => x.UseStreakFreezeAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new UseStreakFreezeCommand(userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new UseStreakFreezeCommand(userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    private static User CreateUser(int id, Guid externalGuid, int freezeTokens = 0)
    {
        var user = User.Create(externalGuid.ToString(), "test@test.com", "TestUser", AuthProvider.Local);
        SetProperty(user, "Id", id);
        SetProperty(user, "StreakFreezeTokens", freezeTokens);
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
