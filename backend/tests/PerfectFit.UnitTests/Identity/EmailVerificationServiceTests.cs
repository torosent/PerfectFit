using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Identity;

namespace PerfectFit.UnitTests.Identity;

public class EmailVerificationServiceTests
{
    private readonly IEmailVerificationService _service;

    public EmailVerificationServiceTests()
    {
        _service = new EmailVerificationService();
    }

    [Fact]
    public void GenerateVerificationToken_ReturnsNonEmptyToken()
    {
        // Act
        var token = _service.GenerateVerificationToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateVerificationToken_ReturnsDifferentTokensEachCall()
    {
        // Act
        var token1 = _service.GenerateVerificationToken();
        var token2 = _service.GenerateVerificationToken();

        // Assert
        token1.Should().NotBe(token2, "because tokens should be randomly generated");
    }

    [Fact]
    public void GenerateVerificationToken_ReturnsBase64UrlSafeToken()
    {
        // Act
        var token = _service.GenerateVerificationToken();

        // Assert
        // Base64Url-safe tokens should not contain '+', '/', or '=' characters
        token.Should().NotContain("+");
        token.Should().NotContain("/");
        token.Should().NotContain("=");
        // Should only contain alphanumeric, '-', and '_'
        token.Should().MatchRegex("^[A-Za-z0-9_-]+$");
    }

    [Fact]
    public void SetVerificationToken_SetsTokenAndExpiry()
    {
        // Arrange
        var user = User.Create("test-external-id", "test@example.com", "Test User", AuthProvider.Local);

        // Act
        _service.SetVerificationToken(user);

        // Assert
        user.EmailVerificationToken.Should().NotBeNullOrEmpty();
        user.EmailVerificationTokenExpiry.Should().NotBeNull();
    }

    [Fact]
    public void SetVerificationToken_SetsExpiryTo24Hours()
    {
        // Arrange
        var user = User.Create("test-external-id", "test@example.com", "Test User", AuthProvider.Local);
        var beforeSet = DateTime.UtcNow;

        // Act
        _service.SetVerificationToken(user);

        // Assert
        var afterSet = DateTime.UtcNow;
        var expectedExpiryMin = beforeSet.AddHours(24);
        var expectedExpiryMax = afterSet.AddHours(24);

        user.EmailVerificationTokenExpiry.Should().BeOnOrAfter(expectedExpiryMin);
        user.EmailVerificationTokenExpiry.Should().BeOnOrBefore(expectedExpiryMax);
    }

    [Fact]
    public void IsTokenValid_ReturnsTrueForValidToken()
    {
        // Arrange
        var user = User.Create("test-external-id", "test@example.com", "Test User", AuthProvider.Local);
        _service.SetVerificationToken(user);
        var token = user.EmailVerificationToken!;

        // Act
        var result = _service.IsTokenValid(user, token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTokenValid_ReturnsFalseForWrongToken()
    {
        // Arrange
        var user = User.Create("test-external-id", "test@example.com", "Test User", AuthProvider.Local);
        _service.SetVerificationToken(user);

        // Act
        var result = _service.IsTokenValid(user, "wrong-token");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTokenValid_ReturnsFalseForExpiredToken()
    {
        // Arrange
        var user = User.Create("test-external-id", "test@example.com", "Test User", AuthProvider.Local);
        _service.SetVerificationToken(user);
        var token = user.EmailVerificationToken!;
        
        // Set expiry to the past
        user.SetEmailVerificationTokenExpiry(DateTime.UtcNow.AddHours(-1));

        // Act
        var result = _service.IsTokenValid(user, token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTokenValid_ReturnsFalseForNullToken()
    {
        // Arrange
        var user = User.Create("test-external-id", "test@example.com", "Test User", AuthProvider.Local);
        // Don't set any verification token - it should be null

        // Act
        var result = _service.IsTokenValid(user, "any-token");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkEmailVerified_SetsEmailVerifiedTrue()
    {
        // Arrange
        var user = User.Create("test-external-id", "test@example.com", "Test User", AuthProvider.Local);
        _service.SetVerificationToken(user);

        // Act
        _service.MarkEmailVerified(user);

        // Assert
        user.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public void MarkEmailVerified_ClearsToken()
    {
        // Arrange
        var user = User.Create("test-external-id", "test@example.com", "Test User", AuthProvider.Local);
        _service.SetVerificationToken(user);

        // Act
        _service.MarkEmailVerified(user);

        // Assert
        user.EmailVerificationToken.Should().BeNull();
        user.EmailVerificationTokenExpiry.Should().BeNull();
    }
}
