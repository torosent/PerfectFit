using PerfectFit.Core.Enums;

namespace PerfectFit.Core.Entities;

public class UserCosmetic
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int CosmeticId { get; private set; }
    public DateTime ObtainedAt { get; private set; }
    public ObtainedFrom ObtainedFrom { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public Cosmetic? Cosmetic { get; private set; }

    // Private constructor for EF Core
    private UserCosmetic() { }

    public static UserCosmetic Create(int userId, int cosmeticId, ObtainedFrom obtainedFrom)
    {
        return new UserCosmetic
        {
            UserId = userId,
            CosmeticId = cosmeticId,
            ObtainedAt = DateTime.UtcNow,
            ObtainedFrom = obtainedFrom
        };
    }
}
