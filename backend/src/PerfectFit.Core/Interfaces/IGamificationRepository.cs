using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.Core.Interfaces;

/// <summary>
/// Repository interface for gamification-related data access.
/// </summary>
public interface IGamificationRepository
{
    // Achievement methods
    Task<IReadOnlyList<Achievement>> GetAllAchievementsAsync(CancellationToken ct = default);
    Task<Achievement?> GetAchievementByIdAsync(int achievementId, CancellationToken ct = default);
    Task<IReadOnlyList<UserAchievement>> GetUserAchievementsAsync(int userId, CancellationToken ct = default);
    Task<UserAchievement?> GetUserAchievementAsync(int userId, int achievementId, CancellationToken ct = default);
    Task AddUserAchievementAsync(UserAchievement userAchievement, CancellationToken ct = default);
    Task UpdateUserAchievementAsync(UserAchievement userAchievement, CancellationToken ct = default);

    // Challenge methods
    Task<IReadOnlyList<Challenge>> GetActiveChallengesAsync(ChallengeType? type = null, CancellationToken ct = default);
    Task<Challenge?> GetChallengeByIdAsync(int challengeId, CancellationToken ct = default);
    Task<IReadOnlyList<UserChallenge>> GetUserChallengesAsync(int userId, IEnumerable<int>? challengeIds = null, CancellationToken ct = default);
    Task<UserChallenge?> GetUserChallengeAsync(int userId, int challengeId, CancellationToken ct = default);
    Task AddUserChallengeAsync(UserChallenge userChallenge, CancellationToken ct = default);
    Task UpdateUserChallengeAsync(UserChallenge userChallenge, CancellationToken ct = default);

    // Season methods
    Task<Season?> GetCurrentSeasonAsync(CancellationToken ct = default);
    Task<Season?> GetSeasonByIdAsync(int seasonId, CancellationToken ct = default);
    Task<IReadOnlyList<SeasonReward>> GetSeasonRewardsAsync(int seasonId, CancellationToken ct = default);
    Task<SeasonReward?> GetSeasonRewardByIdAsync(int seasonRewardId, CancellationToken ct = default);
    Task<IReadOnlyList<int>> GetClaimedRewardIdsAsync(int userId, int seasonId, CancellationToken ct = default);
    Task AddClaimedRewardAsync(int userId, int seasonRewardId, CancellationToken ct = default);

    // Cosmetic methods
    Task<IReadOnlyList<Cosmetic>> GetAllCosmeticsAsync(CosmeticType? type = null, CancellationToken ct = default);
    Task<Cosmetic?> GetCosmeticByIdAsync(int cosmeticId, CancellationToken ct = default);
    Task<IReadOnlyList<UserCosmetic>> GetUserCosmeticsAsync(int userId, CancellationToken ct = default);
    Task<UserCosmetic?> GetUserCosmeticAsync(int userId, int cosmeticId, CancellationToken ct = default);
    Task AddUserCosmeticAsync(UserCosmetic userCosmetic, CancellationToken ct = default);

    // Personal goal methods
    Task<IReadOnlyList<PersonalGoal>> GetActiveGoalsAsync(int userId, CancellationToken ct = default);
    Task<PersonalGoal?> GetGoalByIdAsync(int goalId, CancellationToken ct = default);
    Task AddGoalAsync(PersonalGoal goal, CancellationToken ct = default);
    Task UpdateGoalAsync(PersonalGoal goal, CancellationToken ct = default);

    // Statistics methods
    Task<IReadOnlyList<GameSession>> GetUserGameSessionsAsync(int userId, int? limit = null, CancellationToken ct = default);
    Task<int> GetCompletedChallengeCountAsync(int userId, CancellationToken ct = default);
}
