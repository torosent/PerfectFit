using MediatR;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Gamification.Queries;

/// <summary>
/// Query to get all cosmetics with ownership status.
/// </summary>
/// <param name="UserId">The user's external ID (GUID).</param>
/// <param name="Type">Optional filter by cosmetic type.</param>
public record GetCosmeticsQuery(Guid UserId, CosmeticType? Type = null) : IRequest<CosmeticsResult>;

/// <summary>
/// Handler for getting cosmetics with ownership status.
/// </summary>
public class GetCosmeticsQueryHandler : IRequestHandler<GetCosmeticsQuery, CosmeticsResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ICosmeticService _cosmeticService;

    public GetCosmeticsQueryHandler(IUserRepository userRepository, ICosmeticService cosmeticService)
    {
        _userRepository = userRepository;
        _cosmeticService = cosmeticService;
    }

    public async Task<CosmeticsResult> Handle(GetCosmeticsQuery request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromGuid(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException($"User {request.UserId} not found");
        }

        var allCosmetics = await _cosmeticService.GetAllCosmeticsAsync(request.Type, cancellationToken);
        var userCosmetics = await _cosmeticService.GetUserCosmeticsAsync(userId, cancellationToken);

        var ownedCosmeticIds = userCosmetics.Select(uc => uc.CosmeticId).ToHashSet();

        var cosmeticsWithOwnership = allCosmetics.Select(c =>
        {
            var isOwned = c.IsDefault || ownedCosmeticIds.Contains(c.Id);
            var isEquipped = IsEquipped(user, c.Id, c.Type);

            return new CosmeticWithOwnershipResult(
                CosmeticId: c.Id,
                Name: c.Name,
                Description: c.Description,
                Type: c.Type,
                AssetUrl: c.AssetUrl,
                PreviewUrl: c.PreviewUrl,
                Rarity: c.Rarity,
                IsOwned: isOwned,
                IsEquipped: isEquipped,
                IsDefault: c.IsDefault
            );
        }).ToList();

        return new CosmeticsResult(Cosmetics: cosmeticsWithOwnership);
    }

    private static bool IsEquipped(Core.Entities.User user, int cosmeticId, CosmeticType type)
    {
        return type switch
        {
            CosmeticType.BoardTheme => user.EquippedBoardThemeId == cosmeticId,
            CosmeticType.AvatarFrame => user.EquippedAvatarFrameId == cosmeticId,
            CosmeticType.Badge => user.EquippedBadgeId == cosmeticId,
            _ => false
        };
    }

    private static int GetUserIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }
}
