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
    
    // Achievement admin methods
    Task<(IReadOnlyList<Achievement> Items, int TotalCount)> GetAchievementsPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task AddAchievementAsync(Achievement achievement, CancellationToken ct = default);
    Task UpdateAchievementAsync(Achievement achievement, CancellationToken ct = default);
    Task<bool> IsAchievementInUseAsync(int achievementId, CancellationToken ct = default);
    Task DeleteAchievementAsync(int achievementId, CancellationToken ct = default);

    // Challenge methods
    Task<IReadOnlyList<Challenge>> GetActiveChallengesAsync(ChallengeType? type = null, CancellationToken ct = default);
    Task<Challenge?> GetChallengeByIdAsync(int challengeId, CancellationToken ct = default);
    Task<IReadOnlyList<UserChallenge>> GetUserChallengesAsync(int userId, IEnumerable<int>? challengeIds = null, CancellationToken ct = default);
    Task<UserChallenge?> GetUserChallengeAsync(int userId, int challengeId, CancellationToken ct = default);
    Task AddUserChallengeAsync(UserChallenge userChallenge, CancellationToken ct = default);
    Task UpdateUserChallengeAsync(UserChallenge userChallenge, CancellationToken ct = default);
    Task AddChallengeAsync(Challenge challenge, CancellationToken ct = default);
    Task UpdateChallengeAsync(Challenge challenge, CancellationToken ct = default);

    // Challenge template methods
    Task<IReadOnlyList<ChallengeTemplate>> GetChallengeTemplatesAsync(ChallengeType? type = null, CancellationToken ct = default);
    Task AddChallengeTemplateAsync(ChallengeTemplate template, CancellationToken ct = default);
    
    // Challenge template admin methods
    Task<(IReadOnlyList<ChallengeTemplate> Items, int TotalCount)> GetChallengeTemplatesPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ChallengeTemplate?> GetChallengeTemplateByIdAsync(int templateId, CancellationToken ct = default);
    Task UpdateChallengeTemplateAsync(ChallengeTemplate template, CancellationToken ct = default);
    Task<bool> IsChallengeTemplateInUseAsync(int templateId, CancellationToken ct = default);
    Task DeleteChallengeTemplateAsync(int templateId, CancellationToken ct = default);

    // Season methods
    Task<Season?> GetCurrentSeasonAsync(CancellationToken ct = default);
    Task<Season?> GetSeasonByIdAsync(int seasonId, CancellationToken ct = default);
    Task<IReadOnlyList<Season>> GetAllSeasonsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SeasonReward>> GetSeasonRewardsAsync(int seasonId, CancellationToken ct = default);
    Task<SeasonReward?> GetSeasonRewardByIdAsync(int seasonRewardId, CancellationToken ct = default);
    Task<IReadOnlyList<int>> GetClaimedRewardIdsAsync(int userId, int seasonId, CancellationToken ct = default);
    Task AddClaimedRewardAsync(int userId, int seasonRewardId, CancellationToken ct = default);
    /// <summary>
    /// Attempts to add a claimed reward, handling unique constraint violations gracefully.
    /// </summary>
    /// <returns>True if the reward was claimed or already claimed; false on other errors.</returns>
    Task<bool> TryAddClaimedRewardAsync(int userId, int seasonRewardId, CancellationToken ct = default);
    Task AddSeasonAsync(Season season, CancellationToken ct = default);
    Task UpdateSeasonAsync(Season season, CancellationToken ct = default);
    Task AddSeasonRewardAsync(SeasonReward reward, CancellationToken ct = default);

    // Season archive methods
    Task AddSeasonArchiveAsync(SeasonArchive archive, CancellationToken ct = default);
    Task<IReadOnlyList<SeasonArchive>> GetUserSeasonArchivesAsync(int userId, CancellationToken ct = default);

    // Cosmetic methods
    Task<IReadOnlyList<Cosmetic>> GetAllCosmeticsAsync(CosmeticType? type = null, CancellationToken ct = default);
    Task<Cosmetic?> GetCosmeticByIdAsync(int cosmeticId, CancellationToken ct = default);
    Task<Cosmetic?> GetCosmeticByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<UserCosmetic>> GetUserCosmeticsAsync(int userId, CancellationToken ct = default);
    Task<UserCosmetic?> GetUserCosmeticAsync(int userId, int cosmeticId, CancellationToken ct = default);
    Task AddUserCosmeticAsync(UserCosmetic userCosmetic, CancellationToken ct = default);
    /// <summary>
    /// Attempts to add a user cosmetic, handling unique constraint violations gracefully.
    /// </summary>
    /// <returns>True if the cosmetic was granted or already owned; false on other errors.</returns>
    Task<bool> TryAddUserCosmeticAsync(UserCosmetic userCosmetic, CancellationToken ct = default);
    
    // Cosmetic admin methods
    Task<(IReadOnlyList<Cosmetic> Items, int TotalCount)> GetCosmeticsPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task AddCosmeticAsync(Cosmetic cosmetic, CancellationToken ct = default);
    Task UpdateCosmeticAsync(Cosmetic cosmetic, CancellationToken ct = default);
    Task<bool> IsCosmeticInUseAsync(int cosmeticId, CancellationToken ct = default);
    Task DeleteCosmeticAsync(int cosmeticId, CancellationToken ct = default);

    // Personal goal methods
    Task<IReadOnlyList<PersonalGoal>> GetActiveGoalsAsync(int userId, CancellationToken ct = default);
    Task<PersonalGoal?> GetGoalByIdAsync(int goalId, CancellationToken ct = default);
    Task AddGoalAsync(PersonalGoal goal, CancellationToken ct = default);
    Task UpdateGoalAsync(PersonalGoal goal, CancellationToken ct = default);

    // Statistics methods
    Task<IReadOnlyList<GameSession>> GetUserGameSessionsAsync(int userId, int? limit = null, CancellationToken ct = default);
    Task<int> GetCompletedChallengeCountAsync(int userId, CancellationToken ct = default);
}
