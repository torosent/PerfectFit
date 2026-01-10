## Plan: Gamification System Implementation

Implement a comprehensive gamification system with daily/weekly challenges, streak tracking with freeze tokens, 7-day season pass progression, achievements/badges, board themes, avatar frames, and personalized goals. Uses hybrid approach: core stats on User entity plus normalized entities for achievements, challenges, and cosmetics. All features secured server-side to prevent cheating.

**Key Decisions:**
- Challenges: Global (same for all users)
- Streak reset: User's local timezone
- Season duration: 7 days
- Cosmetics: Board themes, avatar frames, badges
- Streak freeze acquisition: Achievement rewards + season pass rewards
- Security: All validation server-side, anti-cheat measures

---

**Phases (8 phases)**

### 1. Phase 1: Database Foundation & Entities
- **Objective:** Create all new entities and extend User entity for gamification tracking
- **Files/Functions to Modify/Create:**
  - Extend `PerfectFit.Core/Entities/User.cs` with streak fields (`CurrentStreak`, `LongestStreak`, `StreakFreezeTokens`, `LastPlayedDate`, `Timezone`), season pass fields (`SeasonPassXP`, `CurrentSeasonTier`), and cosmetic fields (`EquippedBoardTheme`, `EquippedAvatarFrame`, `EquippedBadge`)
  - Create `PerfectFit.Core/Entities/Achievement.cs` - global achievement definitions with unlock conditions
  - Create `PerfectFit.Core/Entities/UserAchievement.cs` - tracks user's unlocked achievements
  - Create `PerfectFit.Core/Entities/Challenge.cs` - global daily/weekly challenge definitions
  - Create `PerfectFit.Core/Entities/UserChallenge.cs` - tracks user's challenge progress
  - Create `PerfectFit.Core/Entities/Season.cs` - season definition with start/end dates
  - Create `PerfectFit.Core/Entities/SeasonReward.cs` - rewards at each tier
  - Create `PerfectFit.Core/Entities/Cosmetic.cs` - board themes, avatar frames, badges
  - Create `PerfectFit.Core/Entities/UserCosmetic.cs` - user's owned cosmetics
  - Create `PerfectFit.Core/Entities/PersonalGoal.cs` - user's personalized goals
  - Create `PerfectFit.Core/Enums/ChallengeType.cs` - Daily, Weekly
  - Create `PerfectFit.Core/Enums/AchievementCategory.cs` - Score, Streak, Games, Challenge, Special
  - Create `PerfectFit.Core/Enums/CosmeticType.cs` - BoardTheme, AvatarFrame, Badge
  - Create `PerfectFit.Core/Enums/GoalType.cs` - BeatAverage, ImproveAccuracy, NewPersonalBest
  - Create `PerfectFit.Core/Enums/RewardType.cs` - Cosmetic, StreakFreeze, XPBoost
  - Update `PerfectFit.Infrastructure/Data/AppDbContext.cs` with new DbSets and entity configurations
  - Create EF Core migration for new tables
- **Tests to Write:**
  - `UserGamificationTests.cs` - streak updates, freeze token usage, cosmetic equipping
  - `AchievementEntityTests.cs` - achievement creation, unlock conditions
  - `ChallengeEntityTests.cs` - challenge creation, progress tracking
  - `SeasonEntityTests.cs` - season creation, tier calculation
  - `CosmeticEntityTests.cs` - cosmetic creation, ownership tracking
- **Steps:**
  1. Write tests for User gamification extensions (expect failures)
  2. Write tests for new entities (expect failures)
  3. Create new enum files
  4. Extend User entity with gamification fields and methods
  5. Create Achievement and UserAchievement entities
  6. Create Challenge and UserChallenge entities
  7. Create Season and SeasonReward entities
  8. Create Cosmetic and UserCosmetic entities
  9. Create PersonalGoal entity
  10. Update AppDbContext with DbSets and configurations
  11. Run tests to confirm they pass

