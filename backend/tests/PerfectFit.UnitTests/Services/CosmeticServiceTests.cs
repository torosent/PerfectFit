using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;
using PerfectFit.Core.Services.Results;
using System.Reflection;

namespace PerfectFit.UnitTests.Services;

public class CosmeticServiceTests
{
    private readonly Mock<IGamificationRepository> _repositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly CosmeticService _service;

    public CosmeticServiceTests()
    {
        _repositoryMock = new Mock<IGamificationRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _service = new CosmeticService(_repositoryMock.Object, _userRepositoryMock.Object);
    }

    #region GetAllCosmeticsAsync Tests

    [Fact]
    public async Task GetAllCosmetics_ReturnsAll()
    {
        // Arrange
        var cosmetics = new List<Cosmetic>
        {
            CreateCosmetic(1, CosmeticType.BoardTheme),
            CreateCosmetic(2, CosmeticType.AvatarFrame),
            CreateCosmetic(3, CosmeticType.Badge),
        };

        _repositoryMock.Setup(r => r.GetAllCosmeticsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmetics);

        // Act
        var result = await _service.GetAllCosmeticsAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllCosmetics_FiltersByType()
    {
        // Arrange
        var boardThemes = new List<Cosmetic>
        {
            CreateCosmetic(1, CosmeticType.BoardTheme),
            CreateCosmetic(2, CosmeticType.BoardTheme),
        };

        _repositoryMock.Setup(r => r.GetAllCosmeticsAsync(CosmeticType.BoardTheme, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardThemes);

        // Act
        var result = await _service.GetAllCosmeticsAsync(CosmeticType.BoardTheme);

        // Assert
        result.Should().HaveCount(2);
        result.All(c => c.Type == CosmeticType.BoardTheme).Should().BeTrue();
    }

    #endregion

    #region GrantCosmeticAsync Tests

    [Fact]
    public async Task GrantCosmetic_Success_CreatesUserCosmetic()
    {
        // Arrange
        var user = CreateUser();
        var cosmetic = CreateCosmetic(1, CosmeticType.Badge);

        _repositoryMock.Setup(r => r.GetCosmeticByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmetic);
        _repositoryMock.Setup(r => r.GetUserCosmeticAsync(user.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserCosmetic?)null);
        _repositoryMock.Setup(r => r.TryAddUserCosmeticAsync(It.IsAny<UserCosmetic>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.GrantCosmeticAsync(user, 1, ObtainedFrom.Achievement);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.TryAddUserCosmeticAsync(It.IsAny<UserCosmetic>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GrantCosmetic_AlreadyOwned_ReturnsTrue()
    {
        // Arrange
        var user = CreateUser();
        var cosmetic = CreateCosmetic(1, CosmeticType.Badge);
        var existingUserCosmetic = UserCosmetic.Create(user.Id, 1, ObtainedFrom.Achievement);

        _repositoryMock.Setup(r => r.GetCosmeticByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmetic);
        _repositoryMock.Setup(r => r.GetUserCosmeticAsync(user.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUserCosmetic);

        // Act
        var result = await _service.GrantCosmeticAsync(user, 1, ObtainedFrom.Achievement);

        // Assert
        result.Should().BeTrue(); // Idempotent
        _repositoryMock.Verify(r => r.AddUserCosmeticAsync(It.IsAny<UserCosmetic>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GrantCosmetic_CosmeticNotFound_ReturnsFalse()
    {
        // Arrange
        var user = CreateUser();

        _repositoryMock.Setup(r => r.GetCosmeticByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cosmetic?)null);

        // Act
        var result = await _service.GrantCosmeticAsync(user, 999, ObtainedFrom.Achievement);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region EquipCosmeticAsync Tests

    [Fact]
    public async Task EquipCosmetic_Owned_Success()
    {
        // Arrange
        var user = CreateUser();
        var cosmetic = CreateCosmetic(1, CosmeticType.BoardTheme, isDefault: false);
        var userCosmetic = UserCosmetic.Create(user.Id, 1, ObtainedFrom.Achievement);

        _repositoryMock.Setup(r => r.GetCosmeticByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmetic);
        _repositoryMock.Setup(r => r.GetUserCosmeticAsync(user.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userCosmetic);

        // Act
        var result = await _service.EquipCosmeticAsync(user, 1);

        // Assert
        result.Success.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EquipCosmetic_NotOwned_Fails()
    {
        // Arrange
        var user = CreateUser();
        var cosmetic = CreateCosmetic(1, CosmeticType.BoardTheme, isDefault: false);

        _repositoryMock.Setup(r => r.GetCosmeticByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmetic);
        _repositoryMock.Setup(r => r.GetUserCosmeticAsync(user.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserCosmetic?)null);

        // Act
        var result = await _service.EquipCosmeticAsync(user, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("do not own");
    }

    [Fact]
    public async Task EquipCosmetic_DefaultCosmetic_AlwaysSuccess()
    {
        // Arrange
        var user = CreateUser();
        var cosmetic = CreateCosmetic(1, CosmeticType.BoardTheme, isDefault: true);

        _repositoryMock.Setup(r => r.GetCosmeticByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmetic);

        // Act
        var result = await _service.EquipCosmeticAsync(user, 1);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task EquipCosmetic_CosmeticNotFound_Fails()
    {
        // Arrange
        var user = CreateUser();

        _repositoryMock.Setup(r => r.GetCosmeticByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cosmetic?)null);

        // Act
        var result = await _service.EquipCosmeticAsync(user, 999);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    #endregion

    #region UserOwnsCosmeticAsync Tests

    [Fact]
    public async Task UserOwnsCosmetic_Default_ReturnsTrue()
    {
        // Arrange
        var cosmetic = CreateCosmetic(1, CosmeticType.BoardTheme, isDefault: true);

        _repositoryMock.Setup(r => r.GetCosmeticByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmetic);

        // Act
        var result = await _service.UserOwnsCosmeticAsync(1, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserOwnsCosmetic_Owned_ReturnsTrue()
    {
        // Arrange
        var cosmetic = CreateCosmetic(1, CosmeticType.BoardTheme, isDefault: false);
        var userCosmetic = UserCosmetic.Create(1, 1, ObtainedFrom.Achievement);

        _repositoryMock.Setup(r => r.GetCosmeticByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmetic);
        _repositoryMock.Setup(r => r.GetUserCosmeticAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userCosmetic);

        // Act
        var result = await _service.UserOwnsCosmeticAsync(1, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserOwnsCosmetic_NotOwned_ReturnsFalse()
    {
        // Arrange
        var cosmetic = CreateCosmetic(1, CosmeticType.BoardTheme, isDefault: false);

        _repositoryMock.Setup(r => r.GetCosmeticByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmetic);
        _repositoryMock.Setup(r => r.GetUserCosmeticAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserCosmetic?)null);

        // Act
        var result = await _service.UserOwnsCosmeticAsync(1, 1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private static User CreateUser(int id = 1)
    {
        var user = User.Create("ext_123", "test@test.com", "TestUser", AuthProvider.Local);
        SetProperty(user, "Id", id);
        return user;
    }

    private static Cosmetic CreateCosmetic(
        int id,
        CosmeticType type,
        bool isDefault = false,
        CosmeticRarity rarity = CosmeticRarity.Common)
    {
        var cosmetic = Cosmetic.Create(
            $"cosmetic_{id}",
            $"Cosmetic {id}",
            $"Description {id}",
            type,
            $"/assets/cosmetic_{id}.png",
            $"/assets/cosmetic_{id}_preview.png",
            rarity,
            isDefault);

        SetProperty(cosmetic, "Id", id);

        return cosmetic;
    }

    private static void SetProperty<T, TValue>(T obj, string propertyName, TValue value) where T : class
    {
        var backingField = typeof(T).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(obj, value);
    }

    #endregion
}
