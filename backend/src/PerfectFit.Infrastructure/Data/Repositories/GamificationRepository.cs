using Microsoft.EntityFrameworkCore;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of IGamificationRepository.
/// </summary>
public class GamificationRepository : IGamificationRepository
{
    private readonly AppDbContext _context;

    public GamificationRepository(AppDbContext context)
    {
        _context = context;
    }

    #region Achievement Methods

    public async Task<IReadOnlyList<Achievement>> GetAllAchievementsAsync(CancellationToken ct = default)
    {
        return await _context.Achievements.ToListAsync(ct);
    }

    public async Task<Achievement?> GetAchievementByIdAsync(int achievementId, CancellationToken ct = default)
    {
        return await _context.Achievements.FindAsync([achievementId], ct);
    }

    public async Task<IReadOnlyList<UserAchievement>> GetUserAchievementsAsync(int userId, CancellationToken ct = default)
    {
        return await _context.UserAchievements
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<UserAchievement?> GetUserAchievementAsync(int userId, int achievementId, CancellationToken ct = default)
    {
        return await _context.UserAchievements
            .Include(ua => ua.Achievement)
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId, ct);
    }

    public async Task AddUserAchievementAsync(UserAchievement userAchievement, CancellationToken ct = default)
    {
        _context.UserAchievements.Add(userAchievement);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateUserAchievementAsync(UserAchievement userAchievement, CancellationToken ct = default)
    {
        var entry = _context.Entry(userAchievement);
        if (entry.State == EntityState.Detached)
        {
            _context.UserAchievements.Update(userAchievement);
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<(IReadOnlyList<Achievement> Items, int TotalCount)> GetAchievementsPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Achievements.OrderBy(a => a.DisplayOrder).ThenBy(a => a.Name);
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, totalCount);
    }

    public async Task AddAchievementAsync(Achievement achievement, CancellationToken ct = default)
    {
        _context.Achievements.Add(achievement);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAchievementAsync(Achievement achievement, CancellationToken ct = default)
    {
        var entry = _context.Entry(achievement);
        if (entry.State == EntityState.Detached)
        {
            _context.Achievements.Update(achievement);
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> IsAchievementInUseAsync(int achievementId, CancellationToken ct = default)
    {
        return await _context.UserAchievements.AnyAsync(ua => ua.AchievementId == achievementId, ct);
    }

    public async Task DeleteAchievementAsync(int achievementId, CancellationToken ct = default)
    {
        var achievement = await _context.Achievements.FindAsync([achievementId], ct);
        if (achievement != null)
        {
            _context.Achievements.Remove(achievement);
            await _context.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region Challenge Methods

    public async Task<IReadOnlyList<Challenge>> GetActiveChallengesAsync(ChallengeType? type = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var query = _context.Challenges
            .Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now);

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<Challenge?> GetChallengeByIdAsync(int challengeId, CancellationToken ct = default)
    {
        return await _context.Challenges.FindAsync([challengeId], ct);
    }

    public async Task<IReadOnlyList<UserChallenge>> GetUserChallengesAsync(int userId, IEnumerable<int>? challengeIds = null, CancellationToken ct = default)
    {
        var query = _context.UserChallenges
            .Include(uc => uc.Challenge)
            .Where(uc => uc.UserId == userId);

        if (challengeIds != null)
        {
            var ids = challengeIds.ToList();
            query = query.Where(uc => ids.Contains(uc.ChallengeId));
        }

        return await query.ToListAsync(ct);
    }

    public async Task<UserChallenge?> GetUserChallengeAsync(int userId, int challengeId, CancellationToken ct = default)
    {
        return await _context.UserChallenges
            .Include(uc => uc.Challenge)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId, ct);
    }

    public async Task AddUserChallengeAsync(UserChallenge userChallenge, CancellationToken ct = default)
    {
        _context.UserChallenges.Add(userChallenge);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateUserChallengeAsync(UserChallenge userChallenge, CancellationToken ct = default)
    {
        var entry = _context.Entry(userChallenge);
        if (entry.State == EntityState.Detached)
        {
            _context.UserChallenges.Update(userChallenge);
        }
        await _context.SaveChangesAsync(ct);
    }

    #endregion

    #region Season Methods

    public async Task<Season?> GetCurrentSeasonAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Seasons
            .FirstOrDefaultAsync(s => s.IsActive && s.StartDate <= now && s.EndDate >= now, ct);
    }

    public async Task<Season?> GetSeasonByIdAsync(int seasonId, CancellationToken ct = default)
    {
        return await _context.Seasons.FindAsync([seasonId], ct);
    }

    public async Task<IReadOnlyList<SeasonReward>> GetSeasonRewardsAsync(int seasonId, CancellationToken ct = default)
    {
        return await _context.SeasonRewards
            .Where(sr => sr.SeasonId == seasonId)
            .OrderBy(sr => sr.Tier)
            .ToListAsync(ct);
    }

    public async Task<SeasonReward?> GetSeasonRewardByIdAsync(int seasonRewardId, CancellationToken ct = default)
    {
        return await _context.SeasonRewards.FindAsync([seasonRewardId], ct);
    }

    public async Task<IReadOnlyList<int>> GetClaimedRewardIdsAsync(int userId, int seasonId, CancellationToken ct = default)
    {
        return await _context.ClaimedSeasonRewards
            .Where(csr => csr.UserId == userId && csr.SeasonReward.SeasonId == seasonId)
            .Select(csr => csr.SeasonRewardId)
            .ToListAsync(ct);
    }

    public async Task AddClaimedRewardAsync(int userId, int seasonRewardId, CancellationToken ct = default)
    {
        var claimedReward = ClaimedSeasonReward.Create(userId, seasonRewardId);
        _context.ClaimedSeasonRewards.Add(claimedReward);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> TryAddClaimedRewardAsync(int userId, int seasonRewardId, CancellationToken ct = default)
    {
        try
        {
            var claimedReward = ClaimedSeasonReward.Create(userId, seasonRewardId);
            _context.ClaimedSeasonRewards.Add(claimedReward);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Race condition - already claimed, which is fine
            return true;
        }
    }

    #endregion

    #region Cosmetic Methods

    public async Task<IReadOnlyList<Cosmetic>> GetAllCosmeticsAsync(CosmeticType? type = null, CancellationToken ct = default)
    {
        var query = _context.Cosmetics.AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<Cosmetic?> GetCosmeticByIdAsync(int cosmeticId, CancellationToken ct = default)
    {
        return await _context.Cosmetics.FindAsync([cosmeticId], ct);
    }

    public async Task<Cosmetic?> GetCosmeticByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.Cosmetics.FirstOrDefaultAsync(c => c.Code == code, ct);
    }

    public async Task<IReadOnlyList<UserCosmetic>> GetUserCosmeticsAsync(int userId, CancellationToken ct = default)
    {
        return await _context.UserCosmetics
            .Include(uc => uc.Cosmetic)
            .Where(uc => uc.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<UserCosmetic?> GetUserCosmeticAsync(int userId, int cosmeticId, CancellationToken ct = default)
    {
        return await _context.UserCosmetics
            .Include(uc => uc.Cosmetic)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CosmeticId == cosmeticId, ct);
    }

    public async Task AddUserCosmeticAsync(UserCosmetic userCosmetic, CancellationToken ct = default)
    {
        _context.UserCosmetics.Add(userCosmetic);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> TryAddUserCosmeticAsync(UserCosmetic userCosmetic, CancellationToken ct = default)
    {
        try
        {
            _context.UserCosmetics.Add(userCosmetic);
            await _context.SaveChangesAsync(ct);
            // Cosmetic was successfully granted.
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Race condition - user already owns this cosmetic; no new grant occurred.
            return false;
        }
    }

    public async Task<(IReadOnlyList<Cosmetic> Items, int TotalCount)> GetCosmeticsPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Cosmetics.OrderBy(c => c.Type).ThenBy(c => c.Name);
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, totalCount);
    }

    public async Task AddCosmeticAsync(Cosmetic cosmetic, CancellationToken ct = default)
    {
        _context.Cosmetics.Add(cosmetic);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateCosmeticAsync(Cosmetic cosmetic, CancellationToken ct = default)
    {
        var entry = _context.Entry(cosmetic);
        if (entry.State == EntityState.Detached)
        {
            _context.Cosmetics.Update(cosmetic);
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> IsCosmeticInUseAsync(int cosmeticId, CancellationToken ct = default)
    {
        // Check if any user owns this cosmetic
        var ownedByUser = await _context.UserCosmetics.AnyAsync(uc => uc.CosmeticId == cosmeticId, ct);
        if (ownedByUser)
        {
            return true;
        }

        // Check if any achievement references this cosmetic via RewardCosmeticCode
        var cosmetic = await _context.Cosmetics.FindAsync([cosmeticId], ct);
        if (cosmetic?.Code != null)
        {
            var cosmeticCode = cosmetic.Code;
            var referencedByAchievement = await _context.Achievements.AnyAsync(
                a => a.RewardCosmeticCode == cosmeticCode, ct);
            if (referencedByAchievement)
            {
                return true;
            }
        }

        return false;
    }

    public async Task DeleteCosmeticAsync(int cosmeticId, CancellationToken ct = default)
    {
        var cosmetic = await _context.Cosmetics.FindAsync([cosmeticId], ct);
        if (cosmetic != null)
        {
            _context.Cosmetics.Remove(cosmetic);
            await _context.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region Personal Goal Methods

    public async Task<IReadOnlyList<PersonalGoal>> GetActiveGoalsAsync(int userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.PersonalGoals
            .Where(g => g.UserId == userId && !g.IsCompleted && (!g.ExpiresAt.HasValue || g.ExpiresAt.Value > now))
            .ToListAsync(ct);
    }

    public async Task<PersonalGoal?> GetGoalByIdAsync(int goalId, CancellationToken ct = default)
    {
        return await _context.PersonalGoals.FindAsync([goalId], ct);
    }

    public async Task AddGoalAsync(PersonalGoal goal, CancellationToken ct = default)
    {
        _context.PersonalGoals.Add(goal);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateGoalAsync(PersonalGoal goal, CancellationToken ct = default)
    {
        var entry = _context.Entry(goal);
        if (entry.State == EntityState.Detached)
        {
            _context.PersonalGoals.Update(goal);
        }
        await _context.SaveChangesAsync(ct);
    }

    #endregion

    #region Statistics Methods

    public async Task<IReadOnlyList<GameSession>> GetUserGameSessionsAsync(int userId, int? limit = null, CancellationToken ct = default)
    {
        var query = _context.GameSessions
            .Where(gs => gs.UserId == userId && gs.Status == GameStatus.Ended)
            .OrderByDescending(gs => gs.EndedAt);

        if (limit.HasValue)
        {
            return await query.Take(limit.Value).ToListAsync(ct);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<int> GetCompletedChallengeCountAsync(int userId, CancellationToken ct = default)
    {
        return await _context.UserChallenges
            .CountAsync(uc => uc.UserId == userId && uc.IsCompleted, ct);
    }

    #endregion

    #region Challenge Management Methods

    public async Task AddChallengeAsync(Challenge challenge, CancellationToken ct = default)
    {
        _context.Challenges.Add(challenge);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateChallengeAsync(Challenge challenge, CancellationToken ct = default)
    {
        var entry = _context.Entry(challenge);
        if (entry.State == EntityState.Detached)
        {
            _context.Challenges.Update(challenge);
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ChallengeTemplate>> GetChallengeTemplatesAsync(ChallengeType? type = null, CancellationToken ct = default)
    {
        var query = _context.ChallengeTemplates.Where(t => t.IsActive);

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        return await query.ToListAsync(ct);
    }

    public async Task AddChallengeTemplateAsync(ChallengeTemplate template, CancellationToken ct = default)
    {
        _context.ChallengeTemplates.Add(template);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<(IReadOnlyList<ChallengeTemplate> Items, int TotalCount)> GetChallengeTemplatesPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.ChallengeTemplates.OrderBy(t => t.Type).ThenBy(t => t.Name);
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, totalCount);
    }

    public async Task<ChallengeTemplate?> GetChallengeTemplateByIdAsync(int templateId, CancellationToken ct = default)
    {
        return await _context.ChallengeTemplates.FindAsync([templateId], ct);
    }

    public async Task UpdateChallengeTemplateAsync(ChallengeTemplate template, CancellationToken ct = default)
    {
        var entry = _context.Entry(template);
        if (entry.State == EntityState.Detached)
        {
            _context.ChallengeTemplates.Update(template);
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> IsChallengeTemplateInUseAsync(int templateId, CancellationToken ct = default)
    {
        return await _context.Challenges.AnyAsync(c => c.ChallengeTemplateId == templateId, ct);
    }

    public async Task DeleteChallengeTemplateAsync(int templateId, CancellationToken ct = default)
    {
        var template = await _context.ChallengeTemplates.FindAsync([templateId], ct);
        if (template != null)
        {
            _context.ChallengeTemplates.Remove(template);
            await _context.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region Season Management Methods

    public async Task<IReadOnlyList<Season>> GetAllSeasonsAsync(CancellationToken ct = default)
    {
        return await _context.Seasons.ToListAsync(ct);
    }

    public async Task AddSeasonAsync(Season season, CancellationToken ct = default)
    {
        _context.Seasons.Add(season);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateSeasonAsync(Season season, CancellationToken ct = default)
    {
        var entry = _context.Entry(season);
        if (entry.State == EntityState.Detached)
        {
            _context.Seasons.Update(season);
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddSeasonRewardAsync(SeasonReward reward, CancellationToken ct = default)
    {
        _context.SeasonRewards.Add(reward);
        await _context.SaveChangesAsync(ct);
    }

    #endregion

    #region Season Archive Methods

    public async Task AddSeasonArchiveAsync(SeasonArchive archive, CancellationToken ct = default)
    {
        _context.SeasonArchives.Add(archive);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SeasonArchive>> GetUserSeasonArchivesAsync(int userId, CancellationToken ct = default)
    {
        return await _context.SeasonArchives
            .Where(sa => sa.UserId == userId)
            .Include(sa => sa.Season)
            .OrderByDescending(sa => sa.ArchivedAt)
            .ToListAsync(ct);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if the exception is a unique constraint violation (duplicate key).
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var innerException = ex.InnerException;
        if (innerException == null) return false;

        var message = innerException.Message;
        return message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("UNIQUE constraint", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("23505"); // PostgreSQL error code
    }

    #endregion
}
