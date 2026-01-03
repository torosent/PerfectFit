## Phase 7 Complete: Create Frontend Admin API Client and Types

Added TypeScript types and API client for admin endpoints in the Next.js frontend.

**Files created/changed:**
- frontend/src/lib/api/admin-client.ts (new)
- frontend/src/__tests__/lib/api/admin-client.test.ts (new)
- frontend/src/types/index.ts
- frontend/src/stores/auth-store.ts
- frontend/src/lib/api/index.ts

**Functions created/changed:**
- `AdminApiError` class - Custom error with status code
- `getUsers(page?, pageSize?)` - Fetch paginated users list
- `getUser(id)` - Fetch single user by ID
- `deleteUser(id)` - Soft delete a user
- `bulkDeleteGuests()` - Bulk delete all guest accounts
- `getAuditLogs(page?, pageSize?)` - Fetch paginated audit logs
- `useIsAdmin()` - Zustand selector hook for admin check

**Types created:**
- `AdminUser` interface - User details for admin responses
- `PaginatedResponse<T>` interface - Generic pagination wrapper
- `AuditLog` interface - Audit log entries
- `BulkDeleteResponse` interface - Bulk delete response

**Tests created/changed:**
- 20 tests for admin-client covering:
  - getUsers - pagination, response structure, 401/403 errors
  - getUser - fetch by ID, 404 handling
  - deleteUser - DELETE request, error handling
  - bulkDeleteGuests - bulk endpoint, response
  - getAuditLogs - pagination, response
  - AdminApiError - status code handling

**Review Status:** APPROVED (after revision to fix AuditLog.id type and AdminUser.email nullability)

**Git Commit Message:**
```
feat: add frontend admin API client and TypeScript types

- Add AdminUser, PaginatedResponse, AuditLog, BulkDeleteResponse types
- Add admin-client.ts with getUsers, getUser, deleteUser, bulkDeleteGuests, getAuditLogs
- Add useIsAdmin selector hook to auth store
- Add AdminApiError class for error handling with status codes
- Add 20 tests for admin API client (all passing)
```
