using FluentAssertions;
using PerfectFit.Core.Identity;

namespace PerfectFit.UnitTests.Identity;

public class BCryptPasswordHasherTests
{
    private readonly IPasswordHasher _passwordHasher;

    public BCryptPasswordHasherTests()
    {
        _passwordHasher = new BCryptPasswordHasher();
    }

    [Fact]
    public void HashPassword_ReturnsNonEmptyHash()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_ReturnsDifferentHashesForSamePassword()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2, "because BCrypt uses random salts");
    }

    [Fact]
    public void VerifyPassword_ReturnsTrueForCorrectPassword()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForIncorrectPassword()
    {
        // Arrange
        var password = "SecurePassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyPassword_ReturnsFalseForNullOrEmptyHash(string? hash)
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForInvalidHashFormat()
    {
        var result = _passwordHasher.VerifyPassword("password", "not_a_valid_bcrypt_hash");
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HashPassword_ThrowsForNullOrEmptyPassword(string? password)
    {
        // Act
        var act = () => _passwordHasher.HashPassword(password!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
