## Plan: Admin API and User Management Portal

Add role-based admin authorization to the backend API with endpoints to view, soft-delete, and bulk-delete users. Includes admin audit logging and a protected admin portal in the Next.js frontend for managing all user data.

**Decisions:**
- Soft delete with `IsDeleted` flag and `DeletedAt` timestamp
- Admin audit logging for all admin actions
- Initial admin email: `tomer.ros1@gmail.com`
- Bulk operations supported (e.g., delete all guest users)

**Phases (8 phases)**

1. **Phase 1: Add Role Enum and Update User Entity**
    - **Objective:** Add Role property and soft delete fields to the User entity
    - **Files/Functions to Modify/Create:**
      - Create `backend/src/PerfectFit.Core/Enums/UserRole.cs` (enum: User=0, Admin=1)
      - Modify `backend/src/PerfectFit.Core/Entities/User.cs` - Add Role, IsDeleted, DeletedAt properties
      - Modify `User.Create()` factory method to accept optional role
      - Add `User.SoftDelete()` method
    - **Tests to Write:**
      - `UserTests.Create_WithRole_ShouldSetRole`
      - `UserTests.Create_DefaultRole_ShouldBeUser`
      - `UserTests.SoftDelete_ShouldSetIsDeletedAndTimestamp`
    - **Steps:**
        1. Write unit tests for User entity with Role and soft delete
        2. Run tests to see them fail
        3. Create UserRole enum
        4. Add Role, IsDeleted, DeletedAt properties to User entity
        5. Update Create factory method
        6. Add SoftDelete method
        7. Run tests to confirm they pass

2. **Phase 2: Create Admin Audit Log Entity**
    - **Objective:** Add entity to track admin actions for audit purposes
    - **Files/Functions to Modify/Create:**
      - Create `backend/src/PerfectFit.Core/Entities/AdminAuditLog.cs` - Id, AdminUserId, Action, TargetUserId, Details, Timestamp
      - Create `backend/src/PerfectFit.Core/Enums/AdminAction.cs` - enum: ViewUsers, ViewUser, DeleteUser, BulkDeleteUsers
      - Modify `backend/src/PerfectFit.Infrastructure/Data/AppDbContext.cs` - Add DbSet for AdminAuditLog
      - Create `backend/src/PerfectFit.Core/Interfaces/IAdminAuditRepository.cs`
      - Create `backend/src/PerfectFit.Infrastructure/Repositories/AdminAuditRepository.cs`
    - **Tests to Write:**
      - `AdminAuditLogTests.Create_ShouldSetAllProperties`
      - `AdminAuditRepositoryTests.AddAsync_ShouldPersistLog`
    - **Steps:**
        1. Write unit tests for AdminAuditLog entity
        2. Run tests to see them fail
        3. Create AdminAction enum
        4. Create AdminAuditLog entity with factory method
        5. Add DbSet to AppDbContext
        6. Create IAdminAuditRepository interface
        7. Create AdminAuditRepository implementation
        8. Run tests to confirm they pass

3. **Phase 3: Add Database Migration**
    - **Objective:** Add migrations for Role column, soft delete fields, and audit log table
    - **Files/Functions to Modify/Create:**
      - Create EF Core migration for all new columns/tables
      - Modify `backend/src/PerfectFit.Infrastructure/Data/AppDbContext.cs` - Configure soft delete query filter
    - **Tests to Write:**
      - (Migration tested via integration tests in later phases)
    - **Steps:**
        1. Add global query filter for IsDeleted = false on User entity
        2. Run `dotnet ef migrations add AddAdminFeatures`
        3. Verify migration SQL looks correct
        4. Apply migration to development database

4. **Phase 4: Extend User Repository and Add Authorization**
    - **Objective:** Add repository methods for admin queries and configure admin authorization policy
    - **Files/Functions to Modify/Create:**
      - Modify `backend/src/PerfectFit.Core/Interfaces/IUserRepository.cs` - Add GetAllAsync, GetCountAsync, GetDeletedUsersAsync, SoftDeleteAsync, BulkSoftDeleteByProviderAsync
      - Modify `backend/src/PerfectFit.Infrastructure/Repositories/UserRepository.cs` - Implement new methods
      - Modify `backend/src/PerfectFit.Web/Program.cs` - Add AdminPolicy authorization
      - Modify `backend/src/PerfectFit.Infrastructure/Services/JwtService.cs` - Include role claim in JWT
    - **Tests to Write:**
      - `UserRepositoryTests.GetAllAsync_ShouldReturnNonDeletedUsers`
      - `UserRepositoryTests.GetAllAsync_WithPagination_ShouldReturnPagedResults`
      - `UserRepositoryTests.SoftDeleteAsync_ShouldMarkUserAsDeleted`
      - `UserRepositoryTests.BulkSoftDeleteByProviderAsync_ShouldDeleteMatchingUsers`
      - `JwtServiceTests.GenerateToken_WithAdminRole_ShouldIncludeRoleClaim`
    - **Steps:**
        1. Write tests for new repository methods and JWT changes
        2. Run tests to see them fail
        3. Add interface methods to IUserRepository
        4. Implement methods in UserRepository (respect soft delete)
        5. Add AdminPolicy to Program.cs requiring Admin role
        6. Update JwtService to include role claim
        7. Run tests to confirm they pass

