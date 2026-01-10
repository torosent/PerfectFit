using MediatR;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.UseCases.Gamification.Commands;

/// <summary>
/// Command to claim a season reward.
/// </summary>
/// <param name="UserId">The user's external ID (GUID).</param>
/// <param name="SeasonRewardId">The season reward's external ID (GUID).</param>
public record ClaimSeasonRewardCommand(Guid UserId, Guid SeasonRewardId) : IRequest<Result<ClaimRewardResult>>;

/// <summary>
/// Handler for claiming season rewards.
/// </summary>
public class ClaimSeasonRewardCommandHandler : IRequestHandler<ClaimSeasonRewardCommand, Result<ClaimRewardResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly ISeasonPassService _seasonPassService;

    public ClaimSeasonRewardCommandHandler(IUserRepository userRepository, ISeasonPassService seasonPassService)
    {
        _userRepository = userRepository;
        _seasonPassService = seasonPassService;
    }

    public async Task<Result<ClaimRewardResult>> Handle(ClaimSeasonRewardCommand request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromGuid(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<ClaimRewardResult>.Failure($"User {request.UserId} not found");
        }

        var rewardId = GetRewardIdFromGuid(request.SeasonRewardId);
        var result = await _seasonPassService.ClaimRewardAsync(user, rewardId, cancellationToken);

        return Result<ClaimRewardResult>.Success(result);
    }

    private static int GetUserIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }

    private static int GetRewardIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }
}
