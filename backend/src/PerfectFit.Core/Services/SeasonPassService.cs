using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.Core.Services;

/// <summary>
/// Service for managing season passes, XP, and rewards.
/// </summary>
public class SeasonPassService : ISeasonPassService
{
    private readonly IGamificationRepository _repository;
    private readonly ICosmeticService _cosmeticService;
    private readonly IUserRepository _userRepository;

    // Tier XP thresholds: 100, 250, 500, 800, 1200, 1700, 2300, 3000, 4000, 5000
    private static readonly int[] TierThresholds = { 100, 250, 500, 800, 1200, 1700, 2300, 3000, 4000, 5000 };

    public SeasonPassService(
        IGamificationRepository repository,
        ICosmeticService cosmeticService,
        IUserRepository userRepository)
    {
        _repository = repository;
        _cosmeticService = cosmeticService;
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<Season?> GetCurrentSeasonAsync(CancellationToken ct = default)
    {
        return await _repository.GetCurrentSeasonAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SeasonReward>> GetSeasonRewardsAsync(int seasonId, CancellationToken ct = default)
    {
        return await _repository.GetSeasonRewardsAsync(seasonId, ct);
    }

    /// <inheritdoc />
    public async Task<SeasonXPResult> AddXPAsync(User user, int xpAmount, string source, CancellationToken ct = default)
    {
        int previousTier = user.CurrentSeasonTier;
        user.AddSeasonXP(xpAmount);

        // Recalculate tier based on our thresholds (user.AddSeasonXP uses different calculation)
        int newTier = CalculateTierFromXP(user.SeasonPassXP);

        // Update the tier if our calculation differs
        if (newTier != user.CurrentSeasonTier)
        {
            // Use reflection to update since there's no public setter
            var tierField = typeof(User).GetField("<CurrentSeasonTier>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            tierField?.SetValue(user, newTier);
        }

        await _userRepository.UpdateAsync(user, ct);

        bool tierUp = newTier > previousTier;
        int rewardsAvailable = 0;

        if (tierUp)
        {
            var season = await _repository.GetCurrentSeasonAsync(ct);
            if (season != null)
            {
                var rewards = await _repository.GetSeasonRewardsAsync(season.Id, ct);
                var claimedIds = await _repository.GetClaimedRewardIdsAsync(user.Id, season.Id, ct);
                rewardsAvailable = rewards.Count(r => r.Tier <= newTier && !claimedIds.Contains(r.Id));
            }
        }

        return new SeasonXPResult(
            Success: true,
            XPEarned: xpAmount,
            NewXP: user.SeasonPassXP,
            NewTier: newTier,
            TierUp: tierUp,
            RewardsAvailable: rewardsAvailable);
    }

    /// <inheritdoc />
    public async Task<ClaimRewardResult> ClaimRewardAsync(User user, int seasonRewardId, CancellationToken ct = default)
    {
        var reward = await _repository.GetSeasonRewardByIdAsync(seasonRewardId, ct);
        if (reward == null)
        {
            return new ClaimRewardResult(false, null, null, "Reward not found.");
        }

        // Check if user has reached the required tier
        int userTier = CalculateTierFromXP(user.SeasonPassXP);
        if (userTier < reward.Tier)
        {
            return new ClaimRewardResult(false, null, null, $"You have not reached the required tier ({reward.Tier}) for this reward.");
        }

        // Check if already claimed
        var claimedIds = await _repository.GetClaimedRewardIdsAsync(user.Id, reward.SeasonId, ct);
        if (claimedIds.Contains(seasonRewardId))
        {
            return new ClaimRewardResult(false, null, null, "This reward has already been claimed.");
        }

        // Grant the reward based on type
        switch (reward.RewardType)
        {
            case RewardType.Cosmetic:
                var cosmeticGranted = await _cosmeticService.GrantCosmeticAsync(user, reward.RewardValue, ObtainedFrom.SeasonPass, ct);
                if (!cosmeticGranted)
                {
                    return new ClaimRewardResult(false, null, null, "Failed to grant cosmetic reward.");
                }
                break;

            case RewardType.StreakFreeze:
                user.AddStreakFreezeTokens(reward.RewardValue);
                await _userRepository.UpdateAsync(user, ct);
                break;

            case RewardType.XPBoost:
                // XP boosts could be stored or applied immediately
                // For now, we'll just record the claim
                break;
        }

        // TryAddClaimedRewardAsync handles unique constraint violations
        await _repository.TryAddClaimedRewardAsync(user.Id, seasonRewardId, ct);

        return new ClaimRewardResult(
            Success: true,
            RewardType: reward.RewardType,
            RewardValue: reward.RewardValue);
    }

    /// <inheritdoc />
    public int CalculateTierFromXP(int totalXP)
    {
        for (int i = TierThresholds.Length - 1; i >= 0; i--)
        {
            if (totalXP >= TierThresholds[i])
            {
                return i + 1;
            }
        }
        return 0;
    }
}
