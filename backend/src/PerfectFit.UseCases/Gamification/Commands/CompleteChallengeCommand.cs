using MediatR;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.UseCases.Gamification.Commands;

/// <summary>
/// Command to complete a challenge for a user.
/// </summary>
/// <param name="UserId">The user's external ID (GUID).</param>
/// <param name="ChallengeId">The challenge's external ID (GUID).</param>
/// <param name="GameSessionId">The game session ID used to validate the completion.</param>
public record CompleteChallengeCommand(Guid UserId, Guid ChallengeId, Guid GameSessionId) : IRequest<Result<ChallengeProgressResult>>;

/// <summary>
/// Handler for completing challenges.
/// </summary>
public class CompleteChallengeCommandHandler : IRequestHandler<CompleteChallengeCommand, Result<ChallengeProgressResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IChallengeService _challengeService;
    private readonly ISeasonPassService _seasonPassService;

    public CompleteChallengeCommandHandler(
        IUserRepository userRepository,
        IGameSessionRepository gameSessionRepository,
        IChallengeService challengeService,
        ISeasonPassService seasonPassService)
    {
        _userRepository = userRepository;
        _gameSessionRepository = gameSessionRepository;
        _challengeService = challengeService;
        _seasonPassService = seasonPassService;
    }

    public async Task<Result<ChallengeProgressResult>> Handle(CompleteChallengeCommand request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromGuid(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<ChallengeProgressResult>.Failure($"User {request.UserId} not found");
        }

        var gameSession = await _gameSessionRepository.GetByIdAsync(request.GameSessionId, cancellationToken);
        if (gameSession is null)
        {
            return Result<ChallengeProgressResult>.Failure("Game session not found");
        }

        if (gameSession.UserId != userId)
        {
            return Result<ChallengeProgressResult>.Failure("Game session does not belong to the user");
        }

        var challengeId = GetChallengeIdFromGuid(request.ChallengeId);
        var userChallenge = await _challengeService.GetOrCreateUserChallengeAsync(userId, challengeId, cancellationToken);

        var isValid = await _challengeService.ValidateChallengeCompletionAsync(userChallenge, gameSession, cancellationToken);
        if (!isValid)
        {
            return Result<ChallengeProgressResult>.Failure("Challenge validation failed");
        }

        // Calculate progress delta based on game session (simplified: use score / 100)
        var progressDelta = Math.Max(1, gameSession.Score / 100);
        var result = await _challengeService.UpdateProgressAsync(userChallenge, progressDelta, cancellationToken);

        // If challenge was completed, add XP
        if (result.IsCompleted && result.XPEarned > 0)
        {
            await _seasonPassService.AddXPAsync(user, result.XPEarned, "challenge", cancellationToken);
        }

        return Result<ChallengeProgressResult>.Success(result);
    }

    private static int GetUserIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }

    private static int GetChallengeIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }
}
