using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Options;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Auth.Commands;

namespace PerfectFit.UnitTests.Auth;

public class OAuthLoginCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly OAuthLoginCommandHandler _sut;

    public OAuthLoginCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _sut = new OAuthLoginCommandHandler(_userRepositoryMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_New_User_When_Not_Exists()
    {
        // Arrange
        var command = new OAuthLoginCommand(
            ExternalId: "google-new-user-123",
            Email: "newuser@gmail.com",
            DisplayName: "New User",
            Provider: AuthProvider.Google
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .ReturnsAsync((User user, CancellationToken _) => user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("test-jwt-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test-jwt-token");
        result.User.Should().NotBeNull();
        result.User.ExternalId.Should().Be(command.ExternalId);
        result.User.Email.Should().Be(command.Email);
        result.User.DisplayName.Should().Be(command.DisplayName);
        result.User.Provider.Should().Be(command.Provider);

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Existing_User_When_Found()
    {
        // Arrange
        var existingUser = User.Create("google-existing-123", "existing@gmail.com", "Existing User", AuthProvider.Google);

        var command = new OAuthLoginCommand(
            ExternalId: "google-existing-123",
            Email: "existing@gmail.com",
            DisplayName: "Existing User",
            Provider: AuthProvider.Google
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(existingUser))
            .Returns("existing-user-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("existing-user-token");
        result.User.ExternalId.Should().Be(existingUser.ExternalId);

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(x => x.UpdateAsync(existingUser, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Update_LastLogin_For_Existing_User()
    {
        // Arrange
        var existingUser = User.Create("microsoft-123", "user@outlook.com", "MS User", AuthProvider.Microsoft);

        var command = new OAuthLoginCommand(
            ExternalId: "microsoft-123",
            Email: "user@outlook.com",
            DisplayName: "MS User",
            Provider: AuthProvider.Microsoft
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("token");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<User>(u => u.LastLoginAt != null),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Work_With_Null_Email()
    {
        // Arrange
        var command = new OAuthLoginCommand(
            ExternalId: "apple-123",
            Email: null,
            DisplayName: "Apple User",
            Provider: AuthProvider.Apple
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("apple-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Email.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Generate_Token_With_User()
    {
        // Arrange
        var command = new OAuthLoginCommand(
            ExternalId: "google-token-test",
            Email: "token@test.com",
            DisplayName: "Token Test",
            Provider: AuthProvider.Google
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("generated-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        _jwtServiceMock.Verify(x => x.GenerateToken(It.Is<User>(u =>
            u.ExternalId == command.ExternalId &&
            u.Provider == command.Provider)),
            Times.Once);
    }

    [Theory]
    [InlineData(AuthProvider.Google)]
    [InlineData(AuthProvider.Apple)]
    [InlineData(AuthProvider.Microsoft)]
    [InlineData(AuthProvider.Guest)]
    public async Task Handle_Should_Support_All_Providers(AuthProvider provider)
    {
        // Arrange
        var command = new OAuthLoginCommand(
            ExternalId: $"{provider}-user-id",
            Email: $"user@{provider}.com",
            DisplayName: $"{provider} User",
            Provider: provider
        );

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.ExternalId, command.Provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns($"{provider}-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Provider.Should().Be(provider);
    }
}
