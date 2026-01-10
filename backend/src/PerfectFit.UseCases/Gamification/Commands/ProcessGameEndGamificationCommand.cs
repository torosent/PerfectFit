using MediatR;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.UseCases.Gamification.Commands;

/// <summary>
/// Command to process all gamification updates after a game ends.
/// </summary>
/// <param name="UserId">The user's external ID (GUID).</param>
/// <param name="GameSessionId">The game session ID.</param>
public record ProcessGameEndGamificationCommand(Guid UserId, Guid GameSessionId) : IRequest<GameEndGamificationResult>;

/// <summary>
/// Handler for processing all gamification after game ends.
/// </summary>
public class ProcessGameEndGamificationCommandHandler : IRequestHandler<ProcessGameEndGamificationCommand, GameEndGamificationResult>
{
    private const int BaseXPPerGame = 10;
    private const int XPPerScore100 = 1;

    private readonly IUserRepository _userRepository;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IStreakService _streakService;
    private readonly IChallengeService _challengeService;
    private readonly IAchievementService _achievementService;
    private readonly ISeasonPassService _seasonPassService;
    private readonly IPersonalGoalService _personalGoalService;

    public ProcessGameEndGamificationCommandHandler(
        IUserRepository userRepository,
        IGameSessionRepository gameSessionRepository,
        IStreakService streakService,
        IChallengeService challengeService,
        IAchievementService achievementService,
        ISeasonPassService seasonPassService,
        IPersonalGoalService personalGoalService)
    {
        _userRepository = userRepository;
        _gameSessionRepository = gameSessionRepository;
        _streakService = streakService;
        _challengeService = challengeService;
        _achievementService = achievementService;
        _seasonPassService = seasonPassService;
        _personalGoalService = personalGoalService;
    }

    public async Task<GameEndGamificationResult> Handle(ProcessGameEndGamificationCommand request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromGuid(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException($"User {request.UserId} not found");
        }

        var gameSession = await _gameSessionRepository.GetByIdAsync(request.GameSessionId, cancellationToken);
        if (gameSession is null)
        {
            throw new InvalidOperationException($"Game session {request.GameSessionId} not found");
        }

        if (gameSession.UserId != userId)
        {
            throw new InvalidOperationException("Game session does not belong to the user");
        }

        // Update user stats (games played, high score)
        user.IncrementGamesPlayed();
        user.UpdateHighScore(gameSession.Score);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // 1. Update streak
        var gameEndTime = gameSession.EndedAt.HasValue 
            ? new DateTimeOffset(gameSession.EndedAt.Value, TimeSpan.Zero) 
            : DateTimeOffset.UtcNow;
        var streakResult = await _streakService.UpdateStreakAsync(user, gameEndTime, cancellationToken);

        // 2. Update challenge progress
        var challengeUpdates = await ProcessChallengesAsync(user, gameSession, cancellationToken);

        // 3. Check and unlock achievements
        var achievementResult = await _achievementService.CheckAndUnlockAchievementsAsync(user, gameSession, cancellationToken);

        // 4. Add season XP
        var xpAmount = CalculateXPFromGame(gameSession);
        var seasonResult = await _seasonPassService.AddXPAsync(user, xpAmount, "game_completion", cancellationToken);

        // 5. Update personal goals
        var goalUpdates = await ProcessPersonalGoalsAsync(user, gameSession, cancellationToken);

        return new GameEndGamificationResult(
            Streak: streakResult,
            ChallengeUpdates: challengeUpdates,
            AchievementUpdates: achievementResult,
            SeasonProgress: seasonResult,
            GoalUpdates: goalUpdates
        );
    }

    private async Task<IReadOnlyList<ChallengeProgressResult>> ProcessChallengesAsync(
        User user,
        GameSession gameSession,
        CancellationToken cancellationToken)
    {
        var results = new List<ChallengeProgressResult>();
        var activeChallenges = await _challengeService.GetActiveChallengesAsync(null, cancellationToken);

        foreach (var challenge in activeChallenges)
        {
            var userChallenge = await _challengeService.GetOrCreateUserChallengeAsync(user.Id, challenge.Id, cancellationToken);
            
            if (userChallenge.IsCompleted)
            {
                continue;
            }

            var isValid = await _challengeService.ValidateChallengeCompletionAsync(userChallenge, gameSession, cancellationToken);
            if (isValid)
            {
                var progressDelta = ChallengeProgressCalculator.CalculateChallengeProgress(challenge, gameSession);
                var result = await _challengeService.UpdateProgressAsync(userChallenge, progressDelta, cancellationToken);
                results.Add(result);

                // If challenge completed, add XP
                if (result.IsCompleted && result.XPEarned > 0)
                {
                    await _seasonPassService.AddXPAsync(user, result.XPEarned, "challenge", cancellationToken);
                }
            }
        }

        return results;
    }

    private async Task<IReadOnlyList<GoalProgressResult>> ProcessPersonalGoalsAsync(
        User user,
        GameSession gameSession,
        CancellationToken cancellationToken)
    {
        var results = new List<GoalProgressResult>();
        var activeGoals = await _personalGoalService.GetActiveGoalsAsync(user.Id, cancellationToken);

        foreach (var goal in activeGoals)
        {
            if (goal.IsCompleted || goal.IsExpired)
            {
                continue;
            }

            var newValue = CalculateGoalProgress(goal, gameSession);
            var result = await _personalGoalService.UpdateGoalProgressAsync(goal, newValue, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    private static int CalculateXPFromGame(GameSession gameSession)
    {
        return BaseXPPerGame + (gameSession.Score / 100 * XPPerScore100);
    }

    private static int CalculateGoalProgress(PersonalGoal goal, GameSession gameSession)
    {
        // Calculate new progress based on goal type
        return goal.Type switch
        {
            Core.Enums.GoalType.BeatAverage => goal.CurrentValue + gameSession.Score,
            Core.Enums.GoalType.ImproveAccuracy => goal.CurrentValue + 1,
            Core.Enums.GoalType.NewPersonalBest => Math.Max(goal.CurrentValue, gameSession.Score),
            _ => goal.CurrentValue
        };
    }

    private static int GetUserIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }
}
