using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;
using System.Reflection;

namespace PerfectFit.UnitTests.Services;

public class ChallengeServiceTests
{
    private readonly Mock<IGamificationRepository> _repositoryMock;
    private readonly ChallengeService _service;

    public ChallengeServiceTests()
    {
        _repositoryMock = new Mock<IGamificationRepository>();
        _service = new ChallengeService(_repositoryMock.Object);
    }

    #region GetActiveChallengesAsync Tests

    [Fact]
    public async Task GetActiveChallenges_ReturnsOnlyActive()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var challenges = new List<Challenge>
        {
            CreateChallenge(1, ChallengeType.Daily, isActive: true, startDate: now.AddDays(-1), endDate: now.AddDays(1)),
            CreateChallenge(2, ChallengeType.Weekly, isActive: true, startDate: now.AddDays(-1), endDate: now.AddDays(6)),
        };

        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenges);

        // Act
        var result = await _service.GetActiveChallengesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(c => c.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveChallenges_FiltersByType()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var dailyChallenges = new List<Challenge>
        {
            CreateChallenge(1, ChallengeType.Daily, isActive: true, startDate: now.AddDays(-1), endDate: now.AddDays(1)),
        };

        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyChallenges);

        // Act
        var result = await _service.GetActiveChallengesAsync(ChallengeType.Daily);

        // Assert
        result.Should().HaveCount(1);
        result[0].Type.Should().Be(ChallengeType.Daily);
    }

    #endregion

    #region GetOrCreateUserChallengeAsync Tests

    [Fact]
    public async Task GetOrCreateUserChallenge_CreatesIfNotExists()
    {
        // Arrange
        var challenge = CreateChallenge(1, ChallengeType.Daily, targetValue: 100);

        _repositoryMock.Setup(r => r.GetUserChallengeAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserChallenge?)null);
        _repositoryMock.Setup(r => r.GetChallengeByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenge);

        // Act
        var result = await _service.GetOrCreateUserChallengeAsync(1, 1);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(1);
        result.ChallengeId.Should().Be(1);
        result.CurrentProgress.Should().Be(0);
        _repositoryMock.Verify(r => r.AddUserChallengeAsync(It.IsAny<UserChallenge>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateUserChallenge_ReturnsExisting()
    {
        // Arrange
        var existingChallenge = UserChallenge.Create(1, 1);
        SetUserChallengeProgress(existingChallenge, 50);

        _repositoryMock.Setup(r => r.GetUserChallengeAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingChallenge);

        // Act
        var result = await _service.GetOrCreateUserChallengeAsync(1, 1);

        // Assert
        result.Should().NotBeNull();
        result.CurrentProgress.Should().Be(50);
        _repositoryMock.Verify(r => r.AddUserChallengeAsync(It.IsAny<UserChallenge>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UpdateProgressAsync Tests

    [Fact]
    public async Task UpdateProgress_IncrementsCorrectly()
    {
        // Arrange
        var challenge = CreateChallenge(1, ChallengeType.Daily, targetValue: 100, xpReward: 50);
        var userChallenge = UserChallenge.Create(1, 1);
        SetUserChallengeProgress(userChallenge, 20);
        SetUserChallengeRelations(userChallenge, challenge);

        _repositoryMock.Setup(r => r.GetChallengeByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenge);

        // Act
        var result = await _service.UpdateProgressAsync(userChallenge, 30);

        // Assert
        result.Success.Should().BeTrue();
        result.NewProgress.Should().Be(50);
        result.IsCompleted.Should().BeFalse();
        result.XPEarned.Should().Be(0);
    }

    [Fact]
    public async Task UpdateProgress_CompletesWhenTargetReached()
    {
        // Arrange
        var challenge = CreateChallenge(1, ChallengeType.Daily, targetValue: 100, xpReward: 50);
        var userChallenge = UserChallenge.Create(1, 1);
        SetUserChallengeProgress(userChallenge, 80);
        SetUserChallengeRelations(userChallenge, challenge);

        _repositoryMock.Setup(r => r.GetChallengeByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenge);

        // Act
        var result = await _service.UpdateProgressAsync(userChallenge, 30);

        // Assert
        result.Success.Should().BeTrue();
        result.NewProgress.Should().Be(110);
        result.IsCompleted.Should().BeTrue();
        result.XPEarned.Should().Be(50);
    }

    [Fact]
    public async Task UpdateProgress_AlreadyCompleted_NoXP()
    {
        // Arrange
        var challenge = CreateChallenge(1, ChallengeType.Daily, targetValue: 100, xpReward: 50);
        var userChallenge = UserChallenge.Create(1, 1);
        SetUserChallengeProgress(userChallenge, 100);
        SetUserChallengeCompleted(userChallenge);
        SetUserChallengeRelations(userChallenge, challenge);

        _repositoryMock.Setup(r => r.GetChallengeByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(challenge);

        // Act
        var result = await _service.UpdateProgressAsync(userChallenge, 10);

        // Assert
        result.Success.Should().BeTrue();
        result.IsCompleted.Should().BeTrue();
        result.XPEarned.Should().Be(0); // No XP because already completed
    }

    #endregion

    #region ValidateChallengeCompletionAsync Tests

    [Fact]
    public async Task ValidateChallengeCompletion_ValidGame_ReturnsTrue()
    {
        // Arrange
        var challenge = CreateChallenge(1, ChallengeType.Daily, targetValue: 100);
        var userChallenge = UserChallenge.Create(1, 1);
        SetUserChallengeRelations(userChallenge, challenge);

        var gameSession = CreateGameSession(userId: 1, score: 150, status: GameStatus.Ended);

        // Act
        var result = await _service.ValidateChallengeCompletionAsync(userChallenge, gameSession);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateChallengeCompletion_InvalidGame_ReturnsFalse()
    {
        // Arrange
        var challenge = CreateChallenge(1, ChallengeType.Daily, targetValue: 100);
        var userChallenge = UserChallenge.Create(1, 1);
        SetUserChallengeRelations(userChallenge, challenge);

        // Game belongs to different user
        var gameSession = CreateGameSession(userId: 2, score: 150, status: GameStatus.Ended);

        // Act
        var result = await _service.ValidateChallengeCompletionAsync(userChallenge, gameSession);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateChallengeCompletion_GameNotEnded_ReturnsFalse()
    {
        // Arrange
        var challenge = CreateChallenge(1, ChallengeType.Daily, targetValue: 100);
        var userChallenge = UserChallenge.Create(1, 1);
        SetUserChallengeRelations(userChallenge, challenge);

        var gameSession = CreateGameSession(userId: 1, score: 150, status: GameStatus.Playing);

        // Act
        var result = await _service.ValidateChallengeCompletionAsync(userChallenge, gameSession);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private static Challenge CreateChallenge(
        int id,
        ChallengeType type,
        int targetValue = 100,
        int xpReward = 50,
        bool isActive = true,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-1);
        var end = endDate ?? DateTime.UtcNow.AddDays(7);
        var challenge = Challenge.Create($"Challenge {id}", $"Description {id}", type, targetValue, xpReward, start, end);

        SetProperty(challenge, "Id", id);
        if (!isActive) challenge.Deactivate();

        return challenge;
    }

    private static void SetUserChallengeProgress(UserChallenge userChallenge, int progress)
    {
        SetProperty(userChallenge, "CurrentProgress", progress);
    }

    private static void SetUserChallengeCompleted(UserChallenge userChallenge)
    {
        SetProperty(userChallenge, "IsCompleted", true);
        SetProperty(userChallenge, "CompletedAt", DateTime.UtcNow);
    }

    private static void SetUserChallengeRelations(UserChallenge userChallenge, Challenge challenge)
    {
        SetProperty(userChallenge, "Challenge", challenge);
    }

    private static GameSession CreateGameSession(int userId, int score, GameStatus status)
    {
        var session = GameSession.Create(userId);
        session.AddScore(score, score / 10);
        if (status == GameStatus.Ended)
        {
            session.EndGame();
        }
        return session;
    }

    private static void SetProperty<T, TValue>(T obj, string propertyName, TValue value) where T : class
    {
        var backingField = typeof(T).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(obj, value);
    }

    #endregion
}
