using System.Collections.Concurrent;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.Infrastructure.Data.InMemory;

/// <summary>
/// In-memory implementation of IGamificationRepository for testing without a database.
/// </summary>
public class InMemoryGamificationRepository : IGamificationRepository
{
    private readonly ConcurrentDictionary<int, Achievement> _achievements = new();
    private readonly ConcurrentDictionary<int, UserAchievement> _userAchievements = new();
    private readonly ConcurrentDictionary<int, Challenge> _challenges = new();
    private readonly ConcurrentDictionary<int, UserChallenge> _userChallenges = new();
    private readonly ConcurrentDictionary<int, ChallengeTemplate> _challengeTemplates = new();
    private readonly ConcurrentDictionary<int, Season> _seasons = new();
    private readonly ConcurrentDictionary<int, SeasonReward> _seasonRewards = new();
    private readonly ConcurrentDictionary<int, SeasonArchive> _seasonArchives = new();
    private readonly ConcurrentDictionary<(int userId, int rewardId), bool> _claimedRewards = new();
    private readonly ConcurrentDictionary<int, Cosmetic> _cosmetics = new();
    private readonly ConcurrentDictionary<int, UserCosmetic> _userCosmetics = new();
    private readonly ConcurrentDictionary<int, PersonalGoal> _personalGoals = new();
    private readonly ConcurrentDictionary<(int userId, Guid sessionId), GameSession> _gameSessions = new();

    private int _nextAchievementId = 1;
    private int _nextUserAchievementId = 1;
    private int _nextChallengeId = 1;
    private int _nextUserChallengeId = 1;
    private int _nextChallengeTemplateId = 1;
    private int _nextSeasonId = 1;
    private int _nextSeasonRewardId = 1;
    private int _nextSeasonArchiveId = 1;
    private int _nextCosmeticId = 1;
    private int _nextUserCosmeticId = 1;
    private int _nextGoalId = 1;

    #region Achievement Methods

