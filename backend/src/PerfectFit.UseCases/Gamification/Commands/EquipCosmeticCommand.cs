using MediatR;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.UseCases.Gamification.Commands;

/// <summary>
/// Command to equip a cosmetic item.
/// </summary>
/// <param name="UserId">The user's external ID (GUID).</param>
/// <param name="CosmeticId">The cosmetic's external ID (GUID).</param>
public record EquipCosmeticCommand(Guid UserId, Guid CosmeticId) : IRequest<Result<EquipResult>>;

/// <summary>
/// Handler for equipping cosmetics.
/// </summary>
public class EquipCosmeticCommandHandler : IRequestHandler<EquipCosmeticCommand, Result<EquipResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICosmeticService _cosmeticService;

    public EquipCosmeticCommandHandler(IUserRepository userRepository, ICosmeticService cosmeticService)
    {
        _userRepository = userRepository;
        _cosmeticService = cosmeticService;
    }

    public async Task<Result<EquipResult>> Handle(EquipCosmeticCommand request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromGuid(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<EquipResult>.Failure($"User {request.UserId} not found");
        }

        var cosmeticId = GetCosmeticIdFromGuid(request.CosmeticId);

        // Check if user owns the cosmetic
        var ownsCosmetic = await _cosmeticService.UserOwnsCosmeticAsync(userId, cosmeticId, cancellationToken);
        if (!ownsCosmetic)
        {
            return Result<EquipResult>.Failure("You do not own this cosmetic");
        }

        var result = await _cosmeticService.EquipCosmeticAsync(user, cosmeticId, cancellationToken);
        return Result<EquipResult>.Success(result);
    }

    private static int GetUserIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }

    private static int GetCosmeticIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }
}
