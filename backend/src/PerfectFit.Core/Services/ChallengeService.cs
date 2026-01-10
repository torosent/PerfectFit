using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.Core.Services;

/// <summary>
/// Service for managing challenges and user progress.
/// </summary>
public class ChallengeService : IChallengeService
{
    private readonly IGamificationRepository _repository;

    public ChallengeService(IGamificationRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Challenge>> GetActiveChallengesAsync(ChallengeType? type = null, CancellationToken ct = default)
    {
        return await _repository.GetActiveChallengesAsync(type, ct);
    }

    /// <inheritdoc />
    public async Task<UserChallenge> GetOrCreateUserChallengeAsync(int userId, int challengeId, CancellationToken ct = default)
    {
        var existing = await _repository.GetUserChallengeAsync(userId, challengeId, ct);
        if (existing != null)
        {
            return existing;
        }

        // Verify challenge exists
        var challenge = await _repository.GetChallengeByIdAsync(challengeId, ct);
        if (challenge == null)
        {
            throw new ArgumentException($"Challenge with ID {challengeId} not found.", nameof(challengeId));
        }

        var userChallenge = UserChallenge.Create(userId, challengeId);
        await _repository.AddUserChallengeAsync(userChallenge, ct);

        return userChallenge;
    }

    /// <inheritdoc />
    public async Task<ChallengeProgressResult> UpdateProgressAsync(UserChallenge userChallenge, int progressDelta, CancellationToken ct = default)
    {
        var challenge = userChallenge.Challenge ?? await _repository.GetChallengeByIdAsync(userChallenge.ChallengeId, ct);
        if (challenge == null)
        {
            return new ChallengeProgressResult(
                Success: false,
                ChallengeId: userChallenge.ChallengeId,
                ChallengeName: string.Empty,
                NewProgress: userChallenge.CurrentProgress,
                IsCompleted: userChallenge.IsCompleted,
                XPEarned: 0,
                ErrorMessage: "Challenge not found.");
        }

        bool wasCompleted = userChallenge.IsCompleted;

        userChallenge.AddProgress(progressDelta, challenge.TargetValue);

        await _repository.UpdateUserChallengeAsync(userChallenge, ct);

        // Calculate XP earned (only if just completed, not if was already completed)
        int xpEarned = 0;
        if (userChallenge.IsCompleted && !wasCompleted)
        {
            xpEarned = challenge.XPReward;
        }

        return new ChallengeProgressResult(
            Success: true,
            ChallengeId: challenge.Id,
            ChallengeName: challenge.Name,
            NewProgress: userChallenge.CurrentProgress,
            IsCompleted: userChallenge.IsCompleted,
            XPEarned: xpEarned);
    }

    /// <inheritdoc />
    public async Task<bool> ValidateChallengeCompletionAsync(UserChallenge userChallenge, GameSession gameSession, CancellationToken ct = default)
    {
        // Anti-cheat validation
        // 1. Game must belong to the user
        if (gameSession.UserId != userChallenge.UserId)
        {
            return false;
        }

        // 2. Game must be ended (completed)
        if (gameSession.Status != GameStatus.Ended)
        {
            return false;
        }

        // 3. Game must not be abandoned
        if (gameSession.Status == GameStatus.Abandoned)
        {
            return false;
        }

        // Additional validations could include:
        // - Check game session date is within challenge period
        // - Verify game session data hasn't been tampered with
        // - Check minimum game duration

        return true;
    }
}
