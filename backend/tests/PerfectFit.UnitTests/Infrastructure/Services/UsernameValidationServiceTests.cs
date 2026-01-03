using FluentAssertions;
using Moq;
using PerfectFit.Core.Interfaces;
using PerfectFit.Infrastructure.Services;

namespace PerfectFit.UnitTests.Infrastructure.Services;

public class UsernameValidationServiceTests
{
    private readonly Mock<IProfanityChecker> _profanityCheckerMock;
    private readonly UsernameValidationService _sut;

    public UsernameValidationServiceTests()
    {
        _profanityCheckerMock = new Mock<IProfanityChecker>();
        _sut = new UsernameValidationService(_profanityCheckerMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSuccess_WhenUsernameIsClean()
    {
        // Arrange
        var username = "Player_123";
        _profanityCheckerMock
            .Setup(x => x.ContainsProfanityAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ValidateAsync(username);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.SuggestedUsername.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsError_WhenUsernameContainsProfanity()
    {
        // Arrange
        var username = "badword123";
        _profanityCheckerMock
            .Setup(x => x.ContainsProfanityAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ValidateAsync(username);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("inappropriate");
        result.SuggestedUsername.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSuggestion_WhenApiCallFails()
    {
        // Arrange
        var username = "Player_123";
        _profanityCheckerMock
            .Setup(x => x.ContainsProfanityAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((bool?)null); // API returned unexpected response

        // Act
        var result = await _sut.ValidateAsync(username);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Unable to validate");
        result.SuggestedUsername.Should().NotBeNullOrEmpty();
        result.SuggestedUsername.Should().StartWith("Player_");
    }

    [Theory]
    [InlineData("ab")] // too short
    [InlineData("user@name")] // invalid chars
    [InlineData("")] // empty
    public async Task ValidateAsync_ReturnsFormatError_BeforeCheckingProfanity(string username)
    {
        // Act
        var result = await _sut.ValidateAsync(username);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();

        // Profanity checker should NOT be called for invalid format
        _profanityCheckerMock.Verify(
            x => x.ContainsProfanityAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_PassesCancellationToken_ToProfanityChecker()
    {
        // Arrange
        var username = "Player_123";
        var cts = new CancellationTokenSource();
        _profanityCheckerMock
            .Setup(x => x.ContainsProfanityAsync(username, cts.Token))
            .ReturnsAsync(false);

        // Act
        await _sut.ValidateAsync(username, cts.Token);

        // Assert
        _profanityCheckerMock.Verify(
            x => x.ContainsProfanityAsync(username, cts.Token),
            Times.Once);
    }
}
