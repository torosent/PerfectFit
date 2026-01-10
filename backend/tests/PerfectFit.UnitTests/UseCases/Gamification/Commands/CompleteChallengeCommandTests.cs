using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;
using PerfectFit.UseCases.Gamification.Commands;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Commands;

public class CompleteChallengeCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IGameSessionRepository> _gameSessionRepositoryMock;
    private readonly Mock<IChallengeService> _challengeServiceMock;
    private readonly Mock<ISeasonPassService> _seasonPassServiceMock;
    private readonly CompleteChallengeCommandHandler _handler;

    public CompleteChallengeCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _gameSessionRepositoryMock = new Mock<IGameSessionRepository>();
        _challengeServiceMock = new Mock<IChallengeService>();
        _seasonPassServiceMock = new Mock<ISeasonPassService>();
        _handler = new CompleteChallengeCommandHandler(
            _userRepositoryMock.Object,
            _gameSessionRepositoryMock.Object,
            _challengeServiceMock.Object,
            _seasonPassServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCompletion_UpdatesProgressSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var challengeId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();
        var challengeIntId = 1;

        User? capturedUser = null;
        int capturedUserId = 0;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns<int, CancellationToken>((id, ct) =>
            {
                capturedUserId = id;
                capturedUser = CreateUser(id, userId);
                return Task.FromResult<User?>(capturedUser);
            });

        _gameSessionRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns<Guid, CancellationToken>((id, ct) =>
            {
                // Create game session with the captured user ID to ensure matching
                var session = CreateGameSession(id, capturedUserId);
                return Task.FromResult<GameSession?>(session);
            });

        var challenge = CreateChallenge(challengeIntId, targetValue: 100, xpReward: 50);
        var userChallenge = CreateUserChallenge(1, challengeIntId, challenge);

        _challengeServiceMock
            .Setup(x => x.GetOrCreateUserChallengeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userChallenge);

        _challengeServiceMock
            .Setup(x => x.ValidateChallengeCompletionAsync(It.IsAny<UserChallenge>(), It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _challengeServiceMock
            .Setup(x => x.UpdateProgressAsync(It.IsAny<UserChallenge>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChallengeProgressResult(true, 1, "Test Challenge", 100, true, 50));

        _seasonPassServiceMock
            .Setup(x => x.AddXPAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SeasonXPResult(true, 50, 100, 1, false, 0));

        var command = new CompleteChallengeCommand(userId, challengeId, gameSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompleted.Should().BeTrue();
        result.Value.XPEarned.Should().Be(50);
    }

    [Fact]
    public async Task Handle_GameSessionNotBelongsToUser_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var challengeId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();
        var userIntId = 1;
        var otherUserIntId = 2;

        var user = CreateUser(userIntId, userId);
        var gameSession = CreateGameSession(gameSessionId, otherUserIntId); // Different user

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _gameSessionRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameSession);

        var command = new CompleteChallengeCommand(userId, challengeId, gameSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("does not belong");
    }

    [Fact]
    public async Task Handle_GameSessionNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var challengeId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _gameSessionRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameSession?)null);

        var command = new CompleteChallengeCommand(userId, challengeId, gameSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Game session not found");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var challengeId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new CompleteChallengeCommand(userId, challengeId, gameSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ValidationFails_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var challengeId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();
        var challengeIntId = 1;

        int capturedUserId = 0;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns<int, CancellationToken>((id, ct) =>
            {
                capturedUserId = id;
                var user = CreateUser(id, userId);
                return Task.FromResult<User?>(user);
            });

        _gameSessionRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns<Guid, CancellationToken>((id, ct) =>
            {
                var session = CreateGameSession(id, capturedUserId);
                return Task.FromResult<GameSession?>(session);
            });

        var challenge = CreateChallenge(challengeIntId, targetValue: 100, xpReward: 50);
        var userChallenge = CreateUserChallenge(1, challengeIntId, challenge);

        _challengeServiceMock
            .Setup(x => x.GetOrCreateUserChallengeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userChallenge);

        _challengeServiceMock
            .Setup(x => x.ValidateChallengeCompletionAsync(It.IsAny<UserChallenge>(), It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CompleteChallengeCommand(userId, challengeId, gameSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("validation failed");
    }

    private static User CreateUser(int id, Guid externalGuid)
    {
        var user = User.Create(externalGuid.ToString(), "test@test.com", "TestUser", AuthProvider.Local);
        SetProperty(user, "Id", id);
        return user;
    }

    private static GameSession CreateGameSession(Guid id, int userId)
    {
        var session = GameSession.Create(userId);
        SetProperty(session, "Id", id);
        return session;
    }

    private static Challenge CreateChallenge(int id, int targetValue, int xpReward)
    {
        var challenge = Challenge.Create(
            "Test Challenge",
            "Test Description",
            ChallengeType.Daily,
            targetValue,
            xpReward,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1));
        SetProperty(challenge, "Id", id);
        return challenge;
    }

    private static UserChallenge CreateUserChallenge(int userId, int challengeId, Challenge challenge)
    {
        var userChallenge = UserChallenge.Create(userId, challengeId);
        SetProperty(userChallenge, "Challenge", challenge);
        return userChallenge;
    }

    private static void SetProperty<TEntity, TValue>(TEntity entity, string propertyName, TValue value) where TEntity : class
    {
        var backingField = typeof(TEntity).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(entity, value);
    }
}
