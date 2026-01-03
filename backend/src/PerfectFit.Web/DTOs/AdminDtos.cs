namespace PerfectFit.Web.DTOs;

/// <summary>
/// Represents a user in admin API responses with full details.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="DisplayName">The user's display name.</param>
/// <param name="Avatar">The user's avatar emoji.</param>
/// <param name="Provider">The authentication provider (Google, Microsoft, Guest, etc.).</param>
/// <param name="Role">The user's role (User or Admin).</param>
/// <param name="CreatedAt">When the user account was created.</param>
/// <param name="LastLoginAt">When the user last logged in.</param>
/// <param name="HighScore">The user's highest score.</param>
/// <param name="GamesPlayed">Total number of games the user has played.</param>
/// <param name="IsDeleted">Whether the user has been soft-deleted.</param>
/// <param name="DeletedAt">When the user was soft-deleted.</param>
public record AdminUserDto(
    int Id,
    string? Email,
    string? DisplayName,
    string? Avatar,
    string Provider,
    string Role,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    int HighScore,
    int GamesPlayed,
    bool IsDeleted,
    DateTime? DeletedAt
);

/// <summary>
/// Generic paginated response for list endpoints.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
/// <param name="Items">The collection of items for the current page.</param>
/// <param name="Page">The current page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
/// <param name="TotalPages">The total number of pages.</param>
public record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

/// <summary>
/// Represents an audit log entry in admin API responses.
/// </summary>
/// <param name="Id">The unique identifier of the audit log entry.</param>
/// <param name="AdminUserId">The ID of the admin who performed the action.</param>
/// <param name="AdminEmail">The email of the admin who performed the action.</param>
/// <param name="Action">The type of action performed.</param>
/// <param name="TargetUserId">The ID of the user affected by the action (if applicable).</param>
/// <param name="TargetUserEmail">The email of the user affected by the action (if applicable).</param>
/// <param name="Details">Additional details about the action.</param>
/// <param name="Timestamp">When the action was performed.</param>
public record AuditLogDto(
    Guid Id,
    int AdminUserId,
    string? AdminEmail,
    string Action,
    int? TargetUserId,
    string? TargetUserEmail,
    string? Details,
    DateTime Timestamp
);

/// <summary>
/// Response for bulk delete operations.
/// </summary>
/// <param name="DeletedCount">The number of items that were deleted.</param>
/// <param name="Message">A human-readable message describing the result.</param>
public record BulkDeleteResponse(int DeletedCount, string Message);
