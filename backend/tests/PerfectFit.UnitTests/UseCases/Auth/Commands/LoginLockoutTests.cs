using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Identity;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Auth.Commands;

namespace PerfectFit.UnitTests.UseCases.Auth.Commands;

public class LoginLockoutTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly LocalLoginCommandHandler _sut;

    public LoginLockoutTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();

        _sut = new LocalLoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Login_LockedAccount_ReturnsLockedError()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetPasswordHash("hashed_password");
        user.MarkEmailAsVerified();
        user.SetLockout(DateTime.UtcNow.AddMinutes(10)); // Locked for 10 more minutes

        var command = new LocalLoginCommand(
            Email: "test@example.com",
            Password: "Password123");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("locked");
        result.Token.Should().BeNull();

        // Password verification should NOT be called when account is locked
        _passwordHasherMock.Verify(
            x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_WrongPassword_IncrementsFailedAttempts()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetPasswordHash("hashed_password");
        user.MarkEmailAsVerified();

        var command = new LocalLoginCommand(
            Email: "test@example.com",
            Password: "WrongPassword123");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, "hashed_password"))
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(1);

        // Verify user was updated
        _userRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<User>(u => u.FailedLoginAttempts == 1), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Login_FifthFailedAttempt_LocksAccount()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetPasswordHash("hashed_password");
        user.MarkEmailAsVerified();

        // Simulate 4 previous failed attempts
        for (int i = 0; i < 4; i++)
        {
            user.IncrementFailedLoginAttempts();
        }

        var command = new LocalLoginCommand(
            Email: "test@example.com",
            Password: "WrongPassword123");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, "hashed_password"))
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(5);
        user.IsLockedOut().Should().BeTrue();
        user.LockoutEnd.Should().NotBeNull();
        user.LockoutEnd.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Login_SuccessfulLogin_ResetsFailedAttempts()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetPasswordHash("hashed_password");
        user.MarkEmailAsVerified();

        // Simulate some previous failed attempts (less than lockout threshold)
        for (int i = 0; i < 3; i++)
        {
            user.IncrementFailedLoginAttempts();
        }

        var command = new LocalLoginCommand(
            Email: "test@example.com",
            Password: "Password123");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, "hashed_password"))
            .Returns(true);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(user))
            .Returns("valid_jwt_token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(0);

        // Verify user was updated with reset failed attempts
        _userRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<User>(u => u.FailedLoginAttempts == 0), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Login_LockoutExpired_AllowsLogin()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetPasswordHash("hashed_password");
        user.MarkEmailAsVerified();

        // Set a lockout that has already expired
        user.SetLockout(DateTime.UtcNow.AddMinutes(-1)); // Expired 1 minute ago

        var command = new LocalLoginCommand(
            Email: "test@example.com",
            Password: "Password123");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, "hashed_password"))
            .Returns(true);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(user))
            .Returns("valid_jwt_token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().Be("valid_jwt_token");
    }

    [Fact]
    public async Task Login_LockedAccount_ShowsUnlockTime()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetPasswordHash("hashed_password");
        user.MarkEmailAsVerified();

        var lockoutEnd = DateTime.UtcNow.AddMinutes(10);
        user.SetLockout(lockoutEnd);

        var command = new LocalLoginCommand(
            Email: "test@example.com",
            Password: "Password123");

        _userRepositoryMock
            .Setup(x => x.GetByExternalIdAsync(command.Email, AuthProvider.Local, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("locked");
        result.LockoutEnd.Should().NotBeNull();
        result.LockoutEnd.Should().BeCloseTo(lockoutEnd, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void IsLockedOut_BeforeLockoutEnd_ReturnsTrue()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetLockout(DateTime.UtcNow.AddMinutes(10)); // Locked for 10 more minutes

        // Act
        var result = user.IsLockedOut();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLockedOut_AfterLockoutEnd_ReturnsFalse()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.SetLockout(DateTime.UtcNow.AddMinutes(-1)); // Lockout expired 1 minute ago

        // Act
        var result = user.IsLockedOut();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsLockedOut_NoLockout_ReturnsFalse()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);

        // Act
        var result = user.IsLockedOut();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IncrementFailedLoginAttempts_IncrementsCount()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.FailedLoginAttempts.Should().Be(0);

        // Act
        user.IncrementFailedLoginAttempts();
        user.IncrementFailedLoginAttempts();
        user.IncrementFailedLoginAttempts();

        // Assert
        user.FailedLoginAttempts.Should().Be(3);
    }

    [Fact]
    public void ResetFailedLoginAttempts_ResetsCountAndClearsLockout()
    {
        // Arrange
        var user = User.Create("test@example.com", "test@example.com", "Test User", AuthProvider.Local);
        user.IncrementFailedLoginAttempts();
        user.IncrementFailedLoginAttempts();
        user.SetLockout(DateTime.UtcNow.AddMinutes(15));

        // Act
        user.ResetFailedLoginAttempts();

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
    }
}
