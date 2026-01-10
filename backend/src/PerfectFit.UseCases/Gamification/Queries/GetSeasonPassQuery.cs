using MediatR;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Gamification.Queries;

/// <summary>
/// Query to get season pass information for a user.
/// </summary>
/// <param name="UserId">The user's external ID (GUID).</param>
public record GetSeasonPassQuery(Guid UserId) : IRequest<SeasonPassResult>;

/// <summary>
/// Handler for getting season pass information.
/// </summary>
public class GetSeasonPassQueryHandler : IRequestHandler<GetSeasonPassQuery, SeasonPassResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ISeasonPassService _seasonPassService;

    public GetSeasonPassQueryHandler(IUserRepository userRepository, ISeasonPassService seasonPassService)
    {
        _userRepository = userRepository;
        _seasonPassService = seasonPassService;
    }

    public async Task<SeasonPassResult> Handle(GetSeasonPassQuery request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromGuid(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException($"User {request.UserId} not found");
        }

        var currentSeason = await _seasonPassService.GetCurrentSeasonAsync(cancellationToken);
        if (currentSeason is null)
        {
            return new SeasonPassResult(SeasonPass: null, HasActiveSeason: false);
        }

        var rewards = await _seasonPassService.GetSeasonRewardsAsync(currentSeason.Id, cancellationToken);

        var rewardInfos = rewards.Select(r => new SeasonRewardInfo(
            RewardId: r.Id,
            Tier: r.Tier,
            RewardType: r.RewardType,
            RewardValue: r.RewardValue,
            XPRequired: r.XPRequired,
            IsClaimed: false, // TODO: Track claimed rewards in a separate table
            CanClaim: user.CurrentSeasonTier >= r.Tier
        )).ToList();

        var seasonPassInfo = new SeasonPassInfo(
            SeasonId: currentSeason.Id,
            SeasonName: currentSeason.Name,
            SeasonNumber: currentSeason.Number,
            CurrentXP: user.SeasonPassXP,
            CurrentTier: user.CurrentSeasonTier,
            EndDate: currentSeason.EndDate,
            Rewards: rewardInfos
        );

        return new SeasonPassResult(SeasonPass: seasonPassInfo, HasActiveSeason: true);
    }

    private static int GetUserIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }
}
