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
        _context.UserAchievements.Update(userAchievement);
        await _context.SaveChangesAsync(ct);
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
        _context.UserChallenges.Update(userChallenge);
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
        _context.PersonalGoals.Update(goal);
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
}
