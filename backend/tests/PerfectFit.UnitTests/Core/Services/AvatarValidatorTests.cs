using FluentAssertions;
using PerfectFit.Core.Services;

namespace PerfectFit.UnitTests.Core.Services;

public class AvatarValidatorTests
{
    [Fact]
    public void IsValidAvatar_ReturnsTrue_WhenNull()
    {
        // Act
        var result = AvatarValidator.IsValidAvatar(null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidAvatar_ReturnsTrue_WhenEmpty()
    {
        // Act
        var result = AvatarValidator.IsValidAvatar(string.Empty);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("😀")]
    [InlineData("🎮")]
    [InlineData("🐶")]
    [InlineData("🚀")]
    [InlineData("🦊")]
    [InlineData("☕")]
    [InlineData("🕹️")]
    [InlineData("☀️")]
    [InlineData("✈️")]
    public void IsValidAvatar_ReturnsTrue_WhenValidEmoji(string avatar)
    {
        // Act
        var result = AvatarValidator.IsValidAvatar(avatar);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("X")]
    [InlineData("invalid")]
    [InlineData("123")]
    [InlineData("💩")] // Not in curated list
    [InlineData("🤬")] // Not in curated list
    public void IsValidAvatar_ReturnsFalse_WhenInvalidString(string avatar)
    {
        // Act
        var result = AvatarValidator.IsValidAvatar(avatar);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("random text")]
    [InlineData("hello world")]
    [InlineData("test123")]
    [InlineData("   ")]
    public void IsValidAvatar_ReturnsFalse_WhenRandomText(string avatar)
    {
        // Act
        var result = AvatarValidator.IsValidAvatar(avatar);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidAvatars_ContainsExpectedCount()
    {
        // The frontend has 90 emojis in the curated list
        AvatarValidator.ValidAvatars.Count.Should().Be(90);
    }
}
