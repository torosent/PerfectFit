## Phase 3 Complete: Database Migration

Created EF Core migration for admin features schema including soft delete support and admin audit log table.

**Files created/changed:**
- backend/src/PerfectFit.Infrastructure/Data/Configurations/UserConfiguration.cs
- backend/src/PerfectFit.Infrastructure/Data/Migrations/XXXXXXXX_AddAdminFeatures.cs (new)
- backend/src/PerfectFit.Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs

**Functions created/changed:**
- UserConfiguration: Added Role, IsDeleted, DeletedAt column configs (snake_case)
- UserConfiguration: Added global query filter `HasQueryFilter(u => !u.IsDeleted)`
- Migration Up(): Adds role, is_deleted, deleted_at to users table
- Migration Up(): Creates admin_audit_logs table with indexes
- Migration Down(): Rollback support

**Tests created/changed:**
- (Migration tested implicitly via integration tests)

**Schema Changes:**
- **users table:**
  - `role` (INTEGER, default: 0)
  - `is_deleted` (BOOLEAN, default: false)
  - `deleted_at` (DATETIME, nullable)
- **admin_audit_logs table (new):**
  - `id` (GUID, PK)
  - `admin_user_id` (GUID)
  - `action` (INTEGER)
  - `target_user_id` (GUID, nullable)
  - `details` (TEXT, max 2000)
  - `timestamp` (DATETIME)
  - Indexes: timestamp (desc), admin_user_id, action

**Review Status:** APPROVED

**Git Commit Message:**
```
feat: add database migration for admin features

- Add role, is_deleted, deleted_at columns to users table
- Add global query filter for soft delete (auto-exclude deleted users)
- Create admin_audit_logs table with performance indexes
- Configure snake_case column naming for all new fields
```
