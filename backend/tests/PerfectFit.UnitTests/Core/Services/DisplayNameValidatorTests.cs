using FluentAssertions;
using PerfectFit.Core.Services;

namespace PerfectFit.UnitTests.Core.Services;

public class DisplayNameValidatorTests
{
    [Theory]
    [InlineData("Player_123")]
    [InlineData("abc")]
    [InlineData("John_Doe")]
    [InlineData("user1234567890123456")] // 20 chars - max valid
    [InlineData("A_B_C")]
    [InlineData("___")]
    public void IsValidFormat_ReturnsTrue_WhenDisplayNameValid(string displayName)
    {
        // Act
        var result = DisplayNameValidator.IsValidFormat(displayName);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("ab")]  // 2 chars - too short
    [InlineData("a")]   // 1 char - too short
    [InlineData("")]    // empty
    public void IsValidFormat_ReturnsFalse_WhenDisplayNameTooShort(string displayName)
    {
        // Act
        var result = DisplayNameValidator.IsValidFormat(displayName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidFormat_ReturnsFalse_WhenDisplayNameTooLong()
    {
        // Arrange
        var displayName = new string('a', 21); // 21 chars - too long

        // Act
        var result = DisplayNameValidator.IsValidFormat(displayName);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("user name")]   // space
    [InlineData("user-name")]   // hyphen
    [InlineData("user@name")]   // at symbol
    [InlineData("user.name")]   // dot
    [InlineData("user!name")]   // exclamation
    [InlineData("用户名")]       // non-ASCII
    [InlineData("user#123")]    // hash
    public void IsValidFormat_ReturnsFalse_WhenDisplayNameHasInvalidChars(string displayName)
    {
        // Act
        var result = DisplayNameValidator.IsValidFormat(displayName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateFormat_ReturnsSuccess_WhenDisplayNameIsValid()
    {
        // Arrange
        var displayName = "Player_123";

        // Act
        var result = DisplayNameValidator.ValidateFormat(displayName);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ValidateFormat_ReturnsError_WhenDisplayNameTooShort()
    {
        // Arrange
        var displayName = "ab";

        // Act
        var result = DisplayNameValidator.ValidateFormat(displayName);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("3");
        result.ErrorMessage.Should().Contain("20");
    }

    [Fact]
    public void ValidateFormat_ReturnsError_WhenDisplayNameHasInvalidChars()
    {
        // Arrange
        var displayName = "user@name";

        // Act
        var result = DisplayNameValidator.ValidateFormat(displayName);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("alphanumeric");
    }

    [Fact]
    public void ValidateFormat_ReturnsError_WhenDisplayNameIsNull()
    {
        // Act
        var result = DisplayNameValidator.ValidateFormat(null!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRandomDisplayName_ReturnsValidFormat()
    {
        // Act
        var displayName = DisplayNameValidator.GenerateRandomDisplayName();

        // Assert
        displayName.Should().StartWith("Player_");
        displayName.Should().HaveLength(13); // "Player_" (7) + 6 random chars
        displayName.Should().MatchRegex(@"^Player_[A-Z0-9]{6}$");
    }

    [Fact]
    public void GenerateRandomDisplayName_ReturnsDifferentDisplayNames()
    {
        // Act
        var displayName1 = DisplayNameValidator.GenerateRandomDisplayName();
        var displayName2 = DisplayNameValidator.GenerateRandomDisplayName();

        // Assert - statistically very unlikely to be equal
        displayName1.Should().NotBe(displayName2);
    }

    [Fact]
    public void DisplayNameValidationResult_FailureWithSuggestion_ContainsSuggestedDisplayName()
    {
        // Arrange
        var errorMessage = "Test error";
        var suggestedDisplayName = "Player_ABC123";

        // Act
        var result = DisplayNameValidationResult.FailureWithSuggestion(errorMessage, suggestedDisplayName);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.SuggestedDisplayName.Should().Be(suggestedDisplayName);
    }
}
