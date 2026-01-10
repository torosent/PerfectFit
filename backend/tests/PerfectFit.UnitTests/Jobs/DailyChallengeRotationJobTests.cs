using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Infrastructure.Jobs;

namespace PerfectFit.UnitTests.Jobs;

public class DailyChallengeRotationJobTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IGamificationRepository> _repositoryMock;
    private readonly Mock<ILogger<DailyChallengeRotationJob>> _loggerMock;
    private readonly DailyChallengeRotationJob _job;

    public DailyChallengeRotationJobTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _repositoryMock = new Mock<IGamificationRepository>();
        _loggerMock = new Mock<ILogger<DailyChallengeRotationJob>>();

        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IGamificationRepository)))
            .Returns(_repositoryMock.Object);

        _job = new DailyChallengeRotationJob(_scopeFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeactivatesExpiredDailyChallenges()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiredChallenge = CreateChallenge(1, "Expired", ChallengeType.Daily, 
            now.AddDays(-2), now.AddDays(-1), isActive: true);
        var activeChallenge = CreateChallenge(2, "Active", ChallengeType.Daily,
            now.AddHours(-1), now.AddHours(23), isActive: true);
        
        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge> { expiredChallenge, activeChallenge });
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChallengeTemplate>());

        // Act
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.UpdateChallengeAsync(
            It.Is<Challenge>(c => c.Id == 1 && !c.IsActive),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesNewDailyChallengesFromTemplates()
    {
        // Arrange
        var templates = new List<ChallengeTemplate>
        {
            CreateTemplate(1, "Win 3 Games", ChallengeType.Daily, 3, 50),
            CreateTemplate(2, "Score 1000 Points", ChallengeType.Daily, 1000, 75),
        };

        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge>());
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        // Act
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.AddChallengeAsync(
            It.Is<Challenge>(c => c.Type == ChallengeType.Daily),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_SetsChallengeTemplateId()
    {
        // Arrange
        var template = CreateTemplate(42, "Daily Test", ChallengeType.Daily, 5, 50);
        Challenge? createdChallenge = null;

        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge>());
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChallengeTemplate> { template });
        _repositoryMock.Setup(r => r.AddChallengeAsync(It.IsAny<Challenge>(), It.IsAny<CancellationToken>()))
            .Callback<Challenge, CancellationToken>((c, _) => createdChallenge = c)
            .Returns(Task.CompletedTask);

        // Act
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert - Challenge should have template ID set for multi-instance deduplication
        createdChallenge.Should().NotBeNull();
        createdChallenge!.ChallengeTemplateId.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteAsync_NewChallengesHaveCorrectDailyDuration()
    {
        // Arrange
        var template = CreateTemplate(1, "Daily Test", ChallengeType.Daily, 5, 50);
        Challenge? createdChallenge = null;

        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge>());
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChallengeTemplate> { template });
        _repositoryMock.Setup(r => r.AddChallengeAsync(It.IsAny<Challenge>(), It.IsAny<CancellationToken>()))
            .Callback<Challenge, CancellationToken>((c, _) => createdChallenge = c)
            .Returns(Task.CompletedTask);

        // Act
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert
        createdChallenge.Should().NotBeNull();
        (createdChallenge!.EndDate - createdChallenge.StartDate).TotalHours.Should().BeApproximately(24, 1);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotDeactivateActiveNonExpiredChallenges()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var activeChallenge = CreateChallenge(1, "Active", ChallengeType.Daily,
            now.AddHours(-12), now.AddHours(12), isActive: true);

        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge> { activeChallenge });
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChallengeTemplate>());

        // Act
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.UpdateChallengeAsync(
            It.IsAny<Challenge>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_IsIdempotent_NoDoubleCreation()
    {
        // Arrange - Simulate calling twice in succession
        var templates = new List<ChallengeTemplate>
        {
            CreateTemplate(1, "Daily Test", ChallengeType.Daily, 5, 50)
        };
        var now = DateTime.UtcNow;
        var existingChallenge = CreateChallenge(1, "Daily Test", ChallengeType.Daily,
            now, now.AddHours(24), isActive: true);

        // First call returns empty, second returns the created challenge
        var callCount = 0;
        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => callCount++ == 0 
                ? new List<Challenge>() 
                : new List<Challenge> { existingChallenge });
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        // Act - Execute twice
        await _job.ExecuteRotationAsync(CancellationToken.None);
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert - Only one challenge created
        _repositoryMock.Verify(r => r.AddChallengeAsync(
            It.IsAny<Challenge>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CopiesGoalTypeFromTemplate()
    {
        // Arrange - Template with specific GoalType
        var template = CreateTemplate(1, "Win Games", ChallengeType.Daily, 3, 50, ChallengeGoalType.GameCount);
        Challenge? createdChallenge = null;

        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge>());
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChallengeTemplate> { template });
        _repositoryMock.Setup(r => r.AddChallengeAsync(It.IsAny<Challenge>(), It.IsAny<CancellationToken>()))
            .Callback<Challenge, CancellationToken>((c, _) => createdChallenge = c)
            .Returns(Task.CompletedTask);

        // Act
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert
        createdChallenge.Should().NotBeNull();
        createdChallenge!.GoalType.Should().Be(ChallengeGoalType.GameCount);
    }

    [Fact]
    public async Task ExecuteAsync_CopiesNullGoalTypeFromTemplate()
    {
        // Arrange - Template with null GoalType
        var template = CreateTemplate(1, "Generic Challenge", ChallengeType.Daily, 5, 50, goalType: null);
        Challenge? createdChallenge = null;

        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge>());
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChallengeTemplate> { template });
        _repositoryMock.Setup(r => r.AddChallengeAsync(It.IsAny<Challenge>(), It.IsAny<CancellationToken>()))
            .Callback<Challenge, CancellationToken>((c, _) => createdChallenge = c)
            .Returns(Task.CompletedTask);

        // Act
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert
        createdChallenge.Should().NotBeNull();
        createdChallenge!.GoalType.Should().BeNull();
    }

    [Theory]
    [InlineData(ChallengeGoalType.ScoreTotal)]
    [InlineData(ChallengeGoalType.ScoreSingleGame)]
    [InlineData(ChallengeGoalType.WinStreak)]
    [InlineData(ChallengeGoalType.Accuracy)]
    [InlineData(ChallengeGoalType.TimeBased)]
    public async Task ExecuteAsync_CopiesAllGoalTypesCorrectly(ChallengeGoalType goalType)
    {
        // Arrange
        var template = CreateTemplate(1, "Parameterized Test", ChallengeType.Daily, 5, 50, goalType);
        Challenge? createdChallenge = null;

        _repositoryMock.Setup(r => r.GetActiveChallengesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Challenge>());
        _repositoryMock.Setup(r => r.GetChallengeTemplatesAsync(ChallengeType.Daily, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChallengeTemplate> { template });
        _repositoryMock.Setup(r => r.AddChallengeAsync(It.IsAny<Challenge>(), It.IsAny<CancellationToken>()))
            .Callback<Challenge, CancellationToken>((c, _) => createdChallenge = c)
            .Returns(Task.CompletedTask);

        // Act
        await _job.ExecuteRotationAsync(CancellationToken.None);

        // Assert
        createdChallenge.Should().NotBeNull();
        createdChallenge!.GoalType.Should().Be(goalType);
    }

    #region Helper Methods

    private static Challenge CreateChallenge(int id, string name, ChallengeType type,
        DateTime startDate, DateTime endDate, bool isActive = true, ChallengeGoalType? goalType = null)
    {
        var challenge = Challenge.Create(name, $"{name} description", type, 10, 50, startDate, endDate, goalType: goalType);
        SetProperty(challenge, "Id", id);
        if (!isActive)
        {
            challenge.Deactivate();
        }
        return challenge;
    }

    private static ChallengeTemplate CreateTemplate(int id, string name, ChallengeType type,
        int targetValue, int xpReward, ChallengeGoalType? goalType = null)
    {
        var template = ChallengeTemplate.Create(name, $"{name} description", type, targetValue, xpReward, goalType);
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
