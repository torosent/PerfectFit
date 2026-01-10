using System.Text.Json;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.Core.Services;

/// <summary>
/// Service for managing achievements and tracking user progress.
/// </summary>
public class AchievementService : IAchievementService
{
    private readonly IGamificationRepository _repository;

    public AchievementService(IGamificationRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Achievement>> GetAllAchievementsAsync(CancellationToken ct = default)
    {
        return await _repository.GetAllAchievementsAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserAchievement>> GetUserAchievementsAsync(int userId, CancellationToken ct = default)
    {
        return await _repository.GetUserAchievementsAsync(userId, ct);
    }

    /// <inheritdoc />
    public async Task<AchievementUnlockResult> CheckAndUnlockAchievementsAsync(User user, GameSession? gameSession = null, CancellationToken ct = default)
    {
        var allAchievements = await _repository.GetAllAchievementsAsync(ct);
        var userAchievements = await _repository.GetUserAchievementsAsync(user.Id, ct);
        var unlockedIds = userAchievements.Where(ua => ua.IsUnlocked).Select(ua => ua.AchievementId).ToHashSet();

        var newlyUnlocked = new List<Achievement>();
        int totalRewards = 0;

        foreach (var achievement in allAchievements)
        {
            if (unlockedIds.Contains(achievement.Id))
            {
                continue; // Already unlocked
            }

            bool shouldUnlock = await CheckUnlockConditionAsync(user, achievement, ct);

            if (shouldUnlock)
            {
                var userAchievement = UserAchievement.Create(user.Id, achievement.Id);
                userAchievement.Unlock();
                await _repository.AddUserAchievementAsync(userAchievement, ct);

                newlyUnlocked.Add(achievement);
                totalRewards += achievement.RewardValue;
            }
        }

        return new AchievementUnlockResult(newlyUnlocked, totalRewards);
    }

    /// <inheritdoc />
    public async Task<int> CalculateProgressAsync(User user, Achievement achievement, CancellationToken ct = default)
    {
        var condition = ParseUnlockCondition(achievement.UnlockCondition);
        if (condition == null)
        {
            return 0;
        }

        int currentValue = await GetCurrentValueForConditionAsync(user, condition, ct);
        int targetValue = condition.Value;

        if (targetValue <= 0)
        {
            return 0;
        }

        int progress = (int)((currentValue * 100.0) / targetValue);
        return Math.Min(progress, 100);
    }

    private async Task<bool> CheckUnlockConditionAsync(User user, Achievement achievement, CancellationToken ct)
    {
        var condition = ParseUnlockCondition(achievement.UnlockCondition);
        if (condition == null)
        {
            return false;
        }

        int currentValue = await GetCurrentValueForConditionAsync(user, condition, ct);
        return currentValue >= condition.Value;
    }

    private async Task<int> GetCurrentValueForConditionAsync(User user, UnlockCondition condition, CancellationToken ct)
    {
        return condition.Type.ToLowerInvariant() switch
        {
            "score" => user.HighScore,
            "streak" => user.CurrentStreak,
            "games" => user.GamesPlayed,
            "challenges" => await _repository.GetCompletedChallengeCountAsync(user.Id, ct),
            _ => 0
        };
    }

    private static UnlockCondition? ParseUnlockCondition(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<UnlockCondition>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    private class UnlockCondition
    {
        public string Type { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
