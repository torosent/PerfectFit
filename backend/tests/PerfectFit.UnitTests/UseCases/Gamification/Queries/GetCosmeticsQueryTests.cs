using FluentAssertions;
using Moq;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Gamification;
using PerfectFit.UseCases.Gamification.Queries;
using System.Reflection;

namespace PerfectFit.UnitTests.UseCases.Gamification.Queries;

public class GetCosmeticsQueryTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICosmeticService> _cosmeticServiceMock;
    private readonly GetCosmeticsQueryHandler _handler;

    public GetCosmeticsQueryTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _cosmeticServiceMock = new Mock<ICosmeticService>();
        _handler = new GetCosmeticsQueryHandler(_userRepositoryMock.Object, _cosmeticServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAllCosmeticsWithOwnership()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var cosmetics = new List<Cosmetic>
        {
            CreateCosmetic(1, "Default Theme", CosmeticType.BoardTheme, isDefault: true),
            CreateCosmetic(2, "Premium Theme", CosmeticType.BoardTheme, isDefault: false),
            CreateCosmetic(3, "Gold Frame", CosmeticType.AvatarFrame, isDefault: false)
        };
        var userCosmetics = new List<UserCosmetic>
        {
            CreateUserCosmetic(userIntId, 2) // User owns Premium Theme
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _cosmeticServiceMock
            .Setup(x => x.GetAllCosmeticsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmetics);

        _cosmeticServiceMock
            .Setup(x => x.GetUserCosmeticsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userCosmetics);

        var query = new GetCosmeticsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Cosmetics.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_DefaultCosmeticsMarkedAsOwned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var cosmetics = new List<Cosmetic>
        {
            CreateCosmetic(1, "Default Theme", CosmeticType.BoardTheme, isDefault: true)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _cosmeticServiceMock
            .Setup(x => x.GetAllCosmeticsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmetics);

        _cosmeticServiceMock
            .Setup(x => x.GetUserCosmeticsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserCosmetic>());

        var query = new GetCosmeticsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Cosmetics[0].IsOwned.Should().BeTrue();
        result.Cosmetics[0].IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FiltersByType_ReturnsOnlyBoardThemes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);
        var boardThemes = new List<Cosmetic>
        {
            CreateCosmetic(1, "Default Theme", CosmeticType.BoardTheme, isDefault: true),
            CreateCosmetic(2, "Premium Theme", CosmeticType.BoardTheme, isDefault: false)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _cosmeticServiceMock
            .Setup(x => x.GetAllCosmeticsAsync(CosmeticType.BoardTheme, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardThemes);

        _cosmeticServiceMock
            .Setup(x => x.GetUserCosmeticsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserCosmetic>());

        var query = new GetCosmeticsQuery(userId, CosmeticType.BoardTheme);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Cosmetics.Should().HaveCount(2);
        result.Cosmetics.All(c => c.Type == CosmeticType.BoardTheme).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EquippedCosmeticsMarked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId, equippedBoardThemeId: 2);
        var cosmetics = new List<Cosmetic>
        {
            CreateCosmetic(1, "Default Theme", CosmeticType.BoardTheme, isDefault: true),
            CreateCosmetic(2, "Premium Theme", CosmeticType.BoardTheme, isDefault: false)
        };
        var userCosmetics = new List<UserCosmetic>
        {
            CreateUserCosmetic(userIntId, 2)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _cosmeticServiceMock
            .Setup(x => x.GetAllCosmeticsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmetics);

        _cosmeticServiceMock
            .Setup(x => x.GetUserCosmeticsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userCosmetics);

        var query = new GetCosmeticsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var premiumTheme = result.Cosmetics.First(c => c.CosmeticId == 2);
        premiumTheme.IsEquipped.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetCosmeticsQuery(userId);

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_NoCosmetics_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIntId = 1;

        var user = CreateUser(userIntId, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _cosmeticServiceMock
            .Setup(x => x.GetAllCosmeticsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Cosmetic>());

        _cosmeticServiceMock
            .Setup(x => x.GetUserCosmeticsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserCosmetic>());

        var query = new GetCosmeticsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Cosmetics.Should().BeEmpty();
    }

    private static User CreateUser(int id, Guid externalGuid, int? equippedBoardThemeId = null)
    {
        var user = User.Create(externalGuid.ToString(), "test@test.com", "TestUser", AuthProvider.Local);
        SetProperty(user, "Id", id);
        SetProperty(user, "EquippedBoardThemeId", equippedBoardThemeId);
        return user;
    }

    private static Cosmetic CreateCosmetic(int id, string name, CosmeticType type, bool isDefault = false)
    {
        var cosmetic = Cosmetic.Create($"cosmetic_{id}", name, "Test cosmetic", type, "/asset.png", "/preview.png", CosmeticRarity.Common, isDefault);
        SetProperty(cosmetic, "Id", id);
        return cosmetic;
    }

    private static UserCosmetic CreateUserCosmetic(int userId, int cosmeticId)
    {
        var userCosmetic = UserCosmetic.Create(userId, cosmeticId, ObtainedFrom.SeasonPass);
        return userCosmetic;
    }

    private static void SetProperty<TEntity, TValue>(TEntity entity, string propertyName, TValue value) where TEntity : class
    {
        var backingField = typeof(TEntity).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        backingField?.SetValue(entity, value);
    }
}
