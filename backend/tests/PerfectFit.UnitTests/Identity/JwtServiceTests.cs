using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Infrastructure.Identity;
using System.Security.Claims;

namespace PerfectFit.UnitTests.Identity;

public class JwtServiceTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtService _sut;

    public JwtServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-super-secret-key-that-is-at-least-32-characters-long",
            Issuer = "PerfectFit",
            Audience = "PerfectFit",
            ExpirationDays = 7
        };

        var options = Options.Create(_jwtSettings);
        _sut = new JwtService(options);
    }

    [Fact]
    public void GenerateToken_Should_Return_Valid_Token_For_User()
    {
        // Arrange
        var user = User.Create("google-123", "test@example.com", "Test User", AuthProvider.Google);

        // Act
        var token = _sut.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts separated by dots
    }

    [Fact]
    public void GenerateToken_Should_Include_User_Claims()
    {
        // Arrange
        var user = User.Create("google-123", "test@example.com", "Test User", AuthProvider.Google);

        // Act
        var token = _sut.GenerateToken(user);
        var principal = _sut.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(user.Id.ToString());
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be("test@example.com");
        principal.FindFirst(ClaimTypes.Name)?.Value.Should().Be("Test User");
        principal.FindFirst("provider")?.Value.Should().Be("Google");
    }

    [Fact]
    public void GenerateToken_Should_Work_Without_Email()
    {
        // Arrange
        var user = User.Create("guest-123", null, "Guest User", AuthProvider.Guest);

        // Act
        var token = _sut.GenerateToken(user);
        var principal = _sut.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.Email).Should().BeNull();
        principal.FindFirst(ClaimTypes.Name)?.Value.Should().Be("Guest User");
    }

    [Fact]
    public void ValidateToken_Should_Return_Principal_For_Valid_Token()
    {
        // Arrange
        var user = User.Create("microsoft-456", "user@outlook.com", "MS User", AuthProvider.Microsoft);
        var token = _sut.GenerateToken(user);

        // Act
        var principal = _sut.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_Should_Return_Null_For_Invalid_Token()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _sut.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_Should_Return_Null_For_Tampered_Token()
    {
        // Arrange
        var user = User.Create("google-789", "tamper@test.com", "Tamper User", AuthProvider.Google);
        var token = _sut.GenerateToken(user);
        var tamperedToken = token[..^5] + "xxxxx"; // Tamper with the signature

        // Act
        var principal = _sut.ValidateToken(tamperedToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_Should_Return_Null_For_Token_With_Different_Secret()
    {
        // Arrange
        var differentSettings = new JwtSettings
        {
            Secret = "this-is-a-different-secret-key-that-is-also-32-chars-plus",
            Issuer = "PerfectFit",
            Audience = "PerfectFit",
            ExpirationDays = 7
        };
        var differentService = new JwtService(Options.Create(differentSettings));

        var user = User.Create("google-999", "other@test.com", "Other User", AuthProvider.Google);
        var tokenFromDifferentService = differentService.GenerateToken(user);

        // Act
        var principal = _sut.ValidateToken(tokenFromDifferentService);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GenerateToken_Should_Include_External_Id_Claim()
    {
        // Arrange
        var externalId = "apple-unique-id-12345";
        var user = User.Create(externalId, "apple@icloud.com", "Apple User", AuthProvider.Apple);

        // Act
        var token = _sut.GenerateToken(user);
        var principal = _sut.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst("external_id")?.Value.Should().Be(externalId);
    }

    [Fact]
    public void GenerateToken_WithAdminRole_ShouldIncludeRoleClaim()
    {
        // Arrange
        var user = User.Create("admin-123", "admin@example.com", "Admin User", AuthProvider.Google, UserRole.Admin);

        // Act
        var token = _sut.GenerateToken(user);
        var principal = _sut.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        var roleClaim = principal!.FindFirst(ClaimTypes.Role);
        roleClaim.Should().NotBeNull("because the token should include a role claim");
        roleClaim!.Value.Should().Be("Admin");
    }

    [Fact]
    public void GenerateToken_WithUserRole_ShouldIncludeUserRoleClaim()
    {
        // Arrange
        var user = User.Create("user-456", "user@example.com", "Regular User", AuthProvider.Google, UserRole.User);

        // Act
        var token = _sut.GenerateToken(user);
        var principal = _sut.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        var roleClaim = principal!.FindFirst(ClaimTypes.Role);
        roleClaim.Should().NotBeNull("because the token should include a role claim");
        roleClaim!.Value.Should().Be("User");
    }
}
