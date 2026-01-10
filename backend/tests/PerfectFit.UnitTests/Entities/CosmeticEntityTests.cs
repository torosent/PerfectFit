using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.UnitTests.Entities;

public class CosmeticEntityTests
{
    #region Cosmetic Tests

    [Fact]
    public void Cosmetic_Create_SetsProperties()
    {
        // Arrange
        var code = "theme_ocean_blue";
        var name = "Ocean Blue Theme";
        var description = "A calming blue board theme";
        var type = CosmeticType.BoardTheme;
        var assetUrl = "https://example.com/themes/ocean-blue.json";
        var previewUrl = "https://example.com/themes/ocean-blue-preview.png";
        var rarity = CosmeticRarity.Rare;

        // Act
        var cosmetic = Cosmetic.Create(code, name, description, type, assetUrl, previewUrl, rarity);

        // Assert
        cosmetic.Id.Should().Be(0); // Not set until persisted
        cosmetic.Code.Should().Be(code);
        cosmetic.Name.Should().Be(name);
        cosmetic.Description.Should().Be(description);
        cosmetic.Type.Should().Be(type);
        cosmetic.AssetUrl.Should().Be(assetUrl);
        cosmetic.PreviewUrl.Should().Be(previewUrl);
        cosmetic.Rarity.Should().Be(rarity);
        cosmetic.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void Cosmetic_Create_WithIsDefaultTrue_SetsIsDefault()
    {
        // Arrange & Act
        var cosmetic = Cosmetic.Create(
            "theme_default",
            "Default Theme",
            "The default board theme",
            CosmeticType.BoardTheme,
            "https://example.com/themes/default.json",
            "https://example.com/themes/default-preview.png",
            CosmeticRarity.Common,
            isDefault: true);

        // Assert
        cosmetic.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void Cosmetic_Create_ThrowsWhenNameEmpty()
    {
        // Arrange & Act
        var act = () => Cosmetic.Create(
            "badge_test",
            "",
            "Description",
            CosmeticType.Badge,
            "https://example.com/asset.png",
            "https://example.com/preview.png",
            CosmeticRarity.Common);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Cosmetic_Create_AllowsEmptyDescription()
    {
        // Arrange & Act
        var cosmetic = Cosmetic.Create(
            "badge_simple",
            "Simple Badge",
            "",
            CosmeticType.Badge,
            "https://example.com/asset.png",
            "https://example.com/preview.png",
            CosmeticRarity.Common);

        // Assert
        cosmetic.Description.Should().BeEmpty();
    }

    [Fact]
    public void Cosmetic_Create_ThrowsWhenAssetUrlEmpty()
    {
        // Arrange & Act
        var act = () => Cosmetic.Create(
            "frame_test",
            "Test Cosmetic",
            "Description",
            CosmeticType.AvatarFrame,
            "",
            "https://example.com/preview.png",
            CosmeticRarity.Epic);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("assetUrl");
    }

    [Fact]
    public void Cosmetic_Create_ThrowsWhenCodeEmpty()
    {
        // Arrange & Act
        var act = () => Cosmetic.Create(
            "",
            "Test Cosmetic",
            "Description",
            CosmeticType.AvatarFrame,
            "https://example.com/asset.png",
            "https://example.com/preview.png",
            CosmeticRarity.Epic);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("code");
    }

    [Fact]
    public void Cosmetic_Create_AllowsAllRarities()
    {
        // Arrange & Act
        var common = Cosmetic.Create("badge_common", "Common", "Desc", CosmeticType.Badge, "url", "preview", CosmeticRarity.Common);
        var rare = Cosmetic.Create("badge_rare", "Rare", "Desc", CosmeticType.Badge, "url", "preview", CosmeticRarity.Rare);
        var epic = Cosmetic.Create("badge_epic", "Epic", "Desc", CosmeticType.Badge, "url", "preview", CosmeticRarity.Epic);
        var legendary = Cosmetic.Create("badge_legendary", "Legendary", "Desc", CosmeticType.Badge, "url", "preview", CosmeticRarity.Legendary);

        // Assert
        common.Rarity.Should().Be(CosmeticRarity.Common);
        rare.Rarity.Should().Be(CosmeticRarity.Rare);
        epic.Rarity.Should().Be(CosmeticRarity.Epic);
        legendary.Rarity.Should().Be(CosmeticRarity.Legendary);
    }

    [Fact]
    public void Cosmetic_Create_AllowsAllTypes()
    {
        // Arrange & Act
        var theme = Cosmetic.Create("theme_test", "Theme", "Desc", CosmeticType.BoardTheme, "url", "preview", CosmeticRarity.Common);
        var frame = Cosmetic.Create("frame_test", "Frame", "Desc", CosmeticType.AvatarFrame, "url", "preview", CosmeticRarity.Common);
        var badge = Cosmetic.Create("badge_test", "Badge", "Desc", CosmeticType.Badge, "url", "preview", CosmeticRarity.Common);

        // Assert
        theme.Type.Should().Be(CosmeticType.BoardTheme);
        frame.Type.Should().Be(CosmeticType.AvatarFrame);
        badge.Type.Should().Be(CosmeticType.Badge);
    }

    #endregion

    #region UserCosmetic Tests

    [Fact]
    public void UserCosmetic_Create_TracksObtainment()
    {
        // Arrange
        var userId = 1;
        var cosmeticId = 42;
        var obtainedFrom = ObtainedFrom.Achievement;

        // Act
        var userCosmetic = UserCosmetic.Create(userId, cosmeticId, obtainedFrom);

        // Assert
        userCosmetic.Id.Should().Be(0); // Not set until persisted
        userCosmetic.UserId.Should().Be(userId);
        userCosmetic.CosmeticId.Should().Be(cosmeticId);
        userCosmetic.ObtainedFrom.Should().Be(obtainedFrom);
        userCosmetic.ObtainedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UserCosmetic_Create_FromSeasonPass()
    {
        // Arrange & Act
        var userCosmetic = UserCosmetic.Create(1, 10, ObtainedFrom.SeasonPass);

        // Assert
        userCosmetic.ObtainedFrom.Should().Be(ObtainedFrom.SeasonPass);
    }

    [Fact]
    public void UserCosmetic_Create_FromDefault()
    {
        // Arrange & Act
        var userCosmetic = UserCosmetic.Create(1, 1, ObtainedFrom.Default);

        // Assert
        userCosmetic.ObtainedFrom.Should().Be(ObtainedFrom.Default);
    }

    [Fact]
    public void UserCosmetic_Create_FromPurchase()
    {
        // Arrange & Act
        var userCosmetic = UserCosmetic.Create(1, 100, ObtainedFrom.Purchase);

        // Assert
        userCosmetic.ObtainedFrom.Should().Be(ObtainedFrom.Purchase);
    }

    #endregion
}
