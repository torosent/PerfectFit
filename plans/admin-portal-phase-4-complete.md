## Phase 4 Complete: Extend User Repository and Add Authorization

Extended user repository with admin query methods and configured JWT role claims with admin authorization policy.

**Files created/changed:**
- backend/src/PerfectFit.Core/Interfaces/IUserRepository.cs
- backend/src/PerfectFit.Infrastructure/Repositories/UserRepository.cs
- backend/src/PerfectFit.Infrastructure/Services/JwtService.cs
- backend/src/PerfectFit.Web/Program.cs
- backend/tests/PerfectFit.UnitTests/Infrastructure/Services/JwtServiceTests.cs
- backend/tests/PerfectFit.IntegrationTests/Infrastructure/Repositories/UserRepositoryTests.cs

**Functions created/changed:**
- `IUserRepository.GetAllAsync(int page, int pageSize, CancellationToken)` - Paginated user retrieval
- `IUserRepository.GetCountAsync(CancellationToken)` - Total user count
- `IUserRepository.GetDeletedUsersAsync(int page, int pageSize, CancellationToken)` - Get soft-deleted users
- `IUserRepository.SoftDeleteAsync(Guid id, CancellationToken)` - Soft delete by ID
- `IUserRepository.BulkSoftDeleteByProviderAsync(AuthProvider, CancellationToken)` - Bulk delete by provider
- `JwtService.GenerateToken()` - Added ClaimTypes.Role claim
- `Program.cs` - Added AdminPolicy requiring Admin role

**Tests created/changed:**
- `JwtServiceTests.GenerateToken_WithAdminRole_ShouldIncludeRoleClaim`
- `JwtServiceTests.GenerateToken_WithUserRole_ShouldIncludeUserRoleClaim`
- `UserRepositoryTests.GetAllAsync_ShouldReturnNonDeletedUsers`
- `UserRepositoryTests.GetAllAsync_WithPagination_ShouldReturnCorrectPage`
- `UserRepositoryTests.GetCountAsync_ShouldReturnTotalCount`
- `UserRepositoryTests.SoftDeleteAsync_ShouldMarkUserAsDeleted`
- `UserRepositoryTests.BulkSoftDeleteByProviderAsync_ShouldDeleteMatchingUsers`
- `UserRepositoryTests.GetDeletedUsersAsync_ShouldReturnOnlyDeletedUsers`

**Review Status:** APPROVED

**Git Commit Message:**
```
feat: add admin repository methods and authorization policy

- Add GetAllAsync, GetCountAsync, GetDeletedUsersAsync to IUserRepository
- Add SoftDeleteAsync and BulkSoftDeleteByProviderAsync for user deletion
- Include role claim in JWT token generation
- Add AdminPolicy requiring Admin role for protected endpoints
- Add unit and integration tests for all new functionality
```
