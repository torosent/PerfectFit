using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;

namespace PerfectFit.UnitTests.Entities;

public class AdminAuditLogTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var action = AdminAction.ViewUser;
        var targetUserId = Guid.NewGuid();
        var details = "{\"userId\": 123}";

        // Act
        var log = AdminAuditLog.Create(adminUserId, action, targetUserId, details);

        // Assert
        log.Id.Should().NotBe(Guid.Empty);
        log.AdminUserId.Should().Be(adminUserId);
        log.Action.Should().Be(action);
        log.TargetUserId.Should().Be(targetUserId);
        log.Details.Should().Be(details);
        log.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithNullDetails_ShouldAllowNull()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var action = AdminAction.ViewUsers;
        var targetUserId = Guid.NewGuid();

        // Act
        var log = AdminAuditLog.Create(adminUserId, action, targetUserId, null);

        // Assert
        log.Details.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullTargetUserId_ShouldAllowNull()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var action = AdminAction.ViewUsers;
        var details = "Viewed all users";

        // Act
        var log = AdminAuditLog.Create(adminUserId, action, null, details);

        // Assert
        log.TargetUserId.Should().BeNull();
    }
}
