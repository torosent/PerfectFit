using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.UnitTests.Entities;

public class PersonalGoalEntityTests
{
    #region PersonalGoal Tests

    [Fact]
    public void PersonalGoal_Create_SetsProperties()
    {
        // Arrange
        var userId = 1;
        var type = GoalType.BeatAverage;
        var targetValue = 5000;
        var description = "Beat your average score of 4500";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var goal = PersonalGoal.Create(userId, type, targetValue, description, expiresAt);

        // Assert
        goal.Id.Should().Be(0); // Not set until persisted
        goal.UserId.Should().Be(userId);
        goal.Type.Should().Be(type);
        goal.TargetValue.Should().Be(targetValue);
        goal.CurrentValue.Should().Be(0);
        goal.Description.Should().Be(description);
        goal.IsCompleted.Should().BeFalse();
        goal.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        goal.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void PersonalGoal_Create_ThrowsWhenTargetValueNegative()
    {
        // Arrange & Act
        var act = () => PersonalGoal.Create(
            1,
            GoalType.BeatAverage,
            -100,
            "Description",
            DateTime.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("targetValue");
    }

    [Fact]
    public void PersonalGoal_Create_AllowsNullExpiresAt()
    {
        // Arrange & Act
        var goal = PersonalGoal.Create(1, GoalType.NewPersonalBest, 10000, "Get a new personal best", null);

        // Assert
        goal.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void PersonalGoal_UpdateProgress_SetsCurrentValue()
    {
        // Arrange
        var goal = PersonalGoal.Create(1, GoalType.BeatAverage, 5000, "Beat average", DateTime.UtcNow.AddDays(7));

        // Act
        goal.UpdateProgress(3500);

        // Assert
        goal.CurrentValue.Should().Be(3500);
        goal.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void PersonalGoal_UpdateProgress_CompletesWhenTargetReached()
    {
        // Arrange
        var goal = PersonalGoal.Create(1, GoalType.BeatAverage, 5000, "Beat average", DateTime.UtcNow.AddDays(7));

        // Act
        goal.UpdateProgress(5000);

        // Assert
        goal.CurrentValue.Should().Be(5000);
        goal.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void PersonalGoal_UpdateProgress_CompletesWhenTargetExceeded()
    {
        // Arrange
        var goal = PersonalGoal.Create(1, GoalType.BeatAverage, 5000, "Beat average", DateTime.UtcNow.AddDays(7));

        // Act
        goal.UpdateProgress(7500);

        // Assert
        goal.CurrentValue.Should().Be(7500);
        goal.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void PersonalGoal_IsExpired_ReturnsTrueWhenPastExpiryDate()
    {
        // Arrange
        var goal = PersonalGoal.Create(
            1,
            GoalType.ImproveAccuracy,
            90,
            "Improve accuracy",
            DateTime.UtcNow.AddDays(-1)); // Already expired

        // Act & Assert
        goal.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void PersonalGoal_IsExpired_ReturnsFalseWhenNotExpired()
    {
        // Arrange
        var goal = PersonalGoal.Create(
            1,
            GoalType.ImproveAccuracy,
            90,
            "Improve accuracy",
            DateTime.UtcNow.AddDays(7));

        // Act & Assert
        goal.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void PersonalGoal_IsExpired_ReturnsFalseWhenNoExpiry()
    {
        // Arrange
        var goal = PersonalGoal.Create(
            1,
            GoalType.NewPersonalBest,
            10000,
            "Get personal best",
            null);

        // Act & Assert
        goal.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void PersonalGoal_AllGoalTypes_CanBeCreated()
    {
        // Arrange & Act
        var beatAverage = PersonalGoal.Create(1, GoalType.BeatAverage, 5000, "Beat average", null);
        var improveAccuracy = PersonalGoal.Create(1, GoalType.ImproveAccuracy, 90, "Improve accuracy", null);
        var newPersonalBest = PersonalGoal.Create(1, GoalType.NewPersonalBest, 10000, "New PB", null);

        // Assert
        beatAverage.Type.Should().Be(GoalType.BeatAverage);
        improveAccuracy.Type.Should().Be(GoalType.ImproveAccuracy);
        newPersonalBest.Type.Should().Be(GoalType.NewPersonalBest);
    }

    #endregion
}
