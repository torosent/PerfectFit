## Plan: Add GoalType Field to Challenge Templates

Add an explicit `ChallengeGoalType` enum to challenge templates, replacing the text-based description parsing with a structured field that can be set in the Admin UI. The field will be nullable for backward compatibility.

**Phases (6 phases)**

1. **Phase 1: Create ChallengeGoalType Enum and Update Entities**
    - **Objective:** Add the new enum and property to backend entities
    - **Files/Functions to Create/Modify:**
      - Create `ChallengeGoalType.cs` enum in `PerfectFit.Core/Enums`
      - Update `ChallengeTemplate.cs` - Add `GoalType` property
      - Update `Challenge.cs` - Add `GoalType` property
    - **Tests to Write:** `ChallengeGoalTypeTests.cs`, update entity tests
    - **Steps:**
      1. Write unit tests for new enum values and entity properties
      2. Create `ChallengeGoalType` enum with values: `ScoreTotal`, `ScoreSingleGame`, `GameCount`, `WinStreak`, `Accuracy`, `TimeBased`
      3. Add nullable `GoalType` property to `ChallengeTemplate` entity with Update method
      4. Add nullable `GoalType` property to `Challenge` entity
      5. Run tests to verify

2. **Phase 2: Update DTOs and Admin Endpoints**
    - **Objective:** Expose GoalType in admin API
    - **Files/Functions to Modify:**
      - `AdminGamificationEndpoints.cs` - DTOs and handlers
    - **Tests to Write:** Update integration tests for create/update endpoints
    - **Steps:**
      1. Write/update tests expecting GoalType in DTOs
      2. Add `GoalType` to `AdminChallengeTemplateDto`, `CreateChallengeTemplateRequest`, `UpdateChallengeTemplateRequest`
      3. Update Create handler to pass GoalType to entity
      4. Update Update handler to pass GoalType to entity
      5. Run tests

3. **Phase 3: Update Challenge Creation Jobs**
    - **Objective:** Copy GoalType when creating challenges from templates
    - **Files/Functions to Modify:**
      - `DailyChallengeRotationJob.cs`
      - `WeeklyChallengeRotationJob.cs`
      - `Challenge.cs` - Update `CreateFromTemplate` method
    - **Tests to Write:** Update rotation job tests
    - **Steps:**
      1. Write tests verifying GoalType is copied from template to challenge
      2. Update `Challenge.CreateFromTemplate` to include GoalType parameter
      3. Update rotation jobs to pass GoalType
      4. Run tests

4. **Phase 4: Update Progress Calculation Logic**
    - **Objective:** Use GoalType enum instead of description parsing (with fallback)
    - **Files/Functions to Modify:**
      - `ProcessGameEndGamificationCommand.cs`
    - **Tests to Write:** Tests for each GoalType progress calculation
    - **Steps:**
      1. Write tests for each GoalType calculating correct progress
      2. Update `CalculateChallengeProgress` to check GoalType first
      3. Keep description parsing as fallback when GoalType is null
      4. Run all tests

5. **Phase 5: Create Database Migration**
    - **Objective:** Add GoalType column to database
    - **Files/Functions to Create:**
      - New migration file in `Migrations`
    - **Tests to Write:** None (migration tested by EF tooling)
    - **Steps:**
      1. Add EF Core configuration for GoalType as nullable int
      2. Generate migration using `dotnet ef migrations add AddChallengeGoalType`
      3. Verify migration script is correct

6. **Phase 6: Update Frontend UI and Seed Data**
    - **Objective:** Add GoalType dropdown to admin form
    - **Files/Functions to Modify:**
      - `types/index.ts` - Add ChallengeGoalType type
      - `ChallengeTemplatesTable.tsx` - Add dropdown
      - `ChallengeSeedData.cs` - Update seed data with GoalType values
    - **Tests to Write:** Update frontend component tests
    - **Steps:**
      1. Write tests for GoalType dropdown rendering and selection
      2. Add `ChallengeGoalType` union type to frontend types
      3. Add GoalType dropdown to create/edit form in table
      4. Display GoalType in table rows
      5. Update seed data to include GoalType values
      6. Run frontend tests

**Decisions:**
1. GoalType is **optional (nullable)** for backward compatibility
2. Seed data will be updated to include GoalType values
