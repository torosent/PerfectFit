using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Web.DTOs;
using System.Security.Claims;

namespace PerfectFit.Web.Endpoints;

/// <summary>
/// Admin API endpoints for user management and audit logging.
/// </summary>
public static class AdminEndpoints
{
    /// <summary>
    /// Maps all admin-related endpoints.
    /// </summary>
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization("AdminPolicy");

        // GET /api/admin/users - List all users (paginated)
        group.MapGet("/users", GetUsers)
            .WithName("AdminGetUsers")
            .WithSummary("Get all users (paginated)")
            .Produces<PaginatedResponse<AdminUserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // GET /api/admin/users/{id} - Get single user details
        group.MapGet("/users/{id:int}", GetUser)
            .WithName("AdminGetUser")
            .WithSummary("Get a single user by ID")
            .Produces<AdminUserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/admin/users/{id} - Soft delete a user
        group.MapDelete("/users/{id:int}", DeleteUser)
            .WithName("AdminDeleteUser")
            .WithSummary("Soft delete a user")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/admin/users/bulk/guests - Bulk delete all guest users
        group.MapDelete("/users/bulk/guests", BulkDeleteGuests)
            .WithName("AdminBulkDeleteGuests")
            .WithSummary("Bulk soft delete all guest users")
            .Produces<BulkDeleteResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // GET /api/admin/audit-logs - View audit logs (paginated)
        group.MapGet("/audit-logs", GetAuditLogs)
            .WithName("AdminGetAuditLogs")
            .WithSummary("Get audit logs (paginated)")
            .Produces<PaginatedResponse<AuditLogDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> GetUsers(
        ClaimsPrincipal user,
        IUserRepository userRepository,
        IAdminAuditRepository auditRepository,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Get paginated users
        var users = await userRepository.GetAllAsync(page, pageSize, cancellationToken);
        var totalCount = await userRepository.GetCountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Create audit log with actual admin user ID
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.ViewUsers,
            null,
            $"Viewed users page {page}"
        );
        await auditRepository.AddAsync(auditLog, cancellationToken);

        var items = users.Select(MapToAdminUserDto);

        return Results.Ok(new PaginatedResponse<AdminUserDto>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages
        ));
    }

    private static async Task<IResult> GetUser(
        int id,
        ClaimsPrincipal user,
        IUserRepository userRepository,
        IAdminAuditRepository auditRepository,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        var targetUser = await userRepository.GetByIdAsync(id, cancellationToken);
        if (targetUser == null)
        {
            return Results.NotFound();
        }

        // Create audit log with actual admin user ID and target user ID
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.ViewUser,
            targetUser.Id,
            $"Viewed user {targetUser.Email ?? targetUser.DisplayName}"
        );
        await auditRepository.AddAsync(auditLog, cancellationToken);

        return Results.Ok(MapToAdminUserDto(targetUser));
    }

    private static async Task<IResult> DeleteUser(
        int id,
        ClaimsPrincipal user,
        IUserRepository userRepository,
        IAdminAuditRepository auditRepository,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        // Self-delete protection
        if (id == adminUserId.Value)
        {
            return Results.BadRequest(new { error = "You cannot delete yourself. Use another admin account." });
        }

        var targetUser = await userRepository.GetByIdAsync(id, cancellationToken);
        if (targetUser == null)
        {
            return Results.NotFound();
        }

        // Perform soft delete
        await userRepository.SoftDeleteAsync(id, cancellationToken);

        // Create audit log with actual admin user ID and target user ID
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.DeleteUser,
            targetUser.Id,
            $"Deleted user {targetUser.Email ?? targetUser.DisplayName}"
        );
        await auditRepository.AddAsync(auditLog, cancellationToken);

        return Results.Ok(new { message = $"User {id} has been deleted" });
    }

    private static async Task<IResult> BulkDeleteGuests(
        ClaimsPrincipal user,
        IUserRepository userRepository,
        IAdminAuditRepository auditRepository,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        // Bulk soft delete all guest users
        var deletedCount = await userRepository.BulkSoftDeleteByProviderAsync(AuthProvider.Guest, cancellationToken);

        // Create audit log with actual admin user ID
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.BulkDeleteUsers,
            null,
            $"Bulk deleted {deletedCount} guest users"
        );
        await auditRepository.AddAsync(auditLog, cancellationToken);

        return Results.Ok(new BulkDeleteResponse(
            DeletedCount: deletedCount,
            Message: $"Successfully deleted {deletedCount} guest user(s)"
        ));
    }

    private static async Task<IResult> GetAuditLogs(
        ClaimsPrincipal user,
        IAdminAuditRepository auditRepository,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var logs = await auditRepository.GetAllAsync(page, pageSize, cancellationToken);
        var totalCount = await auditRepository.GetCountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = logs.Select(log => new AuditLogDto(
            Id: log.Id,
            AdminUserId: log.AdminUserId,
            AdminEmail: null, // Would need a join to get this
            Action: log.Action.ToString(),
            TargetUserId: log.TargetUserId,
            TargetUserEmail: null, // Would need a join to get this
            Details: log.Details,
            Timestamp: log.Timestamp
        ));

        return Results.Ok(new PaginatedResponse<AuditLogDto>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages
        ));
    }

    private static int? GetAdminUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    private static AdminUserDto MapToAdminUserDto(User user)
    {
        return new AdminUserDto(
            Id: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            Avatar: user.Avatar,
            Provider: user.Provider.ToString(),
            Role: user.Role.ToString(),
            CreatedAt: user.CreatedAt,
            LastLoginAt: user.LastLoginAt,
            HighScore: user.HighScore,
            GamesPlayed: user.GamesPlayed,
            IsDeleted: user.IsDeleted,
            DeletedAt: user.DeletedAt
        );
    }
}