### 2. Phase 2: Core Gamification Interfaces & Services
- **Objective:** Create interfaces and domain services for gamification logic with anti-cheat validation
- **Files/Functions to Modify/Create:**
  - Create `PerfectFit.Core/Interfaces/IStreakService.cs` - interface for streak operations
  - Create `PerfectFit.Core/Interfaces/IChallengeService.cs` - interface for challenge operations
  - Create `PerfectFit.Core/Interfaces/IAchievementService.cs` - interface for achievement operations
  - Create `PerfectFit.Core/Interfaces/ISeasonPassService.cs` - interface for season pass operations
  - Create `PerfectFit.Core/Interfaces/ICosmeticService.cs` - interface for cosmetic operations
  - Create `PerfectFit.Core/Interfaces/IPersonalGoalService.cs` - interface for goal operations
  - Create `PerfectFit.Core/Services/StreakService.cs` - streak increment, freeze usage, timezone-aware expiry
  - Create `PerfectFit.Core/Services/ChallengeService.cs` - challenge completion, progress tracking
  - Create `PerfectFit.Core/Services/AchievementService.cs` - unlock conditions, progress calculation
  - Create `PerfectFit.Core/Services/SeasonPassService.cs` - XP gain, tier calculation, reward claiming
  - Create `PerfectFit.Core/Services/CosmeticService.cs` - ownership validation, equipping
  - Create `PerfectFit.Core/Services/PersonalGoalService.cs` - goal generation based on history
  - Update `PerfectFit.Infrastructure/DependencyInjection.cs` to register new services
- **Tests to Write:**
  - `StreakServiceTests.cs` - streak increment, freeze usage, timezone expiry, anti-cheat
  - `ChallengeServiceTests.cs` - completion validation, progress tracking
  - `AchievementServiceTests.cs` - unlock conditions, progress calculation
  - `SeasonPassServiceTests.cs` - XP gain, tier unlocks, reward claiming
  - `CosmeticServiceTests.cs` - ownership validation, equipping rules
  - `PersonalGoalServiceTests.cs` - goal generation logic
- **Steps:**
  1. Write tests for StreakService (expect failures)
  2. Create IStreakService interface
  3. Implement StreakService with timezone-aware logic
  4. Run tests to confirm they pass
  5. Repeat for ChallengeService, AchievementService, SeasonPassService, CosmeticService, PersonalGoalService
  6. Register all services in DependencyInjection

### 3. Phase 3: CQRS Commands & Queries for Gamification
- **Objective:** Create MediatR handlers for gamification operations
- **Files/Functions to Modify/Create:**
  - Create `PerfectFit.UseCases/Gamification/` folder structure
  - Create `Commands/UpdateStreakCommand.cs` - updates user streak after game
  - Create `Commands/UseStreakFreezeCommand.cs` - uses freeze token to save streak
  - Create `Commands/CompleteChallengeCommand.cs` - marks challenge as complete
  - Create `Commands/ClaimSeasonRewardCommand.cs` - claims tier reward
  - Create `Commands/EquipCosmeticCommand.cs` - equips owned cosmetic
  - Create `Commands/SetTimezoneCommand.cs` - sets user's timezone
  - Create `Queries/GetGamificationStatusQuery.cs` - full gamification state
  - Create `Queries/GetChallengesQuery.cs` - active daily/weekly challenges
  - Create `Queries/GetAchievementsQuery.cs` - all achievements with progress
  - Create `Queries/GetSeasonPassQuery.cs` - current season with progress
  - Create `Queries/GetCosmeticsQuery.cs` - owned and available cosmetics
  - Create `Queries/GetPersonalGoalsQuery.cs` - personalized goals
  - Update `PerfectFit.UseCases/Games/Commands/EndGameCommand.cs` - integrate gamification updates
- **Tests to Write:**
  - `UpdateStreakCommandTests.cs` - streak update validation
  - `UseStreakFreezeCommandTests.cs` - freeze usage rules
  - `CompleteChallengeCommandTests.cs` - completion validation, anti-cheat
  - `ClaimSeasonRewardCommandTests.cs` - claim validation
  - `GetGamificationStatusQueryTests.cs` - full state retrieval
  - `EndGameGamificationIntegrationTests.cs` - game end triggers gamification
- **Steps:**
  1. Write tests for UpdateStreakCommand (expect failures)
  2. Implement UpdateStreakCommand handler
  3. Run tests to confirm they pass
  4. Repeat for all commands and queries
  5. Update EndGameCommand to trigger gamification updates
  6. Write integration tests for full flow

