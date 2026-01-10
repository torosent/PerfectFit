using PerfectFit.Core.Enums;

namespace PerfectFit.Core.Entities;

public class Cosmetic
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public CosmeticType Type { get; private set; }
    public string AssetUrl { get; private set; } = string.Empty;
    public string PreviewUrl { get; private set; } = string.Empty;
    public CosmeticRarity Rarity { get; private set; }
    public bool IsDefault { get; private set; }

    // Navigation properties
    public ICollection<UserCosmetic> UserCosmetics { get; private set; } = new List<UserCosmetic>();

    // Private constructor for EF Core
    private Cosmetic() { }

    public static Cosmetic Create(
        string name,
        string description,
        CosmeticType type,
        string assetUrl,
        string previewUrl,
        CosmeticRarity rarity,
        bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(assetUrl, nameof(assetUrl));

        return new Cosmetic
        {
            Name = name,
            Description = description ?? string.Empty,
            Type = type,
            AssetUrl = assetUrl,
            PreviewUrl = previewUrl ?? string.Empty,
            Rarity = rarity,
            IsDefault = isDefault
        };
    }
}
