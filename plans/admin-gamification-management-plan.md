## Plan: Admin Gamification Management

Add a new "Gamification" tab to the Admin Portal with sub-tabs for managing Achievements, Challenge Templates, and Cosmetics. This includes backend CRUD endpoints with audit logging, repository updates, frontend inline-editable tables, and comprehensive tests.

**Decisions:**
1. Prevent deletion if entities are in use (referenced by users)
2. Challenge Templates remain editable after challenges are created from them
3. Use inline editing in tables (not modal-only)
4. Validate JSON format for cosmetic codes and achievement unlock conditions

**Phases: 6**

1. **Phase 1: Backend DTOs & Enums**
    - **Objective:** Create DTOs and extend enums for admin gamification operations
    - **Files/Functions to Create:** `AdminGamificationDtos.cs`
    - **Files/Functions to Modify:** `AdminAction.cs`
    - **Tests to Write:** N/A (DTOs)
    - **Steps:**
        1. Create `AdminGamificationDtos.cs` with request/response records
        2. Add new `AdminAction` enum values for gamification CRUD operations

2. **Phase 2: Backend Repository Updates**
    - **Objective:** Add CRUD and pagination methods to the gamification repository
    - **Files/Functions to Modify:** `IGamificationRepository.cs`, `GamificationRepository.cs`
    - **Tests to Write:** `GamificationRepositoryTests.cs` (pagination, CRUD methods)
    - **Steps:**
        1. Write tests for new repository methods
        2. Add interface methods for paginated queries, add, update, check-in-use
        3. Implement repository methods
        4. Run tests to verify

3. **Phase 3: Backend Admin Endpoints**
    - **Objective:** Create admin API endpoints for managing gamification entities
    - **Files/Functions to Create:** `AdminGamificationEndpoints.cs`
    - **Files/Functions to Modify:** `Program.cs`
    - **Tests to Write:** `AdminGamificationEndpointsTests.cs`
    - **Steps:**
        1. Write integration tests for the new endpoints
        2. Create endpoints with CRUD for achievements, templates, cosmetics
        3. Register endpoints in `Program.cs`
        4. Run tests to verify

4. **Phase 4: Frontend Types & API Client**
    - **Objective:** Add TypeScript types and API client functions
    - **Files/Functions to Create:** `admin-gamification.ts` (types), `admin-gamification-client.ts`
    - **Files/Functions to Modify:** `types/index.ts`, `lib/api/index.ts`
    - **Tests to Write:** N/A (covered by component tests)
    - **Steps:**
        1. Create TypeScript interfaces matching backend DTOs
        2. Create API client with CRUD functions
        3. Export from index files

5. **Phase 5: Frontend Components**
    - **Objective:** Build inline-editable table components for gamification management
    - **Files/Functions to Create:** `AchievementsTable.tsx`, `ChallengeTemplatesTable.tsx`, `CosmeticsTable.tsx`, `GamificationTab.tsx`
    - **Files/Functions to Modify:** `admin/page.tsx`, `components/admin/index.ts`
    - **Tests to Write:** Component tests for each table
    - **Steps:**
        1. Write component tests first (TDD)
        2. Create table components with inline editing
        3. Create `GamificationTab.tsx` with sub-tabs
        4. Add "Gamification" tab to admin page
        5. Run tests

6. **Phase 6: Frontend Tests & Polish**
    - **Objective:** Complete test coverage and polish UI
    - **Files/Functions to Create:** Test files for all new components
    - **Steps:**
        1. Write comprehensive tests for tables
        2. Write tests for inline editing operations
        3. Verify all tests pass
        4. Final cleanup and lint

**Open Questions:** All resolved per user input.
