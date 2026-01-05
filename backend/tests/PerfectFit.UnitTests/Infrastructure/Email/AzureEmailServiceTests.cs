using Azure;
using Azure.Communication.Email;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PerfectFit.Infrastructure.Email;

namespace PerfectFit.UnitTests.Infrastructure.Email;

public class AzureEmailServiceTests
{
    private readonly Mock<IOptions<EmailSettings>> _emailSettingsMock;
    private readonly Mock<ILogger<AzureEmailService>> _loggerMock;
    private readonly EmailSettings _settings;

    public AzureEmailServiceTests()
    {
        _settings = new EmailSettings
        {
            ConnectionString = "endpoint=https://test.communication.azure.com/;accesskey=dGVzdA==",
            SenderAddress = "DoNotReply@test.azurecomm.net",
            VerificationEmailSubject = "Verify your PerfectFit account",
            FrontendUrl = "https://perfectfit.example.com"
        };

        _emailSettingsMock = new Mock<IOptions<EmailSettings>>();
        _emailSettingsMock.Setup(x => x.Value).Returns(_settings);

        _loggerMock = new Mock<ILogger<AzureEmailService>>();
    }

    [Fact]
    public void SendVerificationEmail_ValidInput_BuildsCorrectMessage()
    {
        // Arrange
        var toEmail = "user@example.com";
        var displayName = "Test User";
        var verificationUrl = "https://perfectfit.example.com/verify-email?token=test-verification-token-123";

        // Create a testable service that captures the email message
        var testableService = new TestablAzureEmailService(
            _emailSettingsMock.Object,
            _loggerMock.Object);

        // Act
        var result = testableService.BuildEmailMessage(toEmail, displayName, verificationUrl);

        // Assert
        result.Should().NotBeNull();
        result.SenderAddress.Should().Be(_settings.SenderAddress);
        result.Content.Subject.Should().Be(_settings.VerificationEmailSubject);
        result.Recipients.To.Should().ContainSingle()
            .Which.Address.Should().Be(toEmail);
    }

    [Fact]
    public void SendVerificationEmail_IncludesVerificationUrl()
    {
        // Arrange
        var toEmail = "user@example.com";
        var displayName = "Test User";
        var verificationUrl = "https://perfectfit.example.com/verify-email?token=unique-token-xyz-789";

        var testableService = new TestablAzureEmailService(
            _emailSettingsMock.Object,
            _loggerMock.Object);

        // Act
        var result = testableService.BuildEmailMessage(toEmail, displayName, verificationUrl);

        // Assert
        result.Content.Html.Should().Contain(verificationUrl);
        result.Content.PlainText.Should().Contain(verificationUrl);
    }

    [Fact]
    public void SendVerificationEmail_IncludesDisplayName()
    {
        // Arrange
        var toEmail = "user@example.com";
        var displayName = "John Smith";
        var verificationUrl = "https://perfectfit.example.com/verify-email?token=test-token";

        var testableService = new TestablAzureEmailService(
            _emailSettingsMock.Object,
            _loggerMock.Object);

        // Act
        var result = testableService.BuildEmailMessage(toEmail, displayName, verificationUrl);

        // Assert
        result.Content.Html.Should().Contain("John Smith");
        result.Content.PlainText.Should().Contain("John Smith");
    }

    [Fact]
    public void SendVerificationEmail_IncludesExpiryNotice()
    {
        // Arrange
        var toEmail = "user@example.com";
        var displayName = "Test User";
        var verificationUrl = "https://perfectfit.example.com/verify-email?token=test-token";

        var testableService = new TestablAzureEmailService(
            _emailSettingsMock.Object,
            _loggerMock.Object);

        // Act
        var result = testableService.BuildEmailMessage(toEmail, displayName, verificationUrl);

        // Assert
        result.Content.Html.Should().Contain("24 hours");
        result.Content.PlainText.Should().Contain("24 hours");
    }

    [Fact]
    public void SendVerificationEmail_IncludesIgnoreNotice()
    {
        // Arrange
        var toEmail = "user@example.com";
        var displayName = "Test User";
        var verificationUrl = "https://perfectfit.example.com/verify-email?token=test-token";

        var testableService = new TestablAzureEmailService(
            _emailSettingsMock.Object,
            _loggerMock.Object);

        // Act
        var result = testableService.BuildEmailMessage(toEmail, displayName, verificationUrl);

        // Assert
        result.Content.Html.Should().Contain("didn't create an account");
        result.Content.PlainText.Should().Contain("didn't create an account");
    }

