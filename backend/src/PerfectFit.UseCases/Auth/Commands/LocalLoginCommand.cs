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
    string? ErrorMessage = null
);

/// <summary>
/// Handler for the local login command.
/// </summary>
public class LocalLoginCommandHandler : IRequestHandler<LocalLoginCommand, LocalLoginResult>
{
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

        // Verify password
        if (string.IsNullOrEmpty(user.PasswordHash) ||
            !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return new LocalLoginResult(false, ErrorMessage: "Invalid email or password.");
        }

        // Check if email is verified
        if (!user.EmailVerified)
        {
            return new LocalLoginResult(false, ErrorMessage: "Please verify your email before logging in.");
        }

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
                Username: user.Username,
                Avatar: user.Avatar,
                Provider: user.Provider,
                HighScore: user.HighScore,
                GamesPlayed: user.GamesPlayed));
    }
}
