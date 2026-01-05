using PerfectFit.Core.Entities;

namespace PerfectFit.Core.Services;

/// <summary>
/// Service for validating score submissions before adding to leaderboard.
/// </summary>
public interface IScoreValidationService
{
    /// <summary>
    /// Validates that a score submission is legitimate.
    /// Checks: game session exists, belongs to user, is completed, score matches.
    /// </summary>
    /// <param name="gameSessionId">The game session ID to validate.</param>
    /// <param name="userId">The user attempting to submit the score.</param>
    /// <param name="claimedScore">The score being claimed (optional - if null, uses session score).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with details.</returns>
    Task<ScoreValidationResult> ValidateScoreAsync(
        Guid gameSessionId,
        int userId,
        int? claimedScore = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of score validation.
/// </summary>
/// <param name="IsValid">Whether the score submission is valid.</param>
/// <param name="ErrorMessage">Error message if validation failed.</param>
/// <param name="GameSession">The validated game session if successful.</param>
public record ScoreValidationResult(
    bool IsValid,
    string? ErrorMessage,
    GameSession? GameSession
);
