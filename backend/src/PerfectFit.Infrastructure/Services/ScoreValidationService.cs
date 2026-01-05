using Microsoft.EntityFrameworkCore;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;

namespace PerfectFit.Infrastructure.Services;

/// <summary>
/// Service for validating score submissions before adding to leaderboard.
/// Implements comprehensive anti-cheat validation.
/// </summary>
public class ScoreValidationService : IScoreValidationService
{
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly ILeaderboardRepository _leaderboardRepository;

    // Anti-cheat thresholds
    private const int MinGameDurationSeconds = 5;        // Minimum game duration
    private const int MaxScorePerMove = 500;             // Maximum reasonable score per move
    private const double MaxAverageScorePerSecond = 50;  // Maximum average score rate
    private const int MinMovesForHighScore = 5;          // Minimum moves for a valid high score
    private const int SuspiciouslyHighScore = 10000;     // Score that triggers additional checks
    private const int MaxLeaderboardEntriesPerUser = 100; // Prevent leaderboard spam
    private const int MaxGameAgeHours = 48;              // Maximum age of a game to submit
    private const int MaxClockSkewMinutes = 5;           // Maximum allowable clock skew for timestamps

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

        // Check if user has too many leaderboard entries (prevent spam)
        var userEntryCount = await _leaderboardRepository.GetUserEntryCountAsync(userId, cancellationToken);
        if (userEntryCount >= MaxLeaderboardEntriesPerUser)
        {
            return new ScoreValidationResult(
                IsValid: false,
                ErrorMessage: "Maximum leaderboard entries reached. Only your best scores are retained.",
                GameSession: null);
        }

        // === ANTI-CHEAT VALIDATIONS ===

        // Check for future-dated timestamps (clock manipulation)
        var now = DateTime.UtcNow;
        if (session.StartedAt > now.AddMinutes(MaxClockSkewMinutes))
        {
            return new ScoreValidationResult(
                IsValid: false,
                ErrorMessage: "Invalid game timestamps detected.",
                GameSession: null);
        }

        // Check if ended time is in the future
        if (session.EndedAt.HasValue && session.EndedAt.Value > now.AddMinutes(MaxClockSkewMinutes))
        {
            return new ScoreValidationResult(
                IsValid: false,
                ErrorMessage: "Invalid game timestamps detected.",
                GameSession: null);
        }

        // Check if game is too old to submit
        var gameAge = (now - session.StartedAt).TotalHours;
        if (gameAge > MaxGameAgeHours)
        {
            return new ScoreValidationResult(
                IsValid: false,
                ErrorMessage: "Game session has expired. Scores must be submitted within 48 hours.",
                GameSession: null);
        }

        // Check minimum game duration
        var gameDuration = session.GetGameDuration();
        if (gameDuration < MinGameDurationSeconds)
        {
            return new ScoreValidationResult(
                IsValid: false,
                ErrorMessage: "Game duration too short.",
                GameSession: null);
        }

        // Check minimum moves for high scores
        if (session.Score > SuspiciouslyHighScore && session.MoveCount < MinMovesForHighScore)
        {
            return new ScoreValidationResult(
                IsValid: false,
                ErrorMessage: "Insufficient moves for claimed score.",
                GameSession: null);
        }

        // Check score rate (points per second)
        if (gameDuration > 0)
        {
            var scoreRate = session.Score / gameDuration;
            if (scoreRate > MaxAverageScorePerSecond)
            {
                return new ScoreValidationResult(
                    IsValid: false,
                    ErrorMessage: "Score rate exceeds reasonable limits.",
                    GameSession: null);
            }
        }

        // Validate score is mathematically plausible
        if (!ValidateScorePlausibility(session))
        {
            return new ScoreValidationResult(
                IsValid: false,
                ErrorMessage: "Score is not mathematically plausible.",
                GameSession: null);
        }

        return new ScoreValidationResult(
            IsValid: true,
            ErrorMessage: null,
            GameSession: session);
    }

    /// <summary>
    /// Validates that the score is mathematically plausible given the game state.
    /// </summary>
    private static bool ValidateScorePlausibility(GameSession session)
    {
        // Score must be non-negative
        if (session.Score < 0)
            return false;

        // If there are moves, there should be some relationship between moves and score
        if (session.MoveCount > 0)
        {
            // Maximum theoretical score per move (clearing 10 lines with max combo)
            // 10 lines = 400 base points, with high combo multiplier ~5x = 2000 max per move
            var maxTheoreticalScore = session.MoveCount * 2000;
            if (session.Score > maxTheoreticalScore)
                return false;
        }

        // Lines cleared should not exceed theoretical maximum
        // Each move can clear at most 2 lines (one row, one column)
        // With perfect play, each move could clear 2 lines max
        var maxTheoreticalLines = session.MoveCount * 2;
        if (session.LinesCleared > maxTheoreticalLines)
            return false;

        // Max combo should not exceed lines cleared
        // You need to clear lines to build combo
        if (session.MaxCombo > session.LinesCleared)
            return false;

        return true;
    }
}
