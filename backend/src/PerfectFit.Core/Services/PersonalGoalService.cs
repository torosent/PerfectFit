using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.Core.Services;

/// <summary>
/// Service for managing personalized goals.
/// </summary>
public class PersonalGoalService : IPersonalGoalService
{
    private readonly IGamificationRepository _repository;

    private const int DefaultTargetScore = 100;
    private const int DefaultAccuracyTarget = 70;
    private const double ImprovementPercentage = 0.10; // 10% improvement target

    public PersonalGoalService(IGamificationRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PersonalGoal>> GetActiveGoalsAsync(int userId, CancellationToken ct = default)
    {
        return await _repository.GetActiveGoalsAsync(userId, ct);
    }

    /// <inheritdoc />
    public async Task<PersonalGoal> GenerateGoalAsync(User user, GoalType type, CancellationToken ct = default)
    {
        var (targetValue, description) = await CalculateGoalTargetAsync(user, type, ct);

        var goal = PersonalGoal.Create(
            user.Id,
            type,
            targetValue,
            description,
            DateTime.UtcNow.AddHours(24)); // 24 hour expiry

        await _repository.AddGoalAsync(goal, ct);

        return goal;
    }

    /// <inheritdoc />
    public async Task<GoalProgressResult> UpdateGoalProgressAsync(PersonalGoal goal, int newValue, CancellationToken ct = default)
    {
        goal.UpdateProgress(newValue);
        await _repository.UpdateGoalAsync(goal, ct);

        return new GoalProgressResult(
            Success: true,
            NewProgress: goal.CurrentValue,
            IsCompleted: goal.IsCompleted);
    }

    /// <inheritdoc />
    public async Task<UserStats> CalculateUserStatsAsync(int userId, CancellationToken ct = default)
    {
        var sessions = await _repository.GetUserGameSessionsAsync(userId, null, ct);
        var completedSessions = sessions.Where(s => s.Status == GameStatus.Ended).ToList();

        if (completedSessions.Count == 0)
        {
            return new UserStats(
                AverageScore: 0,
                BestScore: 0,
                TotalGames: 0,
                Accuracy: 0,
                AverageLinesCleared: 0,
                TotalLinesCleared: 0,
                AverageCombo: 0,
                BestCombo: 0);
        }

        int totalGames = completedSessions.Count;
        double averageScore = completedSessions.Average(s => s.Score);
        int bestScore = completedSessions.Max(s => s.Score);
        int totalLinesCleared = completedSessions.Sum(s => s.LinesCleared);
        double averageLinesCleared = completedSessions.Average(s => s.LinesCleared);
        int bestCombo = completedSessions.Max(s => s.MaxCombo);
        double averageCombo = completedSessions.Average(s => s.MaxCombo);

        // Calculate accuracy based on lines cleared vs move count
        // Higher ratio of lines to moves = better accuracy
        double accuracy = CalculateAccuracy(completedSessions);

        return new UserStats(
            AverageScore: averageScore,
            BestScore: bestScore,
            TotalGames: totalGames,
            Accuracy: accuracy,
            AverageLinesCleared: averageLinesCleared,
            TotalLinesCleared: totalLinesCleared,
            AverageCombo: averageCombo,
            BestCombo: bestCombo);
    }

    private async Task<(int targetValue, string description)> CalculateGoalTargetAsync(User user, GoalType type, CancellationToken ct)
    {
        var sessions = await _repository.GetUserGameSessionsAsync(user.Id, null, ct);
        var completedSessions = sessions.Where(s => s.Status == GameStatus.Ended).ToList();

        return type switch
        {
            GoalType.BeatAverage => CalculateBeatAverageTarget(completedSessions),
            GoalType.NewPersonalBest => CalculateNewPersonalBestTarget(user),
            GoalType.ImproveAccuracy => CalculateImproveAccuracyTarget(completedSessions),
            _ => (DefaultTargetScore, "Score target")
        };
    }

    private static (int targetValue, string description) CalculateBeatAverageTarget(List<GameSession> sessions)
    {
        if (sessions.Count == 0)
        {
            return (DefaultTargetScore, $"Score at least {DefaultTargetScore} points");
        }

        double averageScore = sessions.Average(s => s.Score);
        int target = (int)(averageScore * (1 + ImprovementPercentage));
        return (target, $"Beat your average score of {(int)averageScore}");
    }

    private static (int targetValue, string description) CalculateNewPersonalBestTarget(User user)
    {
        int target = user.HighScore + 1;
        return (target, $"Beat your personal best of {user.HighScore}");
    }

    private static (int targetValue, string description) CalculateImproveAccuracyTarget(List<GameSession> sessions)
    {
        if (sessions.Count == 0)
        {
            return (DefaultAccuracyTarget, $"Achieve {DefaultAccuracyTarget}% accuracy");
        }

        double currentAccuracy = CalculateAccuracy(sessions);
        int target = (int)(currentAccuracy * (1 + ImprovementPercentage));
        target = Math.Min(target, 100); // Cap at 100%
        return (target, $"Improve your accuracy to {target}%");
    }

    private static double CalculateAccuracy(List<GameSession> sessions)
    {
        if (sessions.Count == 0)
        {
            return 0;
        }

        // Accuracy = (lines cleared / move count) * 100
        // This represents how efficiently the player clears lines
        int totalMoves = sessions.Sum(s => s.MoveCount);
        int totalLinesCleared = sessions.Sum(s => s.LinesCleared);

        if (totalMoves == 0)
        {
            return 0;
        }

        return (totalLinesCleared * 100.0) / totalMoves;
    }
}