5. **Phase 5: Create Admin API Endpoints**
    - **Objective:** Add admin-only API endpoints with audit logging
    - **Files/Functions to Modify/Create:**
      - Create `backend/src/PerfectFit.Web/Endpoints/AdminEndpoints.cs`:
        - GET /api/admin/users - List all users (paginated)
        - GET /api/admin/users/{id} - Get user details
        - DELETE /api/admin/users/{id} - Soft delete user
        - DELETE /api/admin/users/bulk/guests - Bulk delete guest users
        - GET /api/admin/audit-logs - View audit logs
      - Create `backend/src/PerfectFit.Web/DTOs/AdminDtos.cs` - AdminUserDto, PaginatedResponse<T>, AuditLogDto
      - Modify `backend/src/PerfectFit.Web/Program.cs` - Register admin endpoints
    - **Tests to Write:**
      - `AdminEndpointsTests.GetUsers_AsAdmin_ShouldReturnPaginatedUsers`
      - `AdminEndpointsTests.GetUsers_AsNonAdmin_ShouldReturn403`
      - `AdminEndpointsTests.DeleteUser_AsAdmin_ShouldSoftDeleteUser`
      - `AdminEndpointsTests.DeleteUser_SelfDelete_ShouldReturn400`
      - `AdminEndpointsTests.BulkDeleteGuests_ShouldDeleteAllGuestUsers`
      - `AdminEndpointsTests.AllActions_ShouldCreateAuditLog`
    - **Steps:**
        1. Write integration tests for admin endpoints
        2. Run tests to see them fail
        3. Create AdminDtos with records for responses
        4. Create AdminEndpoints with MapGroup("/api/admin")
        5. Add RequireAuthorization("AdminPolicy") to all endpoints
        6. Implement all endpoints with audit logging
        7. Add self-delete protection
        8. Register endpoints in Program.cs
        9. Run tests to confirm they pass

6. **Phase 6: Add Admin Bootstrap Configuration**
    - **Objective:** Configure initial admin users via settings and update auth flow
    - **Files/Functions to Modify/Create:**
      - Modify `backend/src/PerfectFit.Web/appsettings.json` - Add Admin:Emails configuration with tomer.ros1@gmail.com
      - Create `backend/src/PerfectFit.Core/Configuration/AdminSettings.cs` - Settings POCO
      - Modify `backend/src/PerfectFit.Web/Endpoints/AuthEndpoints.cs` - Set admin role on login for configured emails
    - **Tests to Write:**
      - `AuthEndpointsTests.Login_WithAdminEmail_ShouldSetAdminRole`
      - `AuthEndpointsTests.Login_WithNonAdminEmail_ShouldSetUserRole`
    - **Steps:**
        1. Write tests for admin email login
        2. Run tests to see them fail
        3. Add AdminSettings configuration class
        4. Add Admin:Emails to appsettings.json
        5. Update auth flow to check if user email matches admin list
        6. Set Role = Admin for matching users
        7. Run tests to confirm they pass

7. **Phase 7: Create Frontend Admin API Client and Types**
    - **Objective:** Add TypeScript types and API client for admin endpoints
    - **Files/Functions to Modify/Create:**
      - Create `frontend/src/lib/api/admin-client.ts` - getUsers, getUser, deleteUser, bulkDeleteGuests, getAuditLogs
      - Modify `frontend/src/types/index.ts` - Add AdminUser, PaginatedResponse, AuditLog types, UserRole enum
      - Modify `frontend/src/stores/auth-store.ts` - Add isAdmin computed property based on role
    - **Tests to Write:**
      - `admin-client.test.ts` - API client methods with mocked fetch
    - **Steps:**
        1. Write tests for admin API client functions
        2. Run tests to see them fail
        3. Add AdminUser, PaginatedResponse, AuditLog TypeScript types
        4. Add UserRole enum and isAdmin getter to auth store
        5. Create admin-client.ts with authenticated API calls
        6. Run tests to confirm they pass

8. **Phase 8: Create Admin Portal UI**
    - **Objective:** Build admin portal UI with users table, delete functionality, and audit log viewer
    - **Files/Functions to Modify/Create:**
      - Create `frontend/src/app/(admin)/admin/page.tsx` - Admin dashboard page
      - Create `frontend/src/app/(admin)/admin/layout.tsx` - Admin layout with guard
      - Create `frontend/src/components/admin/UsersTable.tsx` - Paginated users table with actions
      - Create `frontend/src/components/admin/DeleteUserModal.tsx` - Confirmation modal for single delete
      - Create `frontend/src/components/admin/BulkDeleteModal.tsx` - Confirmation modal for bulk delete
      - Create `frontend/src/components/admin/AuditLogTable.tsx` - Display audit logs
      - Create `frontend/src/components/admin/AdminGuard.tsx` - Role protection component
    - **Tests to Write:**
      - `UsersTable.test.tsx` - Table renders users, pagination works, actions trigger correctly
      - `DeleteUserModal.test.tsx` - Modal confirms deletion
      - `AdminGuard.test.tsx` - Redirects non-admins to home
    - **Steps:**
        1. Write component tests
        2. Run tests to see them fail
        3. Create AdminGuard component checking isAdmin from auth store
        4. Create admin layout wrapping children with AdminGuard
        5. Create UsersTable with pagination and action buttons
        6. Create DeleteUserModal with confirmation dialog
        7. Create BulkDeleteModal for guest user bulk delete
        8. Create AuditLogTable component
        9. Create admin page composing all components with tabs
        10. Run tests to confirm they pass