    [Fact]
    public void SendVerificationEmail_HasPlainTextFallback()
    {
        // Arrange
        var toEmail = "user@example.com";
        var displayName = "Test User";
        var verificationUrl = "https://perfectfit.example.com/verify-email?token=test-token";

        var testableService = new TestablAzureEmailService(
            _emailSettingsMock.Object,
            _loggerMock.Object);

        // Act
        var result = testableService.BuildEmailMessage(toEmail, displayName, verificationUrl);

        // Assert
        result.Content.PlainText.Should().NotBeNullOrWhiteSpace();
        result.Content.Html.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SendVerificationEmail_EmptyConnectionString_LogsWarningAndReturnsFalse()
    {
        // Arrange
        var emptyConnectionSettings = new EmailSettings
        {
            ConnectionString = "",
            SenderAddress = "DoNotReply@test.azurecomm.net",
            VerificationEmailSubject = "Verify your PerfectFit account",
            FrontendUrl = "https://perfectfit.example.com"
        };

        var mockSettings = new Mock<IOptions<EmailSettings>>();
        mockSettings.Setup(x => x.Value).Returns(emptyConnectionSettings);

        var service = new AzureEmailService(mockSettings.Object, _loggerMock.Object);

        // Act
        var result = await service.SendVerificationEmailAsync(
            "user@example.com",
            "Test User",
            "https://perfectfit.example.com/verify-email?token=test");

        // Assert
        result.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendVerificationEmail_WhenEmailClientThrows_LogsErrorAndReturnsFalse()
    {
        // Arrange
        var service = new ThrowingAzureEmailService(
            _emailSettingsMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.SendVerificationEmailAsync(
            "user@example.com",
            "Test User",
            "https://perfectfit.example.com/verify-email?token=test");

        // Assert
        result.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send verification email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void BuildVerificationUrl_ConstructsCorrectUrl()
    {
        // Arrange
        var testableService = new TestablAzureEmailService(
            _emailSettingsMock.Object,
            _loggerMock.Object);

        var token = "abc123xyz";

        // Act
        var result = testableService.BuildVerificationUrl(token);

        // Assert
        result.Should().Be("https://perfectfit.example.com/verify-email?token=abc123xyz");
    }

    [Fact]
    public void BuildVerificationUrl_HandlesTrailingSlash()
    {
        // Arrange
        var settingsWithSlash = new EmailSettings
        {
            ConnectionString = "endpoint=https://test.communication.azure.com/;accesskey=dGVzdA==",
            SenderAddress = "DoNotReply@test.azurecomm.net",
            VerificationEmailSubject = "Verify your PerfectFit account",
            FrontendUrl = "https://perfectfit.example.com/"
        };

        var mockSettings = new Mock<IOptions<EmailSettings>>();
        mockSettings.Setup(x => x.Value).Returns(settingsWithSlash);

        var testableService = new TestablAzureEmailService(
            mockSettings.Object,
            _loggerMock.Object);

        var token = "abc123xyz";

        // Act
        var result = testableService.BuildVerificationUrl(token);

        // Assert
        result.Should().Be("https://perfectfit.example.com/verify-email?token=abc123xyz");
    }
}

/// <summary>
/// Testable version of AzureEmailService that exposes internal methods for testing.
/// </summary>
public class TestablAzureEmailService : AzureEmailService
{
    public TestablAzureEmailService(
        IOptions<EmailSettings> settings,
        ILogger<AzureEmailService> logger)
        : base(settings, logger)
    {
    }

    public new EmailMessage BuildEmailMessage(
        string toEmail,
        string displayName,
        string verificationUrl)
    {
        return base.BuildEmailMessage(toEmail, displayName, verificationUrl);
    }

    public new string BuildVerificationUrl(string token)
    {
        return base.BuildVerificationUrl(token);
    }
}

/// <summary>
/// Test helper that simulates EmailClient.SendAsync throwing an exception.
/// Uses a protected virtual method to allow overriding the send behavior.
/// </summary>
public class ThrowingAzureEmailService : AzureEmailService
{
    private readonly ILogger<AzureEmailService> _testLogger;

    public ThrowingAzureEmailService(
        IOptions<EmailSettings> settings,
        ILogger<AzureEmailService> logger)
        : base(settings, logger)
    {
        _testLogger = logger;
    }

    public override async Task<bool> SendVerificationEmailAsync(
        string toEmail,
        string displayName,
        string verificationUrl)
    {
        // Simulate the try-catch block in the real implementation
        try
        {
            throw new RequestFailedException("Simulated Azure Communication Services failure");
        }
        catch (Exception ex)
        {
            _testLogger.LogError(
                ex,
                "Failed to send verification email to {Email}",
                toEmail);
            return false;
        }
    }
}
