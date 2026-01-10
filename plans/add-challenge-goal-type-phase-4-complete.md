## Phase 4 Complete: Update Progress Calculation

Updated challenge progress calculation to use GoalType enum with fallback to description parsing.

**Files created:**
- backend/src/PerfectFit.UseCases/Gamification/Commands/ChallengeProgressCalculator.cs
- backend/tests/PerfectFit.UnitTests/UseCases/Gamification/Commands/ChallengeProgressCalculationTests.cs

**Files changed:**
- backend/src/PerfectFit.UseCases/Gamification/Commands/ProcessGameEndGamificationCommand.cs

**Functions created/changed:**
- `ChallengeProgressCalculator.CalculateChallengeProgress()` - Main method that checks GoalType first, then falls back to description parsing
- `ChallengeProgressCalculator.CalculateTimeProgress()` - Helper for time-based challenges
- `ChallengeProgressCalculator.CalculateProgressFromDescription()` - Legacy fallback with fixed ordering (more specific patterns first)

**GoalType Progress Logic:**
- ScoreTotal → returns gameSession.Score
- ScoreSingleGame → returns 1 if score >= target, 0 otherwise
- GameCount → returns 1
- WinStreak → returns 1 (streak tracking handled elsewhere)
- Accuracy → returns 1 (accuracy tracking handled elsewhere)
- TimeBased → returns game duration in minutes (ceiling)

**Tests created:**
- 14 unit tests covering all GoalType values and fallback scenarios
- Tests for ScoreTotal, ScoreSingleGame, GameCount, WinStreak, Accuracy, TimeBased
- Tests for null GoalType fallback to description parsing
- Tests for time-based edge cases (partial minutes, no EndedAt)

**Review Status:** APPROVED (after revision to fix fallback ordering)

**Git Commit Message:**
```
feat: use GoalType enum for challenge progress calculation

- Create ChallengeProgressCalculator static class
- Check GoalType first, fall back to description parsing
- Fix fallback parsing order (specific patterns before general)
- Add 14 unit tests for all GoalType values and fallback
- All 875 tests passing (768 unit + 107 integration)
```
