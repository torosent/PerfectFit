## Phase 1 Complete: Backend DTOs & Enums

Created data transfer objects and extended enums for admin gamification CRUD operations.

**Files created/changed:**
- backend/src/PerfectFit.Web/DTOs/AdminGamificationDtos.cs (new)
- backend/src/PerfectFit.Core/Enums/AdminAction.cs (modified)

**Functions created/changed:**
- AdminAchievementDto record
- CreateAchievementRequest record
- UpdateAchievementRequest record
- AdminChallengeTemplateDto record
- CreateChallengeTemplateRequest record
- UpdateChallengeTemplateRequest record
- AdminCosmeticDto record
- CreateCosmeticRequest record
- UpdateCosmeticRequest record
- EntityInUseResponse record
- AdminAction enum (12 new values: ViewAchievements, CreateAchievement, UpdateAchievement, DeleteAchievement, ViewChallengeTemplates, CreateChallengeTemplate, UpdateChallengeTemplate, DeleteChallengeTemplate, ViewCosmetics, CreateCosmetic, UpdateCosmetic, DeleteCosmetic)

**Tests created/changed:**
- N/A (DTOs don't require unit tests)

**Review Status:** APPROVED

**Git Commit Message:**
```
feat: add admin gamification DTOs and audit action enums

- Add AdminGamificationDtos.cs with request/response records for
  achievements, challenge templates, and cosmetics
- Extend AdminAction enum with CRUD actions for gamification entities
- Add EntityInUseResponse for deletion prevention scenarios
```