### 4. Phase 4: API Endpoints & DTOs
- **Objective:** Create REST endpoints for gamification features with authorization
- **Files/Functions to Modify/Create:**
  - Create `PerfectFit.Web/DTOs/GamificationStatusDto.cs` - full gamification state
  - Create `PerfectFit.Web/DTOs/ChallengeDto.cs` - challenge with progress
  - Create `PerfectFit.Web/DTOs/AchievementDto.cs` - achievement with progress
  - Create `PerfectFit.Web/DTOs/SeasonPassDto.cs` - season with tiers and rewards
  - Create `PerfectFit.Web/DTOs/CosmeticDto.cs` - cosmetic details
  - Create `PerfectFit.Web/DTOs/PersonalGoalDto.cs` - goal details
  - Create `PerfectFit.Web/Endpoints/GamificationEndpoints.cs` - main gamification routes
  - Routes: GET /api/gamification (status), GET /api/gamification/challenges, GET /api/gamification/achievements, GET /api/gamification/season-pass, GET /api/gamification/cosmetics, POST /api/gamification/streak-freeze, POST /api/gamification/equip-cosmetic, POST /api/gamification/claim-reward, POST /api/gamification/timezone
  - Update game end response to include gamification updates
- **Tests to Write:**
  - `GamificationEndpointsIntegrationTests.cs` - all endpoints
  - `GamificationAuthorizationTests.cs` - unauthorized access blocked
- **Steps:**
  1. Write integration tests for GET /api/gamification (expect failures)
  2. Create GamificationStatusDto
  3. Create GamificationEndpoints with GET status route
  4. Run tests to confirm they pass
  5. Repeat for all endpoints and DTOs
  6. Add authorization checks to all endpoints

### 5. Phase 5: Frontend Types & API Client
- **Objective:** Create TypeScript types and API client functions for gamification
- **Files/Functions to Modify/Create:**
  - Create `frontend/src/types/gamification.ts` - all gamification types (GamificationStatus, Challenge, Achievement, SeasonPass, Cosmetic, PersonalGoal, etc.)
  - Create `frontend/src/lib/api/gamification.ts` - API client functions (getGamificationStatus, getChallenges, getAchievements, getSeasonPass, getCosmetics, useStreakFreeze, equipCosmetic, claimReward, setTimezone)
  - Update `frontend/src/types/game.ts` - add gamification updates to game end response
- **Tests to Write:**
  - `gamification-api.test.ts` - API client tests with mocked responses
- **Steps:**
  1. Write tests for gamification API client (expect failures)
  2. Create gamification types
  3. Create API client functions
  4. Update game types
  5. Run tests to confirm they pass

### 6. Phase 6: Frontend State & Hooks
- **Objective:** Create Zustand store and hooks for gamification state
- **Files/Functions to Modify/Create:**
  - Create `frontend/src/stores/gamificationStore.ts` - Zustand store with gamification state, actions, selectors
  - Create `frontend/src/hooks/useGamification.ts` - main gamification hook
  - Create `frontend/src/hooks/useStreaks.ts` - streak-specific hook
  - Create `frontend/src/hooks/useChallenges.ts` - challenges hook
  - Create `frontend/src/hooks/useAchievements.ts` - achievements hook
  - Create `frontend/src/hooks/useSeasonPass.ts` - season pass hook
  - Create `frontend/src/hooks/usePersonalGoals.ts` - personal goals hook
  - Create `frontend/src/hooks/useCosmetics.ts` - cosmetics hook
  - Update game store to trigger gamification updates on game end
- **Tests to Write:**
  - `gamificationStore.test.ts` - store actions and selectors
  - `useGamification.test.ts` - hook behavior
- **Steps:**
  1. Write tests for gamification store (expect failures)
  2. Create gamificationStore with state and actions
  3. Create hooks for each feature
  4. Update game store integration
  5. Run tests to confirm they pass

