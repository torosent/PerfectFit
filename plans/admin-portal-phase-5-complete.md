## Phase 5 Complete: Create Admin API Endpoints

Created admin-only API endpoints with proper audit logging for user management operations.

**Files created/changed:**
- backend/src/PerfectFit.Web/DTOs/AdminDtos.cs (new)
- backend/src/PerfectFit.Web/Endpoints/AdminEndpoints.cs (new)
- backend/src/PerfectFit.Web/Program.cs
- backend/src/PerfectFit.Core/Entities/AdminAuditLog.cs (updated ID types)
- backend/src/PerfectFit.Infrastructure/Data/Configurations/AdminAuditLogConfiguration.cs
- backend/src/PerfectFit.Infrastructure/Data/Migrations/*_ChangeAuditLogUserIdsToInt.cs (new)
- backend/tests/PerfectFit.IntegrationTests/Web/Endpoints/AdminEndpointsTests.cs (new)
- backend/tests/PerfectFit.UnitTests/Core/Entities/AdminAuditLogTests.cs

**Endpoints implemented:**
- `GET /api/admin/users` - List all users (paginated)
- `GET /api/admin/users/{id}` - Get user details
- `DELETE /api/admin/users/{id}` - Soft delete user (with self-delete protection)
- `DELETE /api/admin/users/bulk/guests` - Bulk delete all guest users
- `GET /api/admin/audit-logs` - View audit logs (paginated)

**Functions created/changed:**
- `AdminUserDto` - User details for admin responses
- `PaginatedResponse<T>` - Generic pagination wrapper
- `AuditLogDto` - Audit log entry response
- `BulkDeleteResponse` - Bulk operation response
- `AdminEndpoints.MapAdminEndpoints()` - All admin endpoints with audit logging
- `AdminAuditLog` - Updated to use int IDs matching User entity

**Tests created/changed:**
- `GetUsers_AsAdmin_ShouldReturnPaginatedUsers`
- `GetUsers_AsNonAdmin_ShouldReturn403`
- `GetUser_AsAdmin_ShouldReturnUserDetails`
- `GetUser_NotFound_ShouldReturn404`
- `DeleteUser_AsAdmin_ShouldSoftDeleteUser`
- `DeleteUser_SelfDelete_ShouldReturn400`
- `DeleteUser_ShouldCreateAuditLog`
- `BulkDeleteGuests_AsAdmin_ShouldDeleteAllGuestUsers`
- `BulkDeleteGuests_ShouldCreateAuditLog`
- `GetAuditLogs_AsAdmin_ShouldReturnLogs`

**Review Status:** APPROVED (after revision to fix audit log user ID tracking)

**Git Commit Message:**
```
feat: add admin API endpoints with audit logging

- Add GET /api/admin/users for paginated user listing
- Add GET /api/admin/users/{id} for user details
- Add DELETE /api/admin/users/{id} with self-delete protection
- Add DELETE /api/admin/users/bulk/guests for bulk guest deletion
- Add GET /api/admin/audit-logs for viewing admin actions
- All endpoints protected by AdminPolicy authorization
- All operations create audit log entries with proper user IDs
- Add migration to align audit log ID types with User entity
- Add 10 integration tests for admin endpoints
```
