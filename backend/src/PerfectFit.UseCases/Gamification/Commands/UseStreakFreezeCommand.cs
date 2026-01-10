using MediatR;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Gamification.Commands;

/// <summary>
/// Command to use a streak freeze token.
/// </summary>
/// <param name="UserId">The user's external ID (GUID).</param>
public record UseStreakFreezeCommand(Guid UserId) : IRequest<Result<bool>>;

/// <summary>
/// Handler for using streak freeze tokens.
/// </summary>
public class UseStreakFreezeCommandHandler : IRequestHandler<UseStreakFreezeCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IStreakService _streakService;

    public UseStreakFreezeCommandHandler(IUserRepository userRepository, IStreakService streakService)
    {
        _userRepository = userRepository;
        _streakService = streakService;
    }

    public async Task<Result<bool>> Handle(UseStreakFreezeCommand request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromGuid(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<bool>.Failure($"User {request.UserId} not found");
        }

        var success = await _streakService.UseStreakFreezeAsync(user, cancellationToken);
        return Result<bool>.Success(success);
    }

    private static int GetUserIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }
}
