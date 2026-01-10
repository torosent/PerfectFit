using MediatR;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Gamification.Queries;

/// <summary>
/// Query to get active challenges with user's progress.
/// </summary>
/// <param name="UserId">The user's external ID (GUID).</param>
/// <param name="Type">Optional filter by challenge type.</param>
public record GetChallengesQuery(Guid UserId, ChallengeType? Type = null) : IRequest<IReadOnlyList<ChallengeWithProgressResult>>;

/// <summary>
/// Handler for getting challenges with progress.
/// </summary>
public class GetChallengesQueryHandler : IRequestHandler<GetChallengesQuery, IReadOnlyList<ChallengeWithProgressResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IChallengeService _challengeService;

    public GetChallengesQueryHandler(IUserRepository userRepository, IChallengeService challengeService)
    {
        _userRepository = userRepository;
        _challengeService = challengeService;
    }

    public async Task<IReadOnlyList<ChallengeWithProgressResult>> Handle(GetChallengesQuery request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromGuid(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException($"User {request.UserId} not found");
        }

        var challenges = await _challengeService.GetActiveChallengesAsync(request.Type, cancellationToken);
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

    private static int GetUserIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }
}