    public Task<IReadOnlyList<Achievement>> GetAllAchievementsAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<Achievement>>(_achievements.Values.ToList());
    }

    public Task<Achievement?> GetAchievementByIdAsync(int achievementId, CancellationToken ct = default)
    {
        _achievements.TryGetValue(achievementId, out var achievement);
        return Task.FromResult(achievement);
    }

    public Task<IReadOnlyList<UserAchievement>> GetUserAchievementsAsync(int userId, CancellationToken ct = default)
    {
        var achievements = _userAchievements.Values.Where(ua => ua.UserId == userId).ToList();
        return Task.FromResult<IReadOnlyList<UserAchievement>>(achievements);
    }

    public Task<UserAchievement?> GetUserAchievementAsync(int userId, int achievementId, CancellationToken ct = default)
    {
        var achievement = _userAchievements.Values.FirstOrDefault(ua => ua.UserId == userId && ua.AchievementId == achievementId);
        return Task.FromResult(achievement);
    }

    public Task AddUserAchievementAsync(UserAchievement userAchievement, CancellationToken ct = default)
    {
        var id = Interlocked.Increment(ref _nextUserAchievementId);
        SetProperty(userAchievement, "Id", id);
        _userAchievements[id] = userAchievement;
        return Task.CompletedTask;
    }

    public Task UpdateUserAchievementAsync(UserAchievement userAchievement, CancellationToken ct = default)
    {
        _userAchievements[userAchievement.Id] = userAchievement;
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<Achievement> Items, int TotalCount)> GetAchievementsPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _achievements.Values.OrderBy(a => a.DisplayOrder).ThenBy(a => a.Name).ToList();
        var totalCount = query.Count;
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<Achievement> Items, int TotalCount)>((items, totalCount));
    }

    public Task AddAchievementAsync(Achievement achievement, CancellationToken ct = default)
    {
        var id = Interlocked.Increment(ref _nextAchievementId);
        SetProperty(achievement, "Id", id);
        _achievements[id] = achievement;
        return Task.CompletedTask;
    }

    public Task UpdateAchievementAsync(Achievement achievement, CancellationToken ct = default)
    {
        _achievements[achievement.Id] = achievement;
        return Task.CompletedTask;
    }

    public Task<bool> IsAchievementInUseAsync(int achievementId, CancellationToken ct = default)
    {
        var inUse = _userAchievements.Values.Any(ua => ua.AchievementId == achievementId);
        return Task.FromResult(inUse);
    }

    public Task DeleteAchievementAsync(int achievementId, CancellationToken ct = default)
    {
        _achievements.TryRemove(achievementId, out _);
        return Task.CompletedTask;
    }

    #endregion

    #region Challenge Methods

    public Task<IReadOnlyList<Challenge>> GetActiveChallengesAsync(ChallengeType? type = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var challenges = _challenges.Values
            .Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now)
            .Where(c => type == null || c.Type == type)
            .ToList();
        return Task.FromResult<IReadOnlyList<Challenge>>(challenges);
    }

    public Task<Challenge?> GetChallengeByIdAsync(int challengeId, CancellationToken ct = default)
    {
        _challenges.TryGetValue(challengeId, out var challenge);
        return Task.FromResult(challenge);
    }

    public Task<IReadOnlyList<UserChallenge>> GetUserChallengesAsync(int userId, IEnumerable<int>? challengeIds = null, CancellationToken ct = default)
    {
        var challenges = _userChallenges.Values.Where(uc => uc.UserId == userId);
        if (challengeIds != null)
        {
            var ids = challengeIds.ToHashSet();
            challenges = challenges.Where(uc => ids.Contains(uc.ChallengeId));
        }
        return Task.FromResult<IReadOnlyList<UserChallenge>>(challenges.ToList());
    }

    public Task<UserChallenge?> GetUserChallengeAsync(int userId, int challengeId, CancellationToken ct = default)
    {
        var challenge = _userChallenges.Values.FirstOrDefault(uc => uc.UserId == userId && uc.ChallengeId == challengeId);
        return Task.FromResult(challenge);
    }

    public Task AddUserChallengeAsync(UserChallenge userChallenge, CancellationToken ct = default)
    {
        var id = Interlocked.Increment(ref _nextUserChallengeId);
        SetProperty(userChallenge, "Id", id);
        _userChallenges[id] = userChallenge;
        return Task.CompletedTask;
    }

    public Task UpdateUserChallengeAsync(UserChallenge userChallenge, CancellationToken ct = default)
    {
        _userChallenges[userChallenge.Id] = userChallenge;
        return Task.CompletedTask;
    }

    public Task AddChallengeAsync(Challenge challenge, CancellationToken ct = default)
    {
        var id = Interlocked.Increment(ref _nextChallengeId);
        SetProperty(challenge, "Id", id);
        _challenges[id] = challenge;
        return Task.CompletedTask;
    }

    public Task UpdateChallengeAsync(Challenge challenge, CancellationToken ct = default)
    {
        _challenges[challenge.Id] = challenge;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ChallengeTemplate>> GetChallengeTemplatesAsync(ChallengeType? type = null, CancellationToken ct = default)
    {
        var templates = _challengeTemplates.Values.Where(t => t.IsActive);
        if (type.HasValue)
        {
            templates = templates.Where(t => t.Type == type.Value);
        }
        return Task.FromResult<IReadOnlyList<ChallengeTemplate>>(templates.ToList());
    }

    public Task AddChallengeTemplateAsync(ChallengeTemplate template, CancellationToken ct = default)
    {
        var id = Interlocked.Increment(ref _nextChallengeTemplateId);
        SetProperty(template, "Id", id);
        _challengeTemplates[id] = template;
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<ChallengeTemplate> Items, int TotalCount)> GetChallengeTemplatesPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _challengeTemplates.Values.OrderBy(t => t.Type).ThenBy(t => t.Name).ToList();
        var totalCount = query.Count;
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<ChallengeTemplate> Items, int TotalCount)>((items, totalCount));
    }

    public Task<ChallengeTemplate?> GetChallengeTemplateByIdAsync(int templateId, CancellationToken ct = default)
    {
        _challengeTemplates.TryGetValue(templateId, out var template);
        return Task.FromResult(template);
    }

    public Task UpdateChallengeTemplateAsync(ChallengeTemplate template, CancellationToken ct = default)
    {
        _challengeTemplates[template.Id] = template;
        return Task.CompletedTask;
    }

    public Task<bool> IsChallengeTemplateInUseAsync(int templateId, CancellationToken ct = default)
    {
        var inUse = _challenges.Values.Any(c => c.ChallengeTemplateId == templateId);
        return Task.FromResult(inUse);
    }

    public Task DeleteChallengeTemplateAsync(int templateId, CancellationToken ct = default)
    {
        _challengeTemplates.TryRemove(templateId, out _);
        return Task.CompletedTask;
    }

    #endregion

    #region Season Methods

    public Task<Season?> GetCurrentSeasonAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var season = _seasons.Values.FirstOrDefault(s => s.IsActive && s.StartDate <= now && s.EndDate >= now);
        return Task.FromResult(season);
    }

    public Task<Season?> GetSeasonByIdAsync(int seasonId, CancellationToken ct = default)
    {
        _seasons.TryGetValue(seasonId, out var season);
        return Task.FromResult(season);
    }

    public Task<IReadOnlyList<Season>> GetAllSeasonsAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<Season>>(_seasons.Values.ToList());
    }

    public Task<IReadOnlyList<SeasonReward>> GetSeasonRewardsAsync(int seasonId, CancellationToken ct = default)
    {
        var rewards = _seasonRewards.Values.Where(sr => sr.SeasonId == seasonId).ToList();
        return Task.FromResult<IReadOnlyList<SeasonReward>>(rewards);
    }

    public Task<SeasonReward?> GetSeasonRewardByIdAsync(int seasonRewardId, CancellationToken ct = default)
    {
        _seasonRewards.TryGetValue(seasonRewardId, out var reward);
        return Task.FromResult(reward);
    }

    public Task<IReadOnlyList<int>> GetClaimedRewardIdsAsync(int userId, int seasonId, CancellationToken ct = default)
    {
        var claimed = _claimedRewards
            .Where(kvp => kvp.Key.userId == userId && _seasonRewards.TryGetValue(kvp.Key.rewardId, out var r) && r.SeasonId == seasonId)
            .Select(kvp => kvp.Key.rewardId)
            .ToList();
        return Task.FromResult<IReadOnlyList<int>>(claimed);
    }

    public Task AddClaimedRewardAsync(int userId, int seasonRewardId, CancellationToken ct = default)
    {
        _claimedRewards[(userId, seasonRewardId)] = true;
        return Task.CompletedTask;
    }

    public Task<bool> TryAddClaimedRewardAsync(int userId, int seasonRewardId, CancellationToken ct = default)
    {
        // In-memory implementation doesn't throw on duplicates, it just overwrites
        _claimedRewards[(userId, seasonRewardId)] = true;
        return Task.FromResult(true);
    }

    public Task AddSeasonAsync(Season season, CancellationToken ct = default)
    {
        var id = Interlocked.Increment(ref _nextSeasonId);
        SetProperty(season, "Id", id);
        _seasons[id] = season;
        return Task.CompletedTask;
    }

    public Task UpdateSeasonAsync(Season season, CancellationToken ct = default)
    {
        _seasons[season.Id] = season;
        return Task.CompletedTask;
    }

    public Task AddSeasonRewardAsync(SeasonReward reward, CancellationToken ct = default)
    {
        var id = Interlocked.Increment(ref _nextSeasonRewardId);
        SetProperty(reward, "Id", id);
        _seasonRewards[id] = reward;
        return Task.CompletedTask;
    }

    public Task AddSeasonArchiveAsync(SeasonArchive archive, CancellationToken ct = default)
    {
        var id = Interlocked.Increment(ref _nextSeasonArchiveId);
        SetProperty(archive, "Id", id);
        _seasonArchives[id] = archive;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SeasonArchive>> GetUserSeasonArchivesAsync(int userId, CancellationToken ct = default)
    {
        var archives = _seasonArchives.Values
            .Where(sa => sa.UserId == userId)
            .OrderByDescending(sa => sa.ArchivedAt)
            .ToList();
        return Task.FromResult<IReadOnlyList<SeasonArchive>>(archives);
    }

    #endregion

    #region Cosmetic Methods

    public Task<IReadOnlyList<Cosmetic>> GetAllCosmeticsAsync(CosmeticType? type = null, CancellationToken ct = default)
    {
        var cosmetics = type == null
            ? _cosmetics.Values.ToList()
            : _cosmetics.Values.Where(c => c.Type == type).ToList();
        return Task.FromResult<IReadOnlyList<Cosmetic>>(cosmetics);
    }

    public Task<Cosmetic?> GetCosmeticByIdAsync(int cosmeticId, CancellationToken ct = default)
    {
        _cosmetics.TryGetValue(cosmeticId, out var cosmetic);
        return Task.FromResult(cosmetic);
    }

    public Task<Cosmetic?> GetCosmeticByCodeAsync(string code, CancellationToken ct = default)
    {
        var cosmetic = _cosmetics.Values.FirstOrDefault(c => c.Code == code);
        return Task.FromResult(cosmetic);
    }

    public Task<IReadOnlyList<UserCosmetic>> GetUserCosmeticsAsync(int userId, CancellationToken ct = default)
    {
        var cosmetics = _userCosmetics.Values.Where(uc => uc.UserId == userId).ToList();
        return Task.FromResult<IReadOnlyList<UserCosmetic>>(cosmetics);
    }

    public Task<UserCosmetic?> GetUserCosmeticAsync(int userId, int cosmeticId, CancellationToken ct = default)
    {
        var cosmetic = _userCosmetics.Values.FirstOrDefault(uc => uc.UserId == userId && uc.CosmeticId == cosmeticId);
        return Task.FromResult(cosmetic);
    }

    public Task AddUserCosmeticAsync(UserCosmetic userCosmetic, CancellationToken ct = default)
    {
        var id = Interlocked.Increment(ref _nextUserCosmeticId);
        SetProperty(userCosmetic, "Id", id);
        _userCosmetics[id] = userCosmetic;
        return Task.CompletedTask;
    }

    public Task<bool> TryAddUserCosmeticAsync(UserCosmetic userCosmetic, CancellationToken ct = default)
    {
        // Check if already exists (in-memory doesn't have unique constraints, so we check manually)
        var existing = _userCosmetics.Values.FirstOrDefault(uc => uc.UserId == userCosmetic.UserId && uc.CosmeticId == userCosmetic.CosmeticId);
        if (existing != null)
        {
            return Task.FromResult(true); // Already owned
        }

        var id = Interlocked.Increment(ref _nextUserCosmeticId);
        SetProperty(userCosmetic, "Id", id);
        _userCosmetics[id] = userCosmetic;
        return Task.FromResult(true);
    }

    public Task<(IReadOnlyList<Cosmetic> Items, int TotalCount)> GetCosmeticsPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _cosmetics.Values.OrderBy(c => c.Type).ThenBy(c => c.Name).ToList();
        var totalCount = query.Count;
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<Cosmetic> Items, int TotalCount)>((items, totalCount));
    }

    public Task AddCosmeticAsync(Cosmetic cosmetic, CancellationToken ct = default)
    {
        var id = Interlocked.Increment(ref _nextCosmeticId);
        SetProperty(cosmetic, "Id", id);
        _cosmetics[id] = cosmetic;
        return Task.CompletedTask;
    }

    public Task UpdateCosmeticAsync(Cosmetic cosmetic, CancellationToken ct = default)
    {
        _cosmetics[cosmetic.Id] = cosmetic;
        return Task.CompletedTask;
    }

    public Task<bool> IsCosmeticInUseAsync(int cosmeticId, CancellationToken ct = default)
    {
        // Check if any user owns this cosmetic
        var ownedByUser = _userCosmetics.Values.Any(uc => uc.CosmeticId == cosmeticId);
        if (ownedByUser)
        {
            return Task.FromResult(true);
        }

        // Check if any achievement references this cosmetic via RewardCosmeticCode
        if (_cosmetics.TryGetValue(cosmeticId, out var cosmetic) && !string.IsNullOrEmpty(cosmetic.Code))
        {
            var referencedByAchievement = _achievements.Values.Any(a => a.RewardCosmeticCode == cosmetic.Code);
            if (referencedByAchievement)
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public Task DeleteCosmeticAsync(int cosmeticId, CancellationToken ct = default)
    {
        _cosmetics.TryRemove(cosmeticId, out _);
        return Task.CompletedTask;
    }

    #endregion

    #region Personal Goal Methods

    public Task<IReadOnlyList<PersonalGoal>> GetActiveGoalsAsync(int userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var goals = _personalGoals.Values
            .Where(g => g.UserId == userId && !g.IsCompleted && (!g.ExpiresAt.HasValue || g.ExpiresAt.Value > now))
            .ToList();
        return Task.FromResult<IReadOnlyList<PersonalGoal>>(goals);
    }

    public Task<PersonalGoal?> GetGoalByIdAsync(int goalId, CancellationToken ct = default)
    {
        _personalGoals.TryGetValue(goalId, out var goal);
        return Task.FromResult(goal);
    }

    public Task AddGoalAsync(PersonalGoal goal, CancellationToken ct = default)
    {
        var id = Interlocked.Increment(ref _nextGoalId);
        SetProperty(goal, "Id", id);
        _personalGoals[id] = goal;
        return Task.CompletedTask;
    }

    public Task UpdateGoalAsync(PersonalGoal goal, CancellationToken ct = default)
    {
        _personalGoals[goal.Id] = goal;
        return Task.CompletedTask;
    }

    #endregion

    #region Statistics Methods

    public Task<IReadOnlyList<GameSession>> GetUserGameSessionsAsync(int userId, int? limit = null, CancellationToken ct = default)
    {
        var sessions = _gameSessions.Values
            .Where(gs => gs.UserId == userId && gs.Status == GameStatus.Ended)
            .OrderByDescending(gs => gs.EndedAt)
            .ToList();

        if (limit.HasValue)
        {
            sessions = sessions.Take(limit.Value).ToList();
        }

        return Task.FromResult<IReadOnlyList<GameSession>>(sessions);
    }

    public Task<int> GetCompletedChallengeCountAsync(int userId, CancellationToken ct = default)
    {
        var count = _userChallenges.Values.Count(uc => uc.UserId == userId && uc.IsCompleted);
        return Task.FromResult(count);
    }

    #endregion

    #region Helper Methods for Seeding Test Data

    public void AddAchievement(Achievement achievement)
    {
        var id = achievement.Id != 0 ? achievement.Id : Interlocked.Increment(ref _nextAchievementId);
        SetProperty(achievement, "Id", id);
        _achievements[id] = achievement;
    }

    public void AddChallenge(Challenge challenge)
    {
        var id = challenge.Id != 0 ? challenge.Id : Interlocked.Increment(ref _nextChallengeId);
        SetProperty(challenge, "Id", id);
        _challenges[id] = challenge;
    }

    public void AddSeason(Season season)
    {
        var id = season.Id != 0 ? season.Id : Interlocked.Increment(ref _nextSeasonId);
        SetProperty(season, "Id", id);
        _seasons[id] = season;
    }

    public void AddSeasonReward(SeasonReward reward)
    {
        var id = reward.Id != 0 ? reward.Id : Interlocked.Increment(ref _nextSeasonRewardId);
        SetProperty(reward, "Id", id);
        _seasonRewards[id] = reward;
    }

    public void AddCosmetic(Cosmetic cosmetic)
    {
        var id = cosmetic.Id != 0 ? cosmetic.Id : Interlocked.Increment(ref _nextCosmeticId);
        SetProperty(cosmetic, "Id", id);
        _cosmetics[id] = cosmetic;
    }

    public void AddChallengeTemplateSync(ChallengeTemplate template)
    {
        var id = template.Id != 0 ? template.Id : Interlocked.Increment(ref _nextChallengeTemplateId);
        SetProperty(template, "Id", id);
        _challengeTemplates[id] = template;
    }

    public void AddGameSession(GameSession session)
    {
        _gameSessions[(session.UserId ?? 0, session.Id)] = session;
    }

    private static void SetProperty<T, TValue>(T obj, string propertyName, TValue value) where T : class
    {
        var backingField = typeof(T).GetField($"<{propertyName}>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        backingField?.SetValue(obj, value);
    }

    #endregion
}
