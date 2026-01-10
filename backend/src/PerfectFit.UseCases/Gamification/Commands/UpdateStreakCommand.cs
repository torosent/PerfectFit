using MediatR;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.UseCases.Gamification.Commands;

/// <summary>
/// Command to update a user's streak after completing a game.
/// </summary>
/// <param name="UserId">The user's external ID (GUID).</param>
/// <param name="GameEndTime">The time the game ended (UTC).</param>
public record UpdateStreakCommand(Guid UserId, DateTimeOffset GameEndTime) : IRequest<StreakResult>;

/// <summary>
/// Handler for updating user streaks.
/// </summary>
public class UpdateStreakCommandHandler : IRequestHandler<UpdateStreakCommand, StreakResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IStreakService _streakService;

    public UpdateStreakCommandHandler(IUserRepository userRepository, IStreakService streakService)
    {
        _userRepository = userRepository;
        _streakService = streakService;
    }

    public async Task<StreakResult> Handle(UpdateStreakCommand request, CancellationToken cancellationToken)
    {
        // Convert GUID to int ID (assuming first 4 bytes or using a hash)
        var userId = GetUserIdFromGuid(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return new StreakResult(false, 0, 0, false, false, $"User {request.UserId} not found");
        }

        var result = await _streakService.UpdateStreakAsync(user, request.GameEndTime, cancellationToken);
        return result;
    }

    private static int GetUserIdFromGuid(Guid guid)
    {
        // Simple conversion: use first 4 bytes of GUID as int
        // In a real implementation, you might want to look up the user by external ID
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }
}
