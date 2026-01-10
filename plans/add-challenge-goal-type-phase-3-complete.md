## Phase 3 Complete: Update Challenge Creation Jobs

Updated all challenge creation code paths to copy GoalType from templates to challenges.

**Files changed:**
- backend/src/PerfectFit.Core/Entities/Challenge.cs
- backend/src/PerfectFit.Infrastructure/Jobs/DailyChallengeRotationJob.cs
- backend/src/PerfectFit.Infrastructure/Jobs/WeeklyChallengeRotationJob.cs
- backend/src/PerfectFit.Web/Endpoints/AdminGamificationEndpoints.cs
- backend/tests/PerfectFit.UnitTests/Jobs/DailyChallengeRotationJobTests.cs
- frontend/src/lib/api/admin-gamification-client.ts

**Functions created/changed:**
- `Challenge.CreateFromTemplate()` - Now copies GoalType from template
- `DailyChallengeRotationJob` - Uses CreateFromTemplate (includes GoalType)
- `WeeklyChallengeRotationJob` - Uses CreateFromTemplate (includes GoalType)
- `ActivateChallengeResponse` - Added GoalType property
- Admin activation handler - Returns GoalType in response
- Frontend `ActivateChallengeResponse` interface - Added goalType

**Tests created/changed:**
- DailyChallengeRotationJob_CreateFromTemplate_CopiesGoalType
- DailyChallengeRotationJob_CreateFromTemplate_CopiesNullGoalType
- DailyChallengeRotationJob_CreateFromTemplate_CopiesAllGoalTypes (Theory with 6 values)

**Review Status:** APPROVED (after revision to include GoalType in activation response)

**Git Commit Message:**
```
feat: copy GoalType when creating challenges from templates

- Update Challenge.CreateFromTemplate to include GoalType
- Daily/Weekly rotation jobs use CreateFromTemplate with GoalType
- Add GoalType to ActivateChallengeResponse
- Update frontend ActivateChallengeResponse interface
- Add unit tests for GoalType copying in rotation jobs
```
