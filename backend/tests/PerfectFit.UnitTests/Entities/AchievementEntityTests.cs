using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.UnitTests.Entities;

public class AchievementEntityTests
{
    #region Achievement Tests

    [Fact]
    public void Achievement_Create_SetsProperties()
    {
        // Arrange
        var name = "First Win";
        var description = "Complete your first game";
        var category = AchievementCategory.Games;
        var iconUrl = "https://example.com/icons/first-win.png";
        var unlockCondition = """{"type":"games_played","value":1}""";
        var rewardType = RewardType.Cosmetic;
        var rewardValue = 10;

        // Act
        var achievement = Achievement.Create(
            name, 
            description, 
            category, 
            iconUrl, 
            unlockCondition, 
            rewardType, 
            rewardValue);

        // Assert
        achievement.Id.Should().Be(0); // Not set until persisted
        achievement.Name.Should().Be(name);
        achievement.Description.Should().Be(description);
        achievement.Category.Should().Be(category);
        achievement.IconUrl.Should().Be(iconUrl);
        achievement.UnlockCondition.Should().Be(unlockCondition);
        achievement.RewardType.Should().Be(rewardType);
        achievement.RewardValue.Should().Be(rewardValue);
        achievement.IsSecret.Should().BeFalse();
        achievement.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void Achievement_Create_WithSecretFlag_SetsIsSecret()
    {
        // Arrange & Act
        var achievement = Achievement.Create(
            "Hidden Master",
            "A secret achievement",
            AchievementCategory.Special,
            "https://example.com/icons/hidden.png",
            """{"type":"secret","value":true}""",
            RewardType.Cosmetic,
            100,
            isSecret: true,
            displayOrder: 99);

        // Assert
        achievement.IsSecret.Should().BeTrue();
        achievement.DisplayOrder.Should().Be(99);
    }

    [Fact]
    public void Achievement_Create_ThrowsWhenNameEmpty()
    {
        // Arrange & Act
        var act = () => Achievement.Create(
            "",
            "Description",
            AchievementCategory.Games,
            "https://example.com/icon.png",
            "{}",
            RewardType.Cosmetic,
            10);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Achievement_Create_ThrowsWhenDescriptionEmpty()
    {
        // Arrange & Act
        var act = () => Achievement.Create(
            "Name",
            "",
            AchievementCategory.Games,
            "https://example.com/icon.png",
            "{}",
            RewardType.Cosmetic,
            10);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("description");
    }

    #endregion

    #region UserAchievement Tests

    [Fact]
    public void UserAchievement_Create_SetsDefaults()
    {
        // Arrange
        var userId = 1;
        var achievementId = 5;

        // Act
        var userAchievement = UserAchievement.Create(userId, achievementId);

        // Assert
        userAchievement.Id.Should().Be(0); // Not set until persisted
        userAchievement.UserId.Should().Be(userId);
        userAchievement.AchievementId.Should().Be(achievementId);
        userAchievement.Progress.Should().Be(0);
        userAchievement.UnlockedAt.Should().BeNull();
    }

    [Fact]
    public void UserAchievement_UpdateProgress_SetsProgress()
    {
        // Arrange
        var userAchievement = UserAchievement.Create(1, 5);

        // Act
        userAchievement.UpdateProgress(50);

        // Assert
        userAchievement.Progress.Should().Be(50);
    }

    [Fact]
    public void UserAchievement_UpdateProgress_ClampsTo100()
    {
        // Arrange
        var userAchievement = UserAchievement.Create(1, 5);

        // Act
        userAchievement.UpdateProgress(150);

        // Assert
        userAchievement.Progress.Should().Be(100);
    }

    [Fact]
    public void UserAchievement_Unlock_SetsUnlockedAtAndProgress100()
    {
        // Arrange
        var userAchievement = UserAchievement.Create(1, 5);
        userAchievement.UpdateProgress(80);

        // Act
        userAchievement.Unlock();

        // Assert
        userAchievement.Progress.Should().Be(100);
        userAchievement.UnlockedAt.Should().NotBeNull();
        userAchievement.UnlockedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UserAchievement_Unlock_DoesNotOverwriteExistingUnlockedAt()
    {
        // Arrange
        var userAchievement = UserAchievement.Create(1, 5);
        userAchievement.Unlock();
        var firstUnlockedAt = userAchievement.UnlockedAt;

        // Wait a bit to ensure time difference
        Thread.Sleep(10);

        // Act - try to unlock again
        userAchievement.Unlock();

        // Assert
        userAchievement.UnlockedAt.Should().Be(firstUnlockedAt);
    }

    [Fact]
    public void UserAchievement_IsUnlocked_ReturnsTrueWhenUnlocked()
    {
        // Arrange
        var userAchievement = UserAchievement.Create(1, 5);
        userAchievement.Unlock();

        // Act & Assert
        userAchievement.IsUnlocked.Should().BeTrue();
    }

    [Fact]
    public void UserAchievement_IsUnlocked_ReturnsFalseWhenNotUnlocked()
    {
        // Arrange
        var userAchievement = UserAchievement.Create(1, 5);

        // Act & Assert
        userAchievement.IsUnlocked.Should().BeFalse();
    }

    #endregion
}
