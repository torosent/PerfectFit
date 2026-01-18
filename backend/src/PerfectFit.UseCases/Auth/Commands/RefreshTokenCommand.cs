using MediatR;
using PerfectFit.Core.Interfaces;
using System.Security.Claims;

namespace PerfectFit.UseCases.Auth.Commands;

/// <summary>
/// Command for refreshing an expired JWT token.
/// </summary>
public record RefreshTokenCommand(string Token) : IRequest<RefreshTokenResult?>;

/// <summary>
/// Result of a token refresh operation.
/// </summary>
public record RefreshTokenResult(string Token, UserResult User);

/// <summary>
/// Handler for refresh token command.
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResult?>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<RefreshTokenResult?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Validate the existing token (even if expired, we want to check the signature)
        var principal = _jwtService.ValidateToken(request.Token, false);

        if (principal is null)
        {
            return null;
        }

        // Get user ID from claims
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        // Get user from database
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        // Generate new token
        var newToken = _jwtService.GenerateToken(user);

        return new RefreshTokenResult(
            Token: newToken,
            User: new UserResult(
                Id: user.Id,
                ExternalId: user.ExternalId,
                Email: user.Email,
                DisplayName: user.DisplayName,
                Avatar: user.Avatar,
                Provider: user.Provider,
                HighScore: user.HighScore,
                GamesPlayed: user.GamesPlayed,
                Role: user.Role
            )
        );
    }
}
