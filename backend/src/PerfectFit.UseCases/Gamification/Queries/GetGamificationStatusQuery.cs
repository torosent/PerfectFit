using MediatR;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Gamification.Queries;

/// <summary>
/// Query to get comprehensive gamification status for a user.
/// </summary>
/// <param name="UserId">The user's external ID (GUID).</param>
public record GetGamificationStatusQuery(Guid UserId) : IRequest<GamificationStatusResult>;

/// <summary>
/// Handler for getting gamification status.
/// </summary>
public class GetGamificationStatusQueryHandler : IRequestHandler<GetGamificationStatusQuery, GamificationStatusResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IStreakService _streakService;
    private readonly IChallengeService _challengeService;
    private readonly IAchievementService _achievementService;
    private readonly ISeasonPassService _seasonPassService;
    private readonly ICosmeticService _cosmeticService;
    private readonly IPersonalGoalService _personalGoalService;

    public GetGamificationStatusQueryHandler(
        IUserRepository userRepository,
        IStreakService streakService,
        IChallengeService challengeService,
        IAchievementService achievementService,
        ISeasonPassService seasonPassService,
        ICosmeticService cosmeticService,
        IPersonalGoalService personalGoalService)
    {
        _userRepository = userRepository;
        _streakService = streakService;
        _challengeService = challengeService;
        _achievementService = achievementService;
        _seasonPassService = seasonPassService;
        _cosmeticService = cosmeticService;
        _personalGoalService = personalGoalService;
    }

    public async Task<GamificationStatusResult> Handle(GetGamificationStatusQuery request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromGuid(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException($"User {request.UserId} not found");
        }

        var currentTime = DateTimeOffset.UtcNow;

        // Get streak info
        var streakInfo = new StreakInfo(
            CurrentStreak: user.CurrentStreak,
            LongestStreak: user.LongestStreak,
            FreezeTokens: user.StreakFreezeTokens,
            IsAtRisk: _streakService.IsStreakAtRisk(user, currentTime),
            ResetTime: _streakService.GetStreakResetTime(user, currentTime)
        );

        // Get active challenges with progress
        var activeChallenges = await GetActiveChallengesWithProgressAsync(userId, cancellationToken);

        // Get recent achievements (unlocked in last 7 days)
        var recentAchievements = await GetRecentAchievementsAsync(userId, cancellationToken);

        // Get season pass info
        var seasonPass = await GetSeasonPassInfoAsync(user, cancellationToken);

        // Get equipped cosmetics
        var equippedCosmetics = await GetEquippedCosmeticsAsync(user, cancellationToken);

        // Get active personal goals
        var activeGoals = await GetActiveGoalsAsync(userId, cancellationToken);

        return new GamificationStatusResult(
            Streak: streakInfo,
            ActiveChallenges: activeChallenges,
            RecentAchievements: recentAchievements,
            SeasonPass: seasonPass,
            EquippedCosmetics: equippedCosmetics,
            ActiveGoals: activeGoals
        );
    }

    private async Task<IReadOnlyList<ChallengeWithProgressResult>> GetActiveChallengesWithProgressAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var challenges = await _challengeService.GetActiveChallengesAsync(null, cancellationToken);
        var results = new List<ChallengeWithProgressResult>();

        foreach (var challenge in challenges)
        {
            var userChallenge = await _challengeService.GetOrCreateUserChallengeAsync(userId, challenge.Id, cancellationToken);
            results.Add(new ChallengeWithProgressResult(
                ChallengeId: challenge.Id,
                Name: challenge.Name,
                Description: challenge.Description,
                Type: challenge.Type,
                TargetValue: challenge.TargetValue,
                CurrentProgress: userChallenge.CurrentProgress,
                XPReward: challenge.XPReward,
                EndDate: challenge.EndDate,
                IsCompleted: userChallenge.IsCompleted
            ));
        }

        return results;
    }

    private async Task<IReadOnlyList<AchievementInfo>> GetRecentAchievementsAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var userAchievements = await _achievementService.GetUserAchievementsAsync(userId, cancellationToken);
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        return userAchievements
            .Where(ua => ua.UnlockedAt.HasValue && ua.UnlockedAt.Value >= sevenDaysAgo)
            .Select(ua => new AchievementInfo(
                AchievementId: ua.AchievementId,
                Name: ua.Achievement?.Name ?? "Unknown",
                Description: ua.Achievement?.Description ?? "",
                Category: ua.Achievement?.Category ?? Core.Enums.AchievementCategory.Special,
                IconUrl: ua.Achievement?.IconUrl ?? "",
                Progress: ua.Progress,
                IsUnlocked: ua.IsUnlocked,
                UnlockedAt: ua.UnlockedAt,
                IsSecret: ua.Achievement?.IsSecret ?? false
            ))
            .OrderByDescending(a => a.UnlockedAt)
            .Take(5)
            .ToList();
    }

    private async Task<SeasonPassInfo?> GetSeasonPassInfoAsync(User user, CancellationToken cancellationToken)
    {
        var currentSeason = await _seasonPassService.GetCurrentSeasonAsync(cancellationToken);
        if (currentSeason is null)
        {
            return null;
        }

        var rewards = await _seasonPassService.GetSeasonRewardsAsync(currentSeason.Id, cancellationToken);
        var rewardInfos = rewards.Select(r => new SeasonRewardInfo(
            RewardId: r.Id,
            Tier: r.Tier,
            RewardType: r.RewardType,
            RewardValue: r.RewardValue,
            XPRequired: r.XPRequired,
            IsClaimed: false, // TODO: Track claimed rewards
            CanClaim: user.CurrentSeasonTier >= r.Tier
        )).ToList();

        return new SeasonPassInfo(
            SeasonId: currentSeason.Id,
            SeasonName: currentSeason.Name,
            SeasonNumber: currentSeason.Number,
            CurrentXP: user.SeasonPassXP,
            CurrentTier: user.CurrentSeasonTier,
            EndDate: currentSeason.EndDate,
            Rewards: rewardInfos
        );
    }

    private async Task<EquippedCosmeticsInfo> GetEquippedCosmeticsAsync(User user, CancellationToken cancellationToken)
    {
        var allCosmetics = await _cosmeticService.GetAllCosmeticsAsync(null, cancellationToken);

        CosmeticInfo? boardTheme = null;
        CosmeticInfo? avatarFrame = null;
        CosmeticInfo? badge = null;

        if (user.EquippedBoardThemeId.HasValue)
        {
            var cosmetic = allCosmetics.FirstOrDefault(c => c.Id == user.EquippedBoardThemeId.Value);
            if (cosmetic is not null)
            {
                boardTheme = new CosmeticInfo(cosmetic.Id, cosmetic.Name, cosmetic.Type, cosmetic.AssetUrl, cosmetic.Rarity);
            }
        }

        if (user.EquippedAvatarFrameId.HasValue)
        {
            var cosmetic = allCosmetics.FirstOrDefault(c => c.Id == user.EquippedAvatarFrameId.Value);
            if (cosmetic is not null)
            {
                avatarFrame = new CosmeticInfo(cosmetic.Id, cosmetic.Name, cosmetic.Type, cosmetic.AssetUrl, cosmetic.Rarity);
            }
        }

        if (user.EquippedBadgeId.HasValue)
        {
            var cosmetic = allCosmetics.FirstOrDefault(c => c.Id == user.EquippedBadgeId.Value);
            if (cosmetic is not null)
            {
                badge = new CosmeticInfo(cosmetic.Id, cosmetic.Name, cosmetic.Type, cosmetic.AssetUrl, cosmetic.Rarity);
            }
        }

        return new EquippedCosmeticsInfo(BoardTheme: boardTheme, AvatarFrame: avatarFrame, Badge: badge);
    }

    private async Task<IReadOnlyList<PersonalGoalResult>> GetActiveGoalsAsync(int userId, CancellationToken cancellationToken)
    {
        var goals = await _personalGoalService.GetActiveGoalsAsync(userId, cancellationToken);

        return goals.Select(g => new PersonalGoalResult(
            GoalId: g.Id,
            Type: g.Type,
            Description: g.Description,
            TargetValue: g.TargetValue,
            CurrentValue: g.CurrentValue,
            ProgressPercentage: g.TargetValue > 0 ? Math.Min(100, g.CurrentValue * 100 / g.TargetValue) : 0,
            IsCompleted: g.IsCompleted,
            ExpiresAt: g.ExpiresAt
        )).ToList();
    }

    private static int GetUserIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }
}
