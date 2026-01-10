using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;
using PerfectFit.UseCases.Gamification.Commands;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Commands;

public class ClaimSeasonRewardCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ISeasonPassService> _seasonPassServiceMock;
    private readonly ClaimSeasonRewardCommandHandler _handler;

    public ClaimSeasonRewardCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _seasonPassServiceMock = new Mock<ISeasonPassService>();
        _handler = new ClaimSeasonRewardCommandHandler(_userRepositoryMock.Object, _seasonPassServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidClaim_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var seasonRewardId = Guid.NewGuid();
        var userIntId = 1;
        var rewardIntId = 1;

        var user = CreateUser(userIntId, userId);
        var expectedResult = new ClaimRewardResult(true, RewardType.StreakFreeze, 1);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _seasonPassServiceMock
            .Setup(x => x.ClaimRewardAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new ClaimSeasonRewardCommand(userId, seasonRewardId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Success.Should().BeTrue();
        result.Value!.RewardType.Should().Be(RewardType.StreakFreeze);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var seasonRewardId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new ClaimSeasonRewardCommand(userId, seasonRewardId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_AlreadyClaimed_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var seasonRewardId = Guid.NewGuid();
        var userIntId = 1;
        var rewardIntId = 1;

        var user = CreateUser(userIntId, userId);
        var failResult = new ClaimRewardResult(false, null, null, "Reward already claimed");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _seasonPassServiceMock
            .Setup(x => x.ClaimRewardAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failResult);

        var command = new ClaimSeasonRewardCommand(userId, seasonRewardId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // The call succeeded, but the claim failed
        result.Value!.Success.Should().BeFalse();
        result.Value.ErrorMessage.Should().Contain("already claimed");
    }

    [Fact]
    public async Task Handle_InsufficientTier_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var seasonRewardId = Guid.NewGuid();
        var userIntId = 1;
        var rewardIntId = 1;

        var user = CreateUser(userIntId, userId);
        var failResult = new ClaimRewardResult(false, null, null, "Insufficient tier to claim reward");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _seasonPassServiceMock
            .Setup(x => x.ClaimRewardAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failResult);

        var command = new ClaimSeasonRewardCommand(userId, seasonRewardId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Success.Should().BeFalse();
        result.Value.ErrorMessage.Should().Contain("tier");
    }

    [Fact]
    public async Task Handle_CosmeticReward_GrantsCosmetic()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var seasonRewardId = Guid.NewGuid();
        var userIntId = 1;
        var rewardIntId = 1;

        var user = CreateUser(userIntId, userId);
        var expectedResult = new ClaimRewardResult(true, RewardType.Cosmetic, 5); // Cosmetic ID 5

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _seasonPassServiceMock
            .Setup(x => x.ClaimRewardAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new ClaimSeasonRewardCommand(userId, seasonRewardId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RewardType.Should().Be(RewardType.Cosmetic);
        result.Value.RewardValue.Should().Be(5);
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
