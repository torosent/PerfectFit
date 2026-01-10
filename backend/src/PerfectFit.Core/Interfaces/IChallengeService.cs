using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.Core.Interfaces;

/// <summary>
/// Service for managing challenges and user progress.
/// </summary>
public interface IChallengeService
{
    /// <summary>
    /// Gets all currently active challenges, optionally filtered by type.
    /// </summary>
    /// <param name="type">Optional challenge type filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of active challenges.</returns>
    Task<IReadOnlyList<Challenge>> GetActiveChallengesAsync(ChallengeType? type = null, CancellationToken ct = default);

    /// <summary>
    /// Gets or creates a user's challenge record for the specified challenge.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="challengeId">The challenge's ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The existing or newly created user challenge record.</returns>
    Task<UserChallenge> GetOrCreateUserChallengeAsync(int userId, int challengeId, CancellationToken ct = default);

    /// <summary>
    /// Updates the progress of a user's challenge.
    /// </summary>
    /// <param name="userChallenge">The user challenge to update.</param>
    /// <param name="progressDelta">The amount of progress to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the progress update.</returns>
    Task<ChallengeProgressResult> UpdateProgressAsync(UserChallenge userChallenge, int progressDelta, CancellationToken ct = default);

    /// <summary>
    /// Validates that a challenge completion is legitimate based on game session data.
    /// </summary>
    /// <param name="userChallenge">The user challenge to validate.</param>
    /// <param name="gameSession">The game session to validate against.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the completion is valid, false otherwise.</returns>
    Task<bool> ValidateChallengeCompletionAsync(UserChallenge userChallenge, GameSession gameSession, CancellationToken ct = default);
}
