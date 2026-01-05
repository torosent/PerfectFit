using MediatR;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Identity;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Auth.Commands;

/// <summary>
/// Command for logging in a local user.
/// </summary>
public record LocalLoginCommand(
    string Email,
    string Password
) : IRequest<LocalLoginResult>;

/// <summary>
/// Result of a local login operation.
/// </summary>
public record LocalLoginResult(
    bool Success,
    string? Token = null,
    UserResult? User = null,
    string? ErrorMessage = null,
    DateTime? LockoutEnd = null
);

/// <summary>
/// Handler for the local login command.
/// </summary>
public class LocalLoginCommandHandler : IRequestHandler<LocalLoginCommand, LocalLoginResult>
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LocalLoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LocalLoginResult> Handle(LocalLoginCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new LocalLoginResult(false, ErrorMessage: "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new LocalLoginResult(false, ErrorMessage: "Password is required.");
        }

        // Find user by email where AuthProvider = Local
        var user = await _userRepository.GetByExternalIdAsync(
            request.Email,
            AuthProvider.Local,
            cancellationToken);

        // User not found or OAuth user (returns null for wrong provider)
        if (user is null)
        {
            return new LocalLoginResult(false, ErrorMessage: "Invalid email or password.");
        }

        // Check if user is locked out
        if (user.IsLockedOut())
        {
            return new LocalLoginResult(
                Success: false,
                ErrorMessage: "Account is locked due to too many failed login attempts. Please try again later.",
                LockoutEnd: user.LockoutEnd);
        }

        // Verify password
        if (string.IsNullOrEmpty(user.PasswordHash) ||
            !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // Increment failed attempts
            user.IncrementFailedLoginAttempts();

            // Lock account if max attempts reached
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.SetLockout(DateTime.UtcNow.Add(LockoutDuration));
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return new LocalLoginResult(false, ErrorMessage: "Invalid email or password.");
        }

        // Check if email is verified
        if (!user.EmailVerified)
        {
            return new LocalLoginResult(false, ErrorMessage: "Please verify your email before logging in.");
        }

        // Reset failed attempts on successful login
        user.ResetFailedLoginAttempts();

        // Update last login time
        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);

        return new LocalLoginResult(
            Success: true,
            Token: token,
            User: new UserResult(
                Id: user.Id,
                ExternalId: user.ExternalId,
                Email: user.Email,
                DisplayName: user.DisplayName,
                Avatar: user.Avatar,
                Provider: user.Provider,
                HighScore: user.HighScore,
                GamesPlayed: user.GamesPlayed,
                Role: user.Role));
    }
}
