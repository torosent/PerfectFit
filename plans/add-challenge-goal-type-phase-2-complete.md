## Phase 2 Complete: Update DTOs and Admin Endpoints

Added GoalType to admin DTOs and endpoint handlers for challenge template management.

**Files changed:**
- backend/src/PerfectFit.Web/Endpoints/AdminGamificationEndpoints.cs
- backend/tests/PerfectFit.IntegrationTests/Endpoints/AdminGamificationEndpointsTests.cs

**Functions created/changed:**
- `AdminChallengeTemplateDto` - Added GoalType property
- `CreateChallengeTemplateRequest` - Added GoalType property
- `UpdateChallengeTemplateRequest` - Added GoalType property
- `CreateChallengeTemplate` handler - Passes GoalType to entity
- `UpdateChallengeTemplate` handler - Passes GoalType to entity
- DTO mapping - Includes GoalType in response

**Tests created:**
- CreateChallengeTemplate_WithGoalType_ReturnsGoalTypeInResponse
- CreateChallengeTemplate_WithoutGoalType_ReturnsNullGoalType
- UpdateChallengeTemplate_GoalType_UpdatesSuccessfully
- GetChallengeTemplates_IncludesGoalTypeInResponse
- GetChallengeTemplate_SingleById_IncludesGoalType

**Review Status:** APPROVED

**Git Commit Message:**
```
feat: expose GoalType in admin challenge template API

- Add GoalType to AdminChallengeTemplateDto response
- Add GoalType to CreateChallengeTemplateRequest
- Add GoalType to UpdateChallengeTemplateRequest
- Update create/update handlers to pass GoalType to entity
- Add 5 integration tests for GoalType CRUD operations
```
