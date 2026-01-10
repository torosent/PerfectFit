using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Gamification;
using PerfectFit.UseCases.Gamification.Queries;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Queries;

public class GetPersonalGoalsQueryTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPersonalGoalService> _personalGoalServiceMock;
    private readonly GetPersonalGoalsQueryHandler _handler;

    public GetPersonalGoalsQueryTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _personalGoalServiceMock = new Mock<IPersonalGoalService>();
        _handler = new GetPersonalGoalsQueryHandler(_userRepositoryMock.Object, _personalGoalServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsActiveGoalsWithProgress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var goals = new List<PersonalGoal>
        {
            CreatePersonalGoal(1, userIntId, GoalType.BeatAverage, 1000, 500),
            CreatePersonalGoal(2, userIntId, GoalType.ImproveAccuracy, 10, 7),
            CreatePersonalGoal(3, userIntId, GoalType.NewPersonalBest, 5, 5, isCompleted: true)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _personalGoalServiceMock
            .Setup(x => x.GetActiveGoalsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(goals);

        var query = new GetPersonalGoalsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_CalculatesProgressPercentage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var goals = new List<PersonalGoal>
        {
            CreatePersonalGoal(1, userIntId, GoalType.BeatAverage, 100, 50) // 50% progress
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _personalGoalServiceMock
            .Setup(x => x.GetActiveGoalsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(goals);

        var query = new GetPersonalGoalsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result[0].ProgressPercentage.Should().Be(50);
    }

    [Fact]
    public async Task Handle_CompletedGoals_MarkedAsCompleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var goals = new List<PersonalGoal>
        {
            CreatePersonalGoal(1, userIntId, GoalType.BeatAverage, 100, 100, isCompleted: true)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _personalGoalServiceMock
            .Setup(x => x.GetActiveGoalsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(goals);

        var query = new GetPersonalGoalsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result[0].IsCompleted.Should().BeTrue();
        result[0].ProgressPercentage.Should().Be(100);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetPersonalGoalsQuery(userId);

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_NoActiveGoals_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _personalGoalServiceMock
            .Setup(x => x.GetActiveGoalsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PersonalGoal>());

        var query = new GetPersonalGoalsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_GoalsWithExpiry_IncludeExpiryDate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;
        var expiresAt = DateTime.UtcNow.AddDays(1);

        var user = CreateUser(userIntId, userId);
        var goals = new List<PersonalGoal>
        {
            CreatePersonalGoal(1, userIntId, GoalType.BeatAverage, 100, 50, expiresAt: expiresAt)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _personalGoalServiceMock
            .Setup(x => x.GetActiveGoalsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(goals);

        var query = new GetPersonalGoalsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result[0].ExpiresAt.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));
    }

    private static User CreateUser(int id, Guid externalGuid)
    {
        var user = User.Create(externalGuid.ToString(), "test@test.com", "TestUser", AuthProvider.Local);
        SetProperty(user, "Id", id);
        return user;
    }

    private static PersonalGoal CreatePersonalGoal(int id, int userId, GoalType type, int targetValue, int currentValue, bool isCompleted = false, DateTime? expiresAt = null)
    {
        var goal = PersonalGoal.Create(userId, type, targetValue, $"Goal: {type}", expiresAt ?? DateTime.UtcNow.AddDays(1));
        SetProperty(goal, "Id", id);
        SetProperty(goal, "CurrentValue", currentValue);
        SetProperty(goal, "IsCompleted", isCompleted);
        return goal;
    }

    private static void SetProperty<TEntity, TValue>(TEntity entity, string propertyName, TValue value) where TEntity : class
    {
        var backingField = typeof(TEntity).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(entity, value);
    }
}
