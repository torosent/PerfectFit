using MediatR;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Gamification.Queries;

/// <summary>
/// Query to get a user's active personal goals.
/// </summary>
/// <param name="UserId">The user's external ID (GUID).</param>
public record GetPersonalGoalsQuery(Guid UserId) : IRequest<IReadOnlyList<PersonalGoalResult>>;

/// <summary>
/// Handler for getting personal goals.
/// </summary>
public class GetPersonalGoalsQueryHandler : IRequestHandler<GetPersonalGoalsQuery, IReadOnlyList<PersonalGoalResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPersonalGoalService _personalGoalService;

    public GetPersonalGoalsQueryHandler(IUserRepository userRepository, IPersonalGoalService personalGoalService)
    {
        _userRepository = userRepository;
        _personalGoalService = personalGoalService;
    }

    public async Task<IReadOnlyList<PersonalGoalResult>> Handle(GetPersonalGoalsQuery request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromGuid(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException($"User {request.UserId} not found");
        }

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
