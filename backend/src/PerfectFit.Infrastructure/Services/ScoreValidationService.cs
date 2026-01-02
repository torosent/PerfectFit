using Microsoft.EntityFrameworkCore;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;

namespace PerfectFit.Infrastructure.Services;

/// <summary>
/// Service for validating score submissions before adding to leaderboard.
/// </summary>
public class ScoreValidationService : IScoreValidationService
{
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly ILeaderboardRepository _leaderboardRepository;

    public ScoreValidationService(
        IGameSessionRepository gameSessionRepository,
        ILeaderboardRepository leaderboardRepository)
    {
        _gameSessionRepository = gameSessionRepository;
        _leaderboardRepository = leaderboardRepository;
    }

    public async Task<ScoreValidationResult> ValidateScoreAsync(
        Guid gameSessionId,
        int userId,
        int? claimedScore = null,
        CancellationToken cancellationToken = default)
    {
        // Check if game session exists
        var session = await _gameSessionRepository.GetByIdAsync(gameSessionId, cancellationToken);
        
        if (session is null)
        {
            return new ScoreValidationResult(
                IsValid: false,
                ErrorMessage: "Game session not found.",
                GameSession: null);
        }

        // Check if game session belongs to user
        if (session.UserId != userId)
        {
            return new ScoreValidationResult(
                IsValid: false,
                ErrorMessage: "Game session does not belong to this user.",
                GameSession: null);
        }

        // Check if game has ended
        if (session.Status != GameStatus.Ended)
        {
            return new ScoreValidationResult(
                IsValid: false,
                ErrorMessage: "Game has not ended yet.",
                GameSession: null);
        }

        // Check if claimed score matches (if provided)
        if (claimedScore.HasValue && claimedScore.Value != session.Score)
        {
            return new ScoreValidationResult(
                IsValid: false,
                ErrorMessage: "Claimed score does not match game session score.",
                GameSession: null);
        }

        // Check if already submitted to leaderboard
        var alreadySubmitted = await _leaderboardRepository.ExistsByGameSessionIdAsync(gameSessionId, cancellationToken);
        
        if (alreadySubmitted)
        {
            return new ScoreValidationResult(
                IsValid: false,
                ErrorMessage: "Score has already been submitted to leaderboard.",
                GameSession: null);
        }

        return new ScoreValidationResult(
            IsValid: true,
            ErrorMessage: null,
            GameSession: session);
    }
}
