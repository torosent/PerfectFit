using MediatR;
using Microsoft.Extensions.Options;
using PerfectFit.Core.Configuration;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Auth.Commands;

/// <summary>
/// Command for handling OAuth login/registration.
/// </summary>
public record OAuthLoginCommand(
    string ExternalId,
    string? Email,
    string DisplayName,
    AuthProvider Provider
) : IRequest<OAuthLoginResult>;

/// <summary>
/// Result of an OAuth login operation.
/// </summary>
public record OAuthLoginResult(
    string Token,
    UserResult User
);

/// <summary>
/// User data returned from OAuth login.
/// </summary>
public record UserResult(
    int Id,
    string ExternalId,
    string? Email,
    string DisplayName,
    string Username,
    string? Avatar,
    AuthProvider Provider,
    int HighScore,
    int GamesPlayed
);

/// <summary>
/// Handler for OAuth login command.
/// </summary>
public class OAuthLoginCommandHandler : IRequestHandler<OAuthLoginCommand, OAuthLoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly AdminSettings _adminSettings;

    public OAuthLoginCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IOptions<AdminSettings> adminSettings)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _adminSettings = adminSettings.Value;
    }

    public async Task<OAuthLoginResult> Handle(OAuthLoginCommand request, CancellationToken cancellationToken)
    {
        // Reject Google and Facebook providers - only Microsoft, Local, and Guest are supported
        if (request.Provider == AuthProvider.Google)
        {
            throw new InvalidOperationException("Google authentication is not supported. Please use Microsoft, local, or guest authentication.");
        }

        if (request.Provider == AuthProvider.Facebook)
        {
            throw new InvalidOperationException("Facebook authentication is not supported. Please use Microsoft, local, or guest authentication.");
        }

        // Try to find existing user
        var user = await _userRepository.GetByExternalIdAsync(
            request.ExternalId,
            request.Provider,
            cancellationToken);

        if (user is null)
        {
            // Check if this email should be an admin
            var shouldBeAdmin = !string.IsNullOrEmpty(request.Email) &&
                _adminSettings.Emails.Any(e => e.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

            // Create new user with appropriate role
            user = User.Create(
                request.ExternalId,
                request.Email,
                request.DisplayName,
                request.Provider,
                shouldBeAdmin ? UserRole.Admin : UserRole.User
            );

            user = await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            // Update last login time
            user.UpdateLastLogin();

            // Check if admin status needs to be updated based on config
            var isConfiguredAdmin = !string.IsNullOrEmpty(user.Email) &&
                _adminSettings.Emails.Any(e => e.Equals(user.Email, StringComparison.OrdinalIgnoreCase));

            if (isConfiguredAdmin && user.Role != UserRole.Admin)
            {
                // Promote user to admin if their email is in the admin list
                user.SetRole(UserRole.Admin);
            }
            // Note: We don't auto-demote users removed from config for safety

            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);

        return new OAuthLoginResult(
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
                GamesPlayed: user.GamesPlayed
            )
        );
    }
}
