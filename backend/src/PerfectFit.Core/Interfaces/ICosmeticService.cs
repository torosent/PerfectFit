using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.Core.Interfaces;

/// <summary>
/// Service for managing cosmetic items.
/// </summary>
public interface ICosmeticService
{
    /// <summary>
    /// Gets all cosmetics, optionally filtered by type.
    /// </summary>
    /// <param name="type">Optional cosmetic type filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of cosmetics.</returns>
    Task<IReadOnlyList<Cosmetic>> GetAllCosmeticsAsync(CosmeticType? type = null, CancellationToken ct = default);

    /// <summary>
    /// Gets all cosmetics owned by a specific user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of user cosmetics.</returns>
    Task<IReadOnlyList<UserCosmetic>> GetUserCosmeticsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Grants a cosmetic to a user. Idempotent - returns true if already owned.
    /// </summary>
    /// <param name="user">The user to grant the cosmetic to.</param>
    /// <param name="cosmeticId">The ID of the cosmetic to grant.</param>
    /// <param name="source">How the cosmetic was obtained.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the cosmetic was granted or already owned.</returns>
    Task<bool> GrantCosmeticAsync(User user, int cosmeticId, ObtainedFrom source, CancellationToken ct = default);

    /// <summary>
    /// Equips a cosmetic for a user.
    /// </summary>
    /// <param name="user">The user equipping the cosmetic.</param>
    /// <param name="cosmeticId">The ID of the cosmetic to equip.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the equip operation.</returns>
    Task<EquipResult> EquipCosmeticAsync(User user, int cosmeticId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user owns a specific cosmetic.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="cosmeticId">The cosmetic's ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the user owns the cosmetic or it's a default cosmetic.</returns>
    Task<bool> UserOwnsCosmeticAsync(int userId, int cosmeticId, CancellationToken ct = default);
}
