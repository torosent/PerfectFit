using FluentAssertions;
using PerfectFit.Core.Services;

namespace PerfectFit.UnitTests.Core.Services;

public class UsernameValidatorTests
{
    [Theory]
    [InlineData("Player_123")]
    [InlineData("abc")]
    [InlineData("John_Doe")]
    [InlineData("user1234567890123456")] // 20 chars - max valid
    [InlineData("A_B_C")]
    [InlineData("___")]
    public void IsValidFormat_ReturnsTrue_WhenUsernameValid(string username)
    {
        // Act
        var result = UsernameValidator.IsValidFormat(username);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("ab")]  // 2 chars - too short
    [InlineData("a")]   // 1 char - too short
    [InlineData("")]    // empty
    public void IsValidFormat_ReturnsFalse_WhenUsernameTooShort(string username)
    {
        // Act
        var result = UsernameValidator.IsValidFormat(username);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidFormat_ReturnsFalse_WhenUsernameTooLong()
    {
        // Arrange
        var username = new string('a', 21); // 21 chars - too long

        // Act
        var result = UsernameValidator.IsValidFormat(username);

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
    public void IsValidFormat_ReturnsFalse_WhenUsernameHasInvalidChars(string username)
    {
        // Act
        var result = UsernameValidator.IsValidFormat(username);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateFormat_ReturnsSuccess_WhenUsernameIsValid()
    {
        // Arrange
        var username = "Player_123";

        // Act
        var result = UsernameValidator.ValidateFormat(username);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ValidateFormat_ReturnsError_WhenUsernameTooShort()
    {
        // Arrange
        var username = "ab";

        // Act
        var result = UsernameValidator.ValidateFormat(username);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("3");
        result.ErrorMessage.Should().Contain("20");
    }

    [Fact]
    public void ValidateFormat_ReturnsError_WhenUsernameHasInvalidChars()
    {
        // Arrange
        var username = "user@name";

        // Act
        var result = UsernameValidator.ValidateFormat(username);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("alphanumeric");
    }

    [Fact]
    public void ValidateFormat_ReturnsError_WhenUsernameIsNull()
    {
        // Act
        var result = UsernameValidator.ValidateFormat(null!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRandomUsername_ReturnsValidFormat()
    {
        // Act
        var username = UsernameValidator.GenerateRandomUsername();

        // Assert
        username.Should().StartWith("Player_");
        username.Should().HaveLength(13); // "Player_" (7) + 6 random chars
        username.Should().MatchRegex(@"^Player_[A-Z0-9]{6}$");
    }

    [Fact]
    public void GenerateRandomUsername_ReturnsDifferentUsernames()
    {
        // Act
        var username1 = UsernameValidator.GenerateRandomUsername();
        var username2 = UsernameValidator.GenerateRandomUsername();

        // Assert - statistically very unlikely to be equal
        username1.Should().NotBe(username2);
    }

    [Fact]
    public void UsernameValidationResult_FailureWithSuggestion_ContainsSuggestedUsername()
    {
        // Arrange
        var errorMessage = "Test error";
        var suggestedUsername = "Player_ABC123";

        // Act
        var result = UsernameValidationResult.FailureWithSuggestion(errorMessage, suggestedUsername);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.SuggestedUsername.Should().Be(suggestedUsername);
    }
}
