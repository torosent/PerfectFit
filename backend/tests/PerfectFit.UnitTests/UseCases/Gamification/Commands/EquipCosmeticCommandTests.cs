using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;
using PerfectFit.UseCases.Gamification.Commands;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Commands;

public class EquipCosmeticCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICosmeticService> _cosmeticServiceMock;
    private readonly EquipCosmeticCommandHandler _handler;

    public EquipCosmeticCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _cosmeticServiceMock = new Mock<ICosmeticService>();
        _handler = new EquipCosmeticCommandHandler(_userRepositoryMock.Object, _cosmeticServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwnership_EquipsSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cosmeticId = Guid.NewGuid();
        var userIntId = 1;
        var cosmeticIntId = 1;

        var user = CreateUser(userIntId, userId);
        var expectedResult = new EquipResult(true);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _cosmeticServiceMock
            .Setup(x => x.UserOwnsCosmeticAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _cosmeticServiceMock
            .Setup(x => x.EquipCosmeticAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new EquipCosmeticCommand(userId, cosmeticId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserDoesNotOwnCosmetic_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cosmeticId = Guid.NewGuid();
        var userIntId = 1;
        var cosmeticIntId = 1;

        var user = CreateUser(userIntId, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _cosmeticServiceMock
            .Setup(x => x.UserOwnsCosmeticAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new EquipCosmeticCommand(userId, cosmeticId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("do not own");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cosmeticId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new EquipCosmeticCommand(userId, cosmeticId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_DefaultCosmetic_EquipsSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cosmeticId = Guid.NewGuid();
        var userIntId = 1;
        var cosmeticIntId = 1;

        var user = CreateUser(userIntId, userId);
        var expectedResult = new EquipResult(true);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Default cosmetics return true for ownership check
        _cosmeticServiceMock
            .Setup(x => x.UserOwnsCosmeticAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _cosmeticServiceMock
            .Setup(x => x.EquipCosmeticAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new EquipCosmeticCommand(userId, cosmeticId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EquipServiceFails_ReturnsServiceError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cosmeticId = Guid.NewGuid();
        var userIntId = 1;
        var cosmeticIntId = 1;

        var user = CreateUser(userIntId, userId);
        var failResult = new EquipResult(false, "Invalid cosmetic type");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _cosmeticServiceMock
            .Setup(x => x.UserOwnsCosmeticAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _cosmeticServiceMock
            .Setup(x => x.EquipCosmeticAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failResult);

        var command = new EquipCosmeticCommand(userId, cosmeticId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Success.Should().BeFalse();
        result.Value.ErrorMessage.Should().Contain("Invalid cosmetic type");
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
