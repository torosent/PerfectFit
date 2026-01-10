using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Gamification;
using PerfectFit.UseCases.Gamification.Queries;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Queries;

public class GetChallengesQueryTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IChallengeService> _challengeServiceMock;
    private readonly GetChallengesQueryHandler _handler;

    public GetChallengesQueryTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _challengeServiceMock = new Mock<IChallengeService>();
        _handler = new GetChallengesQueryHandler(_userRepositoryMock.Object, _challengeServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAllActiveChallenges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var challenges = new List<Challenge>
        {
            CreateChallenge(1, "Daily Challenge 1", ChallengeType.Daily, 100, 50),
            CreateChallenge(2, "Daily Challenge 2", ChallengeType.Daily, 200, 75),
            CreateChallenge(3, "Weekly Challenge", ChallengeType.Weekly, 1000, 200)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _challengeServiceMock
            .Setup(x => x.GetActiveChallengesAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenges);

        SetupUserChallenges(userIntId, challenges);

        var query = new GetChallengesQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_FiltersByType_ReturnsOnlyDailyChallenges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var dailyChallenges = new List<Challenge>
        {
            CreateChallenge(1, "Daily Challenge 1", ChallengeType.Daily, 100, 50),
            CreateChallenge(2, "Daily Challenge 2", ChallengeType.Daily, 200, 75)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _challengeServiceMock
            .Setup(x => x.GetActiveChallengesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyChallenges);

        SetupUserChallenges(userIntId, dailyChallenges);

        var query = new GetChallengesQuery(userId, ChallengeType.Daily);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.All(c => c.Type == ChallengeType.Daily).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsProgressForEachChallenge()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var challenges = new List<Challenge>
        {
            CreateChallenge(1, "Score Challenge", ChallengeType.Daily, 100, 50)
        };

        var userChallenge = CreateUserChallenge(userIntId, 1, currentProgress: 75);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _challengeServiceMock
            .Setup(x => x.GetActiveChallengesAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenges);

        _challengeServiceMock
            .Setup(x => x.GetOrCreateUserChallengeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userChallenge);

        var query = new GetChallengesQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].CurrentProgress.Should().Be(75);
        result[0].TargetValue.Should().Be(100);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetChallengesQuery(userId);

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_NoChallenges_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _challengeServiceMock
            .Setup(x => x.GetActiveChallengesAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge>());

        var query = new GetChallengesQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CompletedChallenge_MarksAsCompleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var challenges = new List<Challenge>
        {
            CreateChallenge(1, "Score Challenge", ChallengeType.Daily, 100, 50)
        };

        var userChallenge = CreateUserChallenge(userIntId, 1, currentProgress: 100, isCompleted: true);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _challengeServiceMock
            .Setup(x => x.GetActiveChallengesAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenges);

        _challengeServiceMock
            .Setup(x => x.GetOrCreateUserChallengeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userChallenge);

        var query = new GetChallengesQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].IsCompleted.Should().BeTrue();
    }

    private void SetupUserChallenges(int userIntId, List<Challenge> challenges)
    {
        foreach (var challenge in challenges)
        {
            var userChallenge = CreateUserChallenge(userIntId, challenge.Id);
            _challengeServiceMock
                .Setup(x => x.GetOrCreateUserChallengeAsync(It.IsAny<int>(), challenge.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userChallenge);
        }
    }

    private static User CreateUser(int id, Guid externalGuid)
    {
        var user = User.Create(externalGuid.ToString(), "test@test.com", "TestUser", AuthProvider.Local);
        SetProperty(user, "Id", id);
        return user;
    }

    private static Challenge CreateChallenge(int id, string name, ChallengeType type, int targetValue, int xpReward)
    {
        var challenge = Challenge.Create(name, "Test", type, targetValue, xpReward, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));
        SetProperty(challenge, "Id", id);
        return challenge;
    }

    private static UserChallenge CreateUserChallenge(int userId, int challengeId, int currentProgress = 0, bool isCompleted = false)
    {
        var userChallenge = UserChallenge.Create(userId, challengeId);
        SetProperty(userChallenge, "CurrentProgress", currentProgress);
        SetProperty(userChallenge, "IsCompleted", isCompleted);
        return userChallenge;
    }

    private static void SetProperty<TEntity, TValue>(TEntity entity, string propertyName, TValue value) where TEntity : class
    {
        var backingField = typeof(TEntity).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(entity, value);
    }
}
