## Phase 6 Complete: Add Admin Bootstrap Configuration

Configured admin user bootstrap via appsettings so that specific email addresses are automatically granted admin role on login.

**Files created/changed:**
- backend/src/PerfectFit.Core/Configuration/AdminSettings.cs (new)
- backend/src/PerfectFit.Core/Entities/User.cs
- backend/src/PerfectFit.Web/appsettings.json
- backend/src/PerfectFit.Web/appsettings.Development.json
- backend/src/PerfectFit.Web/Program.cs
- backend/src/PerfectFit.UseCases/Auth/OAuthLoginCommandHandler.cs
- backend/src/PerfectFit.UseCases/PerfectFit.UseCases.csproj
- backend/tests/PerfectFit.UnitTests/Core/Entities/UserTests.cs
- backend/tests/PerfectFit.UnitTests/UseCases/Auth/OAuthLoginCommandHandlerTests.cs
- backend/tests/PerfectFit.IntegrationTests/Web/Endpoints/AuthEndpointsTests.cs (new)

**Functions created/changed:**
- `AdminSettings` class with SectionName and Emails list
- `User.SetRole(UserRole role)` method for updating user role
- `OAuthLoginCommandHandler` - checks admin emails on login, promotes users automatically
- Program.cs - registers AdminSettings options

**Configuration added:**
- `Admin:Emails` in appsettings.json with "tomer.ros1@gmail.com"

**Tests created/changed:**
- `UserTests.SetRole_ShouldUpdateRole`
- `UserTests.SetRole_ToAdmin_ShouldSetAdminRole`
- `AdminSettingsTests.Emails_ShouldDefaultToEmptyList`
- `OAuthLoginCommandHandlerTests` - 5 new tests for admin email handling
- `AuthEndpointsTests` - 3 integration tests

**Review Status:** APPROVED

**Git Commit Message:**
```
feat: add admin bootstrap configuration via appsettings

- Add AdminSettings class with configurable admin email list
- Add User.SetRole() method for role updates
- Configure tomer.ros1@gmail.com as initial admin
- Auto-promote users to admin on login if email matches config
- Use case-insensitive email comparison
- Add unit and integration tests for admin bootstrap
```
