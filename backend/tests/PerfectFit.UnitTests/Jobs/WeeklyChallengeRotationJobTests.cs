using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Infrastructure.Jobs;

namespace PerfectFit.UnitTests.Jobs;

public class WeeklyChallengeRotationJobTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IGamificationRepository> _repositoryMock;
    private readonly Mock<ILogger<WeeklyChallengeRotationJob>> _loggerMock;
    private readonly WeeklyChallengeRotationJob _job;

    public WeeklyChallengeRotationJobTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _repositoryMock = new Mock<IGamificationRepository>();
        _loggerMock = new Mock<ILogger<WeeklyChallengeRotationJob>>();

        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IGamificationRepository)))
            .Returns(_repositoryMock.Object);

        _job = new WeeklyChallengeRotationJob(_scopeFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeactivatesExpiredWeeklyChallenges()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiredChallenge = CreateChallenge(1, "Expired Weekly", ChallengeType.Weekly,
            now.AddDays(-14), now.AddDays(-7), isActive: true);
        var activeChallenge = CreateChallenge(2, "Active Weekly", ChallengeType.Weekly,
            now.AddDays(-3), now.AddDays(4), isActive: true);

        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Weekly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge> { expiredChallenge, activeChallenge });
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Weekly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChallengeTemplate>());

        // Act
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.UpdateChallengeAsync(
            It.Is<Challenge>(c => c.Id == 1 && !c.IsActive),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesNewWeeklyChallengesFromTemplates()
    {
        // Arrange
        var templates = new List<ChallengeTemplate>
        {
            CreateTemplate(1, "Win 25 Games", ChallengeType.Weekly, 25, 200),
            CreateTemplate(2, "Score 10000 Total Points", ChallengeType.Weekly, 10000, 300),
        };

        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Weekly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge>());
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Weekly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        // Act
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.AddChallengeAsync(
            It.Is<Challenge>(c => c.Type == ChallengeType.Weekly),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_NewChallengesHaveCorrectWeeklyDuration()
    {
        // Arrange
        var template = CreateTemplate(1, "Weekly Test", ChallengeType.Weekly, 50, 200);
        Challenge? createdChallenge = null;

        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Weekly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge>());
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Weekly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChallengeTemplate> { template });
        _repositoryMock.Setup(r => r.AddChallengeAsync(It.IsAny<Challenge>(), It.IsAny<CancellationToken>()))
            .Callback<Challenge, CancellationToken>((c, _) => createdChallenge = c)
            .Returns(Task.CompletedTask);

        // Act
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert
        createdChallenge.Should().NotBeNull();
        (createdChallenge!.EndDate - createdChallenge.StartDate).TotalDays.Should().BeApproximately(7, 0.1);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyTemplatesList()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Weekly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge>());
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Weekly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChallengeTemplate>());

        // Act
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert - No exceptions, no challenges created
        _repositoryMock.Verify(r => r.AddChallengeAsync(
            It.IsAny<Challenge>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsCreationWhenActiveChallengesExist()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var activeChallenge = CreateChallenge(1, "Existing Weekly", ChallengeType.Weekly,
            now.AddDays(-1), now.AddDays(6), isActive: true);
        var templates = new List<ChallengeTemplate>
        {
            CreateTemplate(1, "New Weekly", ChallengeType.Weekly, 50, 200)
        };

        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Weekly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge> { activeChallenge });
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Weekly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        // Act
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert - Should not create new challenges when valid ones exist
        _repositoryMock.Verify(r => r.AddChallengeAsync(
            It.IsAny<Challenge>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    #region Helper Methods

    private static Challenge CreateChallenge(int id, string name, ChallengeType type,
        DateTime startDate, DateTime endDate, bool isActive = true)
    {
        var challenge = Challenge.Create(name, $"{name} description", type, 10, 50, startDate, endDate);
        SetProperty(challenge, "Id", id);
        if (!isActive)
        {
            challenge.Deactivate();
        }
        return challenge;
    }

    private static ChallengeTemplate CreateTemplate(int id, string name, ChallengeType type,
        int targetValue, int xpReward)
    {
        var template = ChallengeTemplate.Create(name, $"{name} description", type, targetValue, xpReward);
        SetProperty(template, "Id", id);
        return template;
    }

    private static void SetProperty<T, TValue>(T obj, string propertyName, TValue value) where T : class
    {
        var backingField = typeof(T).GetField($"<{propertyName}>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        backingField?.SetValue(obj, value);
    }

    #endregion
}
