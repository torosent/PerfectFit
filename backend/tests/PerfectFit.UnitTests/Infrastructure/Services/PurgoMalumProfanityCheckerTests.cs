using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;
using PerfectFit.Infrastructure.Services;

namespace PerfectFit.UnitTests.Infrastructure.Services;

public class PurgoMalumProfanityCheckerTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly PurgoMalumProfanityChecker _sut;

    public PurgoMalumProfanityCheckerTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _sut = new PurgoMalumProfanityChecker(_httpClient);
    }

    [Fact]
    public async Task ContainsProfanityAsync_ReturnsTrue_WhenApiReturnsTrue()
    {
        // Arrange
        SetupHttpResponse("true");

        // Act
        var result = await _sut.ContainsProfanityAsync("badword");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ContainsProfanityAsync_ReturnsFalse_WhenApiReturnsFalse()
    {
        // Arrange
        SetupHttpResponse("false");

        // Act
        var result = await _sut.ContainsProfanityAsync("goodword");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("TRUE")]
    [InlineData("True")]
    [InlineData("  true  ")]
    public async Task ContainsProfanityAsync_HandlesVariousTrueFormats(string response)
    {
        // Arrange
        SetupHttpResponse(response);

        // Act
        var result = await _sut.ContainsProfanityAsync("test");

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("FALSE")]
    [InlineData("False")]
    [InlineData("  false  ")]
    public async Task ContainsProfanityAsync_HandlesVariousFalseFormats(string response)
    {
        // Arrange
        SetupHttpResponse(response);

        // Act
        var result = await _sut.ContainsProfanityAsync("test");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("error")]
    [InlineData("Invalid input")]
    [InlineData("<error>Something went wrong</error>")]
    [InlineData("")]
    public async Task ContainsProfanityAsync_ReturnsNull_WhenApiReturnsUnexpectedResponse(string response)
    {
        // Arrange
        SetupHttpResponse(response);

        // Act
        var result = await _sut.ContainsProfanityAsync("test");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ContainsProfanityAsync_ReturnsNull_WhenHttpRequestFails()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _sut.ContainsProfanityAsync("test");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ContainsProfanityAsync_ReturnsNull_WhenRequestTimesOut()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timed out"));

        // Act
        var result = await _sut.ContainsProfanityAsync("test");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ContainsProfanityAsync_ReturnsFalse_WhenTextIsEmpty()
    {
        // Act
        var result = await _sut.ContainsProfanityAsync("");

        // Assert
        result.Should().BeFalse();

        // Should not make HTTP call for empty text
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task ContainsProfanityAsync_ReturnsFalse_WhenTextIsNull()
    {
        // Act
        var result = await _sut.ContainsProfanityAsync(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ContainsProfanityAsync_UrlEncodesText()
    {
        // Arrange
        SetupHttpResponse("false");
        string? capturedUrl = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedUrl = req.RequestUri?.ToString())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("false")
            });

        // Act
        await _sut.ContainsProfanityAsync("test with spaces");

        // Assert
        capturedUrl.Should().Contain("test+with+spaces");
    }

    private void SetupHttpResponse(string content)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            });
    }
}
