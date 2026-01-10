using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.UnitTests.Entities;

public class ChallengeEntityTests
{
    #region Challenge Tests

    [Fact]
    public void Challenge_Create_SetsProperties()
    {
        // Arrange
        var name = "Daily High Score";
        var description = "Score at least 5000 points in a single game";
        var type = ChallengeType.Daily;
        var targetValue = 5000;
        var xpReward = 100;
        var startDate = DateTime.UtcNow.Date;
        var endDate = DateTime.UtcNow.Date.AddDays(1);

        // Act
        var challenge = Challenge.Create(name, description, type, targetValue, xpReward, startDate, endDate);

        // Assert
        challenge.Id.Should().Be(0); // Not set until persisted
        challenge.Name.Should().Be(name);
        challenge.Description.Should().Be(description);
        challenge.Type.Should().Be(type);
        challenge.TargetValue.Should().Be(targetValue);
        challenge.XPReward.Should().Be(xpReward);
        challenge.StartDate.Should().Be(startDate);
        challenge.EndDate.Should().Be(endDate);
        challenge.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Challenge_Create_ThrowsWhenNameEmpty()
    {
        // Arrange & Act
        var act = () => Challenge.Create(
            "",
            "Description",
            ChallengeType.Daily,
            1000,
            50,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Challenge_Create_ThrowsWhenTargetValueNegative()
    {
        // Arrange & Act
        var act = () => Challenge.Create(
            "Test",
            "Description",
            ChallengeType.Daily,
            -1,
            50,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("targetValue");
    }

    [Fact]
    public void Challenge_Create_ThrowsWhenEndDateBeforeStartDate()
    {
        // Arrange & Act
        var act = () => Challenge.Create(
            "Test",
            "Description",
            ChallengeType.Daily,
            1000,
            50,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("endDate");
    }

    [Fact]
    public void Challenge_Deactivate_SetsIsActiveFalse()
    {
        // Arrange
        var challenge = Challenge.Create(
            "Test Challenge",
            "Description",
            ChallengeType.Weekly,
            1000,
            100,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7));

        // Act
        challenge.Deactivate();

        // Assert
        challenge.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Challenge_Activate_SetsIsActiveTrue()
    {
        // Arrange
        var challenge = Challenge.Create(
            "Test Challenge",
            "Description",
            ChallengeType.Weekly,
            1000,
            100,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7));
        challenge.Deactivate();

        // Act
        challenge.Activate();

        // Assert
        challenge.IsActive.Should().BeTrue();
    }

    #endregion

    #region UserChallenge Tests

    [Fact]
    public void UserChallenge_Create_SetsDefaults()
    {
        // Arrange
        var userId = 1;
        var challengeId = 5;

        // Act
        var userChallenge = UserChallenge.Create(userId, challengeId);

        // Assert
        userChallenge.Id.Should().Be(0); // Not set until persisted
        userChallenge.UserId.Should().Be(userId);
        userChallenge.ChallengeId.Should().Be(challengeId);
        userChallenge.CurrentProgress.Should().Be(0);
        userChallenge.IsCompleted.Should().BeFalse();
        userChallenge.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void UserChallenge_UpdateProgress_TracksCompletion()
    {
        // Arrange
        var userChallenge = UserChallenge.Create(1, 5);
        var targetValue = 1000;

        // Act
        userChallenge.UpdateProgress(500, targetValue);

        // Assert
        userChallenge.CurrentProgress.Should().Be(500);
        userChallenge.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void UserChallenge_UpdateProgress_CompletesWhenTargetReached()
    {
        // Arrange
        var userChallenge = UserChallenge.Create(1, 5);
        var targetValue = 1000;

        // Act
        userChallenge.UpdateProgress(1000, targetValue);

        // Assert
        userChallenge.CurrentProgress.Should().Be(1000);
        userChallenge.IsCompleted.Should().BeTrue();
        userChallenge.CompletedAt.Should().NotBeNull();
        userChallenge.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UserChallenge_UpdateProgress_CompletesWhenTargetExceeded()
    {
        // Arrange
        var userChallenge = UserChallenge.Create(1, 5);
        var targetValue = 1000;

        // Act
        userChallenge.UpdateProgress(1500, targetValue);

        // Assert
        userChallenge.CurrentProgress.Should().Be(1500);
        userChallenge.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void UserChallenge_UpdateProgress_DoesNotOverwriteCompletedAt()
    {
        // Arrange
        var userChallenge = UserChallenge.Create(1, 5);
        userChallenge.UpdateProgress(1000, 1000);
        var firstCompletedAt = userChallenge.CompletedAt;

        Thread.Sleep(10);

        // Act - update with higher progress
        userChallenge.UpdateProgress(1500, 1000);

        // Assert
        userChallenge.CompletedAt.Should().Be(firstCompletedAt);
    }

    [Fact]
    public void UserChallenge_AddProgress_AccumulatesProgress()
    {
        // Arrange
        var userChallenge = UserChallenge.Create(1, 5);
        var targetValue = 1000;

        // Act
        userChallenge.AddProgress(300, targetValue);
        userChallenge.AddProgress(400, targetValue);

        // Assert
        userChallenge.CurrentProgress.Should().Be(700);
        userChallenge.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void UserChallenge_AddProgress_CompletesWhenAccumulatedTargetReached()
    {
        // Arrange
        var userChallenge = UserChallenge.Create(1, 5);
        var targetValue = 1000;

        // Act
        userChallenge.AddProgress(600, targetValue);
        userChallenge.AddProgress(500, targetValue);

        // Assert
        userChallenge.CurrentProgress.Should().Be(1100);
        userChallenge.IsCompleted.Should().BeTrue();
    }

    #endregion
}
