using FluentAssertions;
using Moq;
using PerfectFit.Core.Interfaces;
using PerfectFit.Infrastructure.Services;

namespace PerfectFit.UnitTests.Infrastructure.Services;

public class DisplayNameValidationServiceTests
{
    private readonly Mock<IProfanityChecker> _profanityCheckerMock;
    private readonly DisplayNameValidationService _sut;

    public DisplayNameValidationServiceTests()
    {
        _profanityCheckerMock = new Mock<IProfanityChecker>();
        _sut = new DisplayNameValidationService(_profanityCheckerMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSuccess_WhenDisplayNameIsClean()
    {
        // Arrange
        var displayName = "Player_123";
        _profanityCheckerMock
            .Setup(x => x.ContainsProfanityAsync(displayName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ValidateAsync(displayName);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.SuggestedDisplayName.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsError_WhenDisplayNameContainsProfanity()
    {
        // Arrange
        var displayName = "badword123";
        _profanityCheckerMock
            .Setup(x => x.ContainsProfanityAsync(displayName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ValidateAsync(displayName);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("inappropriate");
        result.SuggestedDisplayName.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSuggestion_WhenApiCallFails()
    {
        // Arrange
        var displayName = "Player_123";
        _profanityCheckerMock
            .Setup(x => x.ContainsProfanityAsync(displayName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((bool?)null); // API returned unexpected response

        // Act
        var result = await _sut.ValidateAsync(displayName);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Unable to validate");
        result.SuggestedDisplayName.Should().NotBeNullOrEmpty();
        result.SuggestedDisplayName.Should().StartWith("Player_");
    }

    [Theory]
    [InlineData("ab")] // too short
    [InlineData("user@name")] // invalid chars
    [InlineData("")] // empty
    public async Task ValidateAsync_ReturnsFormatError_BeforeCheckingProfanity(string displayName)
    {
        // Act
        var result = await _sut.ValidateAsync(displayName);

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
        var displayName = "Player_123";
        var cts = new CancellationTokenSource();
        _profanityCheckerMock
            .Setup(x => x.ContainsProfanityAsync(displayName, cts.Token))
            .ReturnsAsync(false);

        // Act
        await _sut.ValidateAsync(displayName, cts.Token);

        // Assert
        _profanityCheckerMock.Verify(
            x => x.ContainsProfanityAsync(displayName, cts.Token),
            Times.Once);
    }
}
