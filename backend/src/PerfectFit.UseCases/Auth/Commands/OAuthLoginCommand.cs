using MediatR;
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

    public OAuthLoginCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<OAuthLoginResult> Handle(OAuthLoginCommand request, CancellationToken cancellationToken)
    {
        // Try to find existing user
        var user = await _userRepository.GetByExternalIdAsync(
            request.ExternalId,
            request.Provider,
            cancellationToken);

        if (user is null)
        {
            // Create new user
            user = User.Create(
                request.ExternalId,
                request.Email,
                request.DisplayName,
                request.Provider
            );

            user = await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            // Update last login time
            user.UpdateLastLogin();
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
                Provider: user.Provider,
                HighScore: user.HighScore,
                GamesPlayed: user.GamesPlayed
            )
        );
    }
}
