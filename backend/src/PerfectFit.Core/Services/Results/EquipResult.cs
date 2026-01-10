namespace PerfectFit.Core.Services.Results;

/// <summary>
/// Result of equipping a cosmetic item.
/// </summary>
/// <param name="Success">Whether the cosmetic was successfully equipped.</param>
/// <param name="ErrorMessage">Error message if the operation failed.</param>
public record EquipResult(
    bool Success,
    string? ErrorMessage = null
);
