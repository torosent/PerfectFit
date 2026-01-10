## Phase 1 Complete: Create ChallengeGoalType Enum and Update Entities

Added the ChallengeGoalType enum and GoalType property to backend entities for explicit challenge goal configuration.

**Files created:**
- backend/src/PerfectFit.Core/Enums/ChallengeGoalType.cs
- backend/tests/PerfectFit.UnitTests/Entities/ChallengeGoalTypeTests.cs

**Files changed:**
- backend/src/PerfectFit.Core/Entities/ChallengeTemplate.cs
- backend/src/PerfectFit.Core/Entities/Challenge.cs

**Functions created/changed:**
- `ChallengeGoalType` enum with 6 values: ScoreTotal, ScoreSingleGame, GameCount, WinStreak, Accuracy, TimeBased
- `ChallengeTemplate.Create()` - Added optional goalType parameter
- `ChallengeTemplate.Update()` - Added optional goalType parameter
- `ChallengeTemplate.GoalType` property (nullable)
- `Challenge.GoalType` property (nullable)
- `Challenge.CreateFromTemplate()` - Updated to copy GoalType from template

**Tests created:**
- ChallengeGoalType_ShouldHaveExpectedValues
- ChallengeGoalType_ShouldHaveExpectedCount
- ChallengeTemplate_Create_WithGoalType_ShouldSetGoalType
- ChallengeTemplate_Create_WithoutGoalType_ShouldHaveNullGoalType
- ChallengeTemplate_Update_ShouldUpdateGoalType
- Challenge_Create_WithGoalType_ShouldSetGoalType
- Challenge_Create_WithoutGoalType_ShouldHaveNullGoalType
- Challenge_CreateFromTemplate_ShouldCopyGoalType
- (8 additional tests for entity properties and edge cases)

**Review Status:** APPROVED (with note)

Note: The Update method uses `goalType = null` as default. This is acceptable because the admin endpoint will always explicitly pass the GoalType value from the DTO.

**Git Commit Message:**
```
feat: add ChallengeGoalType enum to challenge templates

- Create ChallengeGoalType enum with 6 goal types (ScoreTotal, ScoreSingleGame, GameCount, WinStreak, Accuracy, TimeBased)
- Add nullable GoalType property to ChallengeTemplate entity
- Add nullable GoalType property to Challenge entity  
- Update Create/Update methods to accept GoalType parameter
- Update CreateFromTemplate to copy GoalType from template
- Add 16 unit tests for new enum and entity changes
```
