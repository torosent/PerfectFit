using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.UnitTests.Entities;

public class ChallengeGoalTypeTests
{
    #region ChallengeGoalType Enum Tests

    [Fact]
    public void ChallengeGoalType_HasExpectedValues()
    {
        // Assert enum values exist with expected integer values
        ((int)ChallengeGoalType.ScoreTotal).Should().Be(0);
        ((int)ChallengeGoalType.ScoreSingleGame).Should().Be(1);
        ((int)ChallengeGoalType.GameCount).Should().Be(2);
        ((int)ChallengeGoalType.WinStreak).Should().Be(3);
        ((int)ChallengeGoalType.Accuracy).Should().Be(4);
        ((int)ChallengeGoalType.TimeBased).Should().Be(5);
    }

    [Fact]
    public void ChallengeGoalType_HasSixValues()
    {
        var values = Enum.GetValues<ChallengeGoalType>();
        values.Should().HaveCount(6);
    }

    #endregion

    #region ChallengeTemplate GoalType Tests

    [Fact]
    public void ChallengeTemplate_Create_WithoutGoalType_DefaultsToNull()
    {
        // Act
        var template = ChallengeTemplate.Create(
            "Test Challenge",
            "Description",
            ChallengeType.Daily,
            1000,
            100);

        // Assert
        template.GoalType.Should().BeNull();
    }

    [Fact]
    public void ChallengeTemplate_Create_WithGoalType_SetsProperty()
    {
        // Act
        var template = ChallengeTemplate.Create(
            "Score Challenge",
            "Achieve 5000 points total",
            ChallengeType.Daily,
            5000,
            150,
            ChallengeGoalType.ScoreTotal);

        // Assert
        template.GoalType.Should().Be(ChallengeGoalType.ScoreTotal);
    }

    [Theory]
    [InlineData(ChallengeGoalType.ScoreTotal)]
    [InlineData(ChallengeGoalType.ScoreSingleGame)]
    [InlineData(ChallengeGoalType.GameCount)]
    [InlineData(ChallengeGoalType.WinStreak)]
    [InlineData(ChallengeGoalType.Accuracy)]
    [InlineData(ChallengeGoalType.TimeBased)]
    public void ChallengeTemplate_Create_WithAllGoalTypes_Works(ChallengeGoalType goalType)
    {
        // Act
        var template = ChallengeTemplate.Create(
            "Test Challenge",
            "Description",
            ChallengeType.Weekly,
            100,
            50,
            goalType);

        // Assert
        template.GoalType.Should().Be(goalType);
    }

    [Fact]
    public void ChallengeTemplate_Update_WithGoalType_SetsProperty()
    {
        // Arrange
        var template = ChallengeTemplate.Create(
            "Original",
            "Original Description",
            ChallengeType.Daily,
            1000,
            100);

        // Act
        template.Update(
            "Updated Name",
            "Updated Description",
            ChallengeType.Weekly,
            2000,
            200,
            true,
            ChallengeGoalType.GameCount);

        // Assert
        template.Name.Should().Be("Updated Name");
        template.GoalType.Should().Be(ChallengeGoalType.GameCount);
    }

    [Fact]
    public void ChallengeTemplate_Update_WithNullGoalType_SetsToNull()
    {
        // Arrange
        var template = ChallengeTemplate.Create(
            "Original",
            "Original Description",
            ChallengeType.Daily,
            1000,
            100,
            ChallengeGoalType.ScoreTotal);

        // Act
        template.Update(
            "Updated Name",
            "Updated Description",
            ChallengeType.Weekly,
            2000,
            200,
            true,
            null);

        // Assert
        template.GoalType.Should().BeNull();
    }

    #endregion

    #region Challenge GoalType Tests

    [Fact]
    public void Challenge_Create_WithoutGoalType_DefaultsToNull()
    {
        // Act
        var challenge = Challenge.Create(
            "Test Challenge",
            "Description",
            ChallengeType.Daily,
            1000,
            100,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1));

        // Assert
        challenge.GoalType.Should().BeNull();
    }

    [Fact]
    public void Challenge_Create_WithGoalType_SetsProperty()
    {
        // Act
        var challenge = Challenge.Create(
            "Score Challenge",
            "Achieve 5000 points",
            ChallengeType.Daily,
            5000,
            150,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            templateId: null,
            goalType: ChallengeGoalType.ScoreSingleGame);

        // Assert
        challenge.GoalType.Should().Be(ChallengeGoalType.ScoreSingleGame);
    }

    [Fact]
    public void Challenge_CreateFromTemplate_CopiesGoalType()
    {
        // Arrange
        var template = ChallengeTemplate.Create(
            "Template Challenge",
            "Template Description",
            ChallengeType.Weekly,
            3000,
            200,
            ChallengeGoalType.WinStreak);

        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(7);

        // Act
        var challenge = Challenge.CreateFromTemplate(template, startDate, endDate);

        // Assert
        challenge.Name.Should().Be(template.Name);
        challenge.Description.Should().Be(template.Description);
        challenge.Type.Should().Be(template.Type);
        challenge.TargetValue.Should().Be(template.TargetValue);
        challenge.XPReward.Should().Be(template.XPReward);
        challenge.GoalType.Should().Be(template.GoalType);
        challenge.ChallengeTemplateId.Should().Be(template.Id);
        challenge.StartDate.Should().Be(startDate);
        challenge.EndDate.Should().Be(endDate);
    }

    [Fact]
    public void Challenge_CreateFromTemplate_WithNullGoalType_PreservesNull()
    {
        // Arrange
        var template = ChallengeTemplate.Create(
            "Template Challenge",
            "Template Description",
            ChallengeType.Daily,
            1000,
            100);
        // Note: No GoalType set, so it should be null

        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(1);

        // Act
        var challenge = Challenge.CreateFromTemplate(template, startDate, endDate);

        // Assert
        challenge.GoalType.Should().BeNull();
    }

    #endregion
}
