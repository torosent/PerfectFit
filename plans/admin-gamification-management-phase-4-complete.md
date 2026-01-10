## Phase 4 Complete: Frontend Types & API Client

Added TypeScript types and API client functions for admin gamification CRUD operations.

**Files created/changed:**
- frontend/src/types/admin-gamification.ts (new)
- frontend/src/lib/api/admin-gamification-client.ts (new)
- frontend/src/types/index.ts (modified)
- frontend/src/lib/api/index.ts (modified)

**Functions created/changed:**
- AdminAchievement, CreateAchievementRequest, UpdateAchievementRequest types
- AdminChallengeTemplate, CreateChallengeTemplateRequest, UpdateChallengeTemplateRequest types
- AdminCosmetic, CreateCosmeticRequest, UpdateCosmeticRequest types
- EntityInUseResponse type
- getAchievements, getAchievement, createAchievement, updateAchievement, deleteAchievement
- getChallengeTemplates, getChallengeTemplate, createChallengeTemplate, updateChallengeTemplate, deleteChallengeTemplate
- getCosmetics, getCosmetic, createCosmetic, updateCosmetic, deleteCosmetic

**Tests created/changed:**
- N/A (covered by component tests in Phase 6)

**Review Status:** APPROVED (after revision)

**Git Commit Message:**
```
feat: add frontend types and API client for admin gamification

- Add TypeScript interfaces for achievements, templates, and cosmetics
- Add request types for create/update operations
- Add EntityInUseResponse for 409 conflict handling
- Add 15 API client functions for CRUD operations
- Export from types and api index files
```
