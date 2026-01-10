using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Infrastructure.Jobs;

namespace PerfectFit.UnitTests.Jobs;

public class SeasonTransitionJobTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IGamificationRepository> _repositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<SeasonTransitionJob>> _loggerMock;
    private readonly SeasonTransitionJob _job;

    public SeasonTransitionJobTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _repositoryMock = new Mock<IGamificationRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<SeasonTransitionJob>>();

        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IGamificationRepository)))
            .Returns(_repositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUserRepository)))
            .Returns(_userRepositoryMock.Object);

        _job = new SeasonTransitionJob(_scopeFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DetectsEndedSeason()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var endedSeason = CreateSeason(1, "Season 1", 1, now.AddDays(-90), now.AddDays(-1), isActive: true);
        var nextSeason = CreateSeason(2, "Season 2", 2, now, now.AddDays(89), isActive: false);

        _repositoryMock.Setup(r => r.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Season?)null); // No current active season with valid dates
        _repositoryMock.Setup(r => r.GetAllSeasonsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Season> { endedSeason, nextSeason });

        // Act
        await _job.ExecuteTransitionAsync(CancellationToken.None);

        // Assert - Should deactivate ended season
        _repositoryMock.Verify(r => r.UpdateSeasonAsync(
            It.Is<Season>(s => s.Id == 1 && !s.IsActive),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ActivatesNewSeason()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var endedSeason = CreateSeason(1, "Season 1", 1, now.AddDays(-90), now.AddDays(-1), isActive: true);
        var newSeason = CreateSeason(2, "Season 2", 2, now, now.AddDays(89), isActive: false);

        _repositoryMock.Setup(r => r.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Season?)null);
        _repositoryMock.Setup(r => r.GetAllSeasonsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Season> { endedSeason, newSeason });

        // Act
        await _job.ExecuteTransitionAsync(CancellationToken.None);

        // Assert - Should activate new season
        _repositoryMock.Verify(r => r.UpdateSeasonAsync(
            It.Is<Season>(s => s.Id == 2 && s.IsActive),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ResetsUserSeasonProgress()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var endedSeason = CreateSeason(1, "Season 1", 1, now.AddDays(-90), now.AddDays(-1), isActive: true);
        var newSeason = CreateSeason(2, "Season 2", 2, now, now.AddDays(89), isActive: false);

        var users = new List<User>
        {
            CreateUser(1, 5000, 10),
            CreateUser(2, 3000, 7),
        };

        _repositoryMock.Setup(r => r.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Season?)null);
        _repositoryMock.Setup(r => r.GetAllSeasonsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Season> { endedSeason, newSeason });
        _userRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        await _job.ExecuteTransitionAsync(CancellationToken.None);

        // Assert - Should reset all users' season progress
        _userRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<User>(u => u.SeasonPassXP == 0 && u.CurrentSeasonTier == 0),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_ArchivesUserProgress()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var endedSeason = CreateSeason(1, "Season 1", 1, now.AddDays(-90), now.AddDays(-1), isActive: true);
        var newSeason = CreateSeason(2, "Season 2", 2, now, now.AddDays(89), isActive: false);

        var user = CreateUser(1, 5000, 10);

        _repositoryMock.Setup(r => r.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Season?)null);
        _repositoryMock.Setup(r => r.GetAllSeasonsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Season> { endedSeason, newSeason });
        _userRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        // Act
        await _job.ExecuteTransitionAsync(CancellationToken.None);

        // Assert - Should archive user's season progress
        _repositoryMock.Verify(r => r.AddSeasonArchiveAsync(
            It.Is<SeasonArchive>(sa => sa.UserId == 1 && sa.SeasonId == 1 && sa.FinalXP == 5000 && sa.FinalTier == 10),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NoActionWhenSeasonStillActive()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var activeSeason = CreateSeason(1, "Season 1", 1, now.AddDays(-30), now.AddDays(60), isActive: true);

        _repositoryMock.Setup(r => r.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeSeason);
        _repositoryMock.Setup(r => r.GetAllSeasonsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Season> { activeSeason });

        // Act
        await _job.ExecuteTransitionAsync(CancellationToken.None);

        // Assert - No season changes
        _repositoryMock.Verify(r => r.UpdateSeasonAsync(
            It.IsAny<Season>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_IsIdempotent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var activeSeason = CreateSeason(2, "Season 2", 2, now, now.AddDays(89), isActive: true);

        _repositoryMock.Setup(r => r.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeSeason);
        _repositoryMock.Setup(r => r.GetAllSeasonsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Season> { activeSeason });

        // Act - Execute twice
        await _job.ExecuteTransitionAsync(CancellationToken.None);
        await _job.ExecuteTransitionAsync(CancellationToken.None);

        // Assert - No double processing
        _repositoryMock.Verify(r => r.UpdateSeasonAsync(
            It.IsAny<Season>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    #region Helper Methods

    private static Season CreateSeason(int id, string name, int number, DateTime startDate, DateTime endDate, bool isActive)
    {
        var season = Season.Create(name, number, "Default Theme", startDate, endDate);
        SetProperty(season, "Id", id);
        if (!isActive)
        {
            season.Deactivate();
        }
        return season;
    }

    private static User CreateUser(int id, int seasonXP, int seasonTier)
    {
        var user = User.Create($"ext_{id}", $"test{id}@test.com", $"User{id}", AuthProvider.Local);
        SetProperty(user, "Id", id);
        SetProperty(user, "SeasonPassXP", seasonXP);
        SetProperty(user, "CurrentSeasonTier", seasonTier);
        return user;
    }

    private static void SetProperty<T, TValue>(T obj, string propertyName, TValue value) where T : class
    {
        var backingField = typeof(T).GetField($"<{propertyName}>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        backingField?.SetValue(obj, value);
    }

    #endregion
}
