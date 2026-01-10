using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.Core.Services;

/// <summary>
/// Service for managing cosmetic items.
/// </summary>
public class CosmeticService : ICosmeticService
{
    private readonly IGamificationRepository _repository;
    private readonly IUserRepository _userRepository;

    public CosmeticService(IGamificationRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Cosmetic>> GetAllCosmeticsAsync(CosmeticType? type = null, CancellationToken ct = default)
    {
        return await _repository.GetAllCosmeticsAsync(type, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserCosmetic>> GetUserCosmeticsAsync(int userId, CancellationToken ct = default)
    {
        return await _repository.GetUserCosmeticsAsync(userId, ct);
    }

    /// <inheritdoc />
    public async Task<bool> GrantCosmeticAsync(User user, int cosmeticId, ObtainedFrom source, CancellationToken ct = default)
    {
        var cosmetic = await _repository.GetCosmeticByIdAsync(cosmeticId, ct);
        if (cosmetic == null)
        {
            return false;
        }

        // Check if user already owns this cosmetic (idempotent)
        var existing = await _repository.GetUserCosmeticAsync(user.Id, cosmeticId, ct);
        if (existing != null)
        {
            return true; // Already owned
        }

        var userCosmetic = UserCosmetic.Create(user.Id, cosmeticId, source);
        
        // TryAddUserCosmeticAsync handles unique constraint violations
        return await _repository.TryAddUserCosmeticAsync(userCosmetic, ct);
    }

    /// <inheritdoc />
    public async Task<bool> GrantCosmeticByCodeAsync(User user, string cosmeticCode, ObtainedFrom source, CancellationToken ct = default)
    {
        var cosmetic = await _repository.GetCosmeticByCodeAsync(cosmeticCode, ct);
        if (cosmetic == null)
        {
            return false;
        }

        return await GrantCosmeticAsync(user, cosmetic.Id, source, ct);
    }

    /// <inheritdoc />
    public async Task<EquipResult> EquipCosmeticAsync(User user, int cosmeticId, CancellationToken ct = default)
    {
        var cosmetic = await _repository.GetCosmeticByIdAsync(cosmeticId, ct);
        if (cosmetic == null)
        {
            return new EquipResult(false, "Cosmetic not found.");
        }

        // Check ownership (default cosmetics are always available)
        if (!cosmetic.IsDefault)
        {
            var userCosmetic = await _repository.GetUserCosmeticAsync(user.Id, cosmeticId, ct);
            if (userCosmetic == null)
            {
                return new EquipResult(false, "You do not own this cosmetic.");
            }
        }

        // Equip the cosmetic
        user.EquipCosmetic(cosmetic.Type, cosmetic.Id);
        await _userRepository.UpdateAsync(user, ct);

        return new EquipResult(true);
    }

    /// <inheritdoc />
    public async Task<bool> UserOwnsCosmeticAsync(int userId, int cosmeticId, CancellationToken ct = default)
    {
        var cosmetic = await _repository.GetCosmeticByIdAsync(cosmeticId, ct);
        if (cosmetic == null)
        {
            return false;
        }

        // Default cosmetics are always owned
        if (cosmetic.IsDefault)
        {
            return true;
        }

        var userCosmetic = await _repository.GetUserCosmeticAsync(userId, cosmeticId, ct);
        return userCosmetic != null;
    }
}
