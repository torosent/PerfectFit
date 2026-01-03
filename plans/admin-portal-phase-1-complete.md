## Phase 1 Complete: Add Role Enum and Update User Entity

Added UserRole enum and soft delete capabilities to the User entity to support admin authorization and user management.

**Files created/changed:**
- backend/src/PerfectFit.Core/Enums/UserRole.cs (new)
- backend/src/PerfectFit.Core/Entities/User.cs
- backend/tests/PerfectFit.UnitTests/Core/Entities/UserTests.cs

**Functions created/changed:**
- `UserRole` enum with `User=0`, `Admin=1` values
- `User.Role` property (default: UserRole.User)
- `User.IsDeleted` property (default: false)
- `User.DeletedAt` property (default: null)
- `User.Create()` factory method - added optional role parameter
- `User.SoftDelete()` method - sets IsDeleted and DeletedAt

**Tests created/changed:**
- `Create_WithRole_ShouldSetRole`
- `Create_DefaultRole_ShouldBeUser`
- `SoftDelete_ShouldSetIsDeletedAndTimestamp`
- `Create_ShouldSetIsDeletedToFalse`
- `Create_ShouldSetDeletedAtToNull`

**Review Status:** APPROVED

**Git Commit Message:**
```
feat: add user role enum and soft delete support

- Add UserRole enum with User and Admin values
- Add Role, IsDeleted, DeletedAt properties to User entity
- Add optional role parameter to User.Create() factory
- Add SoftDelete() method for soft deletion
- Add unit tests for new functionality (23/23 passing)
```