### 7. Phase 7: Frontend Components
- **Objective:** Create beautiful UI components with animations for gamification features
- **Files/Functions to Modify/Create:**
  - Create `frontend/src/components/gamification/StreakDisplay.tsx` - animated fire streak counter with freeze indicator
  - Create `frontend/src/components/gamification/StreakFreezeButton.tsx` - use freeze token UI
  - Create `frontend/src/components/gamification/ChallengeCard.tsx` - individual challenge with progress bar
  - Create `frontend/src/components/gamification/ChallengesList.tsx` - daily/weekly challenges panel
  - Create `frontend/src/components/gamification/AchievementBadge.tsx` - unlocked/locked badge with tooltip
  - Create `frontend/src/components/gamification/AchievementsPanel.tsx` - categorized achievements grid
  - Create `frontend/src/components/gamification/AchievementUnlockModal.tsx` - celebration animation on unlock
  - Create `frontend/src/components/gamification/SeasonPassTrack.tsx` - horizontal progress track with rewards
  - Create `frontend/src/components/gamification/SeasonRewardCard.tsx` - individual tier reward
  - Create `frontend/src/components/gamification/SeasonEndRecap.tsx` - end-of-season summary
  - Create `frontend/src/components/gamification/PersonalGoalPrompt.tsx` - goal notification toast
  - Create `frontend/src/components/gamification/PersonalGoalCard.tsx` - goal with progress
  - Create `frontend/src/components/gamification/GameEndSummary.tsx` - post-game gamification recap
  - Create `frontend/src/components/gamification/CosmeticSelector.tsx` - cosmetic selection UI
  - Create `frontend/src/components/gamification/BoardThemePreview.tsx` - board theme preview
  - Create `frontend/src/components/gamification/AvatarFramePreview.tsx` - avatar frame preview
  - Create `frontend/src/components/gamification/GamificationDashboard.tsx` - main gamification page
  - Update game layout to show streak, daily challenge status in header
  - Update profile page with achievements, cosmetics sections
- **Tests to Write:**
  - `StreakDisplay.test.tsx` - renders correctly, animations
  - `ChallengeCard.test.tsx` - progress display, completion
  - `SeasonPassTrack.test.tsx` - tier progression, reward claiming
  - `GameEndSummary.test.tsx` - gamification updates display
  - `AchievementUnlockModal.test.tsx` - animation and close
- **Steps:**
  1. Write tests for StreakDisplay (expect failures)
  2. Create StreakDisplay with fire animation
  3. Run tests to confirm they pass
  4. Repeat for all components
  5. Create GamificationDashboard page
  6. Integrate components into game layout and profile
  7. Add CSS animations and transitions

### 8. Phase 8: Background Jobs & Seed Data
- **Objective:** Implement scheduled jobs for challenge rotation and season transitions, seed initial data
- **Files/Functions to Modify/Create:**
  - Create `PerfectFit.Core/Interfaces/IBackgroundJobService.cs` - background job interface
  - Create `PerfectFit.Infrastructure/Services/DailyChallengeRotationJob.cs` - rotates daily challenges at midnight UTC
  - Create `PerfectFit.Infrastructure/Services/WeeklyChallengeRotationJob.cs` - rotates weekly challenges on Monday
  - Create `PerfectFit.Infrastructure/Services/SeasonTransitionJob.cs` - ends season, creates new season, generates recap
  - Create `PerfectFit.Infrastructure/Services/StreakExpiryNotificationJob.cs` - notifies users about expiring streaks
  - Create `PerfectFit.Infrastructure/Data/SeedData/AchievementSeedData.cs` - initial achievements
  - Create `PerfectFit.Infrastructure/Data/SeedData/CosmeticSeedData.cs` - initial cosmetics
  - Create `PerfectFit.Infrastructure/Data/SeedData/SeasonSeedData.cs` - first season with rewards
  - Create `PerfectFit.Infrastructure/Data/SeedData/ChallengeSeedData.cs` - challenge templates
  - Update `Program.cs` to register hosted services and seed data
- **Tests to Write:**
  - `DailyChallengeRotationJobTests.cs` - rotation logic
  - `SeasonTransitionJobTests.cs` - transition and recap generation
  - `StreakExpiryNotificationJobTests.cs` - notification timing
  - `SeedDataTests.cs` - seed data validity
- **Steps:**
  1. Write tests for DailyChallengeRotationJob (expect failures)
  2. Create IBackgroundJobService interface
  3. Implement DailyChallengeRotationJob
  4. Run tests to confirm they pass
  5. Repeat for other jobs
  6. Create seed data files
  7. Update Program.cs with hosted services
  8. Create EF migration for seed data
  9. Run full integration test

---

**Security Measures (Applied Throughout):**
- All gamification updates validated server-side
- Challenge completions verified against actual game data
- Streak updates only from authenticated game sessions
- Cosmetic equipping requires ownership verification
- Season rewards claimable only once per tier
- Anti-cheat integration with existing GameSession validation
- Rate limiting on gamification endpoints
