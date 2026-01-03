using PerfectFit.Core.Enums;

namespace PerfectFit.Core.Entities;

public class AdminAuditLog
{
    public Guid Id { get; private set; }
    public Guid AdminUserId { get; private set; }
    public AdminAction Action { get; private set; }
    public Guid? TargetUserId { get; private set; }
    public string? Details { get; private set; }
    public DateTime Timestamp { get; private set; }

    // Private constructor for EF Core
    private AdminAuditLog() { }

    public static AdminAuditLog Create(Guid adminUserId, AdminAction action, Guid? targetUserId, string? details)
    {
        return new AdminAuditLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = action,
            TargetUserId = targetUserId,
            Details = details,
            Timestamp = DateTime.UtcNow
        };
    }
}
