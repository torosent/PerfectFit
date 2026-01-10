# Gamification System

## Overview

PerfectFit includes a comprehensive gamification system designed to increase player engagement and retention. The system consists of daily/weekly challenges, streaks with freeze tokens, a season pass with tiered rewards, achievements, cosmetics, and personalized goals.

## Features

### Daily & Weekly Challenges

Challenges provide short-term goals that refresh regularly.

#### Daily Challenges
- **Refresh**: Every day at midnight UTC
- **Count**: 3 active daily challenges
- **Duration**: 24 hours to complete
- **Examples**:
  - Score 500 points in a single game
  - Clear 10 lines total
  - Achieve a 3x combo

#### Weekly Challenges
- **Refresh**: Every Monday at midnight UTC
- **Count**: 3 active weekly challenges
- **Duration**: 7 days to complete
- **Examples**:
  - Score 5,000 total points
  - Clear 100 lines across all games
  - Play 10 games

#### Challenge Types
| Type | Description |
|------|-------------|
| `Score` | Achieve a target score (single game or cumulative) |
| `LinesCleared` | Clear a number of lines |
| `Combo` | Achieve a specific combo multiplier |
| `GamesPlayed` | Play a certain number of games |
| `Accuracy` | Achieve high placement accuracy |
| `PerfectGame` | Complete a game with perfect moves |

---

### Streak System

Encourages daily play by tracking consecutive days of activity.

#### How It Works
1. **Start**: Play any game to begin a streak
2. **Maintain**: Play at least once per day (based on your timezone)
3. **Lose**: Missing a day resets your streak to 0
4. **Protect**: Use a Streak Freeze Token to skip a day without losing progress

#### Streak Freeze Tokens
- **Source**: Earn from achievements and season pass rewards
- **Usage**: Automatically applied when you miss a day (if available)
- **Limit**: Maximum 3 tokens stored at once
- **Manual Use**: Can be used proactively via the UI

#### Streak Rewards
| Streak Days | Bonus |
|-------------|-------|
| 7 days | 10% XP bonus |
| 14 days | 15% XP bonus |
| 30 days | 25% XP bonus |
| 100 days | 50% XP bonus |

#### Timezone Configuration
Users can set their preferred timezone for streak reset calculations:
```http
POST /api/gamification/timezone
{
  "timezone": "America/New_York"
}
```

---

### Season Pass

A 7-day rotating season with tiered progression.

#### Season Structure
- **Duration**: 7 days per season
- **Tiers**: 50 tiers per season
- **Progression**: Earn XP to advance through tiers
- **Reset**: New season starts automatically when current ends

#### Earning XP
| Activity | XP Earned |
|----------|-----------|
| Complete a game | 10-50 XP (based on score) |
| Complete daily challenge | 50-100 XP |
| Complete weekly challenge | 200-500 XP |
| Unlock achievement | 100-1000 XP |
| Maintain streak | Bonus multiplier |

#### Tier Rewards
Rewards are distributed at key milestones:

| Tier | Reward Type | Example |
|------|------------|---------|
| 1 | XP Bonus | +100 XP |
| 5 | Cosmetic | Bronze Frame |
| 10 | Streak Freeze | 1 Token |
| 15 | Cosmetic | Ocean Theme |
| 20 | XP Bonus | +500 XP |
| 25 | Cosmetic | Pro Badge |
| 30 | Streak Freeze | 2 Tokens |
| 40 | Cosmetic | Gold Frame |
| 50 | Cosmetic | Champion Badge |

#### Claiming Rewards
```http
POST /api/gamification/season-pass/claim-reward
{
  "rewardId": 15
}
```

---

### Achievements

Long-term goals that unlock cosmetics and bragging rights.

#### Categories
| Category | Description |
|----------|-------------|
| Progression | Games played, total score milestones |
| Skill | Combos, accuracy, speed achievements |
| Dedication | Streak milestones, season completions |
| Collection | Unlock all items in a category |
| Special | Limited-time or unique achievements |

#### Rarity Tiers
| Rarity | Color | XP Reward |
|--------|-------|-----------|
| Common | Gray | 100 XP |
| Uncommon | Green | 250 XP |
| Rare | Blue | 500 XP |
| Epic | Purple | 1000 XP |
| Legendary | Gold | 2500 XP |

#### Example Achievements
| Achievement | Description | Rarity |
|-------------|-------------|--------|
| First Win | Complete your first game | Common |
| Combo Master | Achieve a 5x combo | Rare |
| Century Club | Maintain a 100-day streak | Legendary |
| Speed Demon | Complete a game in under 60 seconds | Epic |
| Perfectionist | Play 10 perfect games | Epic |
| Collector | Own all cosmetics in a category | Legendary |

#### Achievement Conditions
Achievements use a JSON-based condition system:
```json
{
  "Type": "TotalWins",
  "Value": 100
}
```

Supported condition types:
- `TotalWins` - Total games completed
- `StreakDays` - Current streak length
- `SeasonTier` - Highest tier reached in a season
- `WinStreak` - Consecutive wins
- `PerfectGames` - Games with 100% accuracy
- `HighAccuracyGames` - Games with 95%+ accuracy
- `GamesUnderTime` - Fast completions
- `NightOwlGames` - Games played midnight-4AM

---

### Cosmetics

Visual customizations unlocked through gameplay.

#### Board Themes
| Theme | Unlock Method |
|-------|---------------|
| Classic | Default |
| Ocean | Season Pass Tier 15 |
| Forest | Achievement: 50 Games |
| Sunset | Season Pass Tier 30 |
| Night Sky | Achievement: 7-Day Streak |
| Galaxy | Season Pass Tier 50 |

#### Avatar Frames
| Frame | Unlock Method |
|-------|---------------|
| Bronze | Season Pass Tier 5 |
| Silver | Achievement: 100 Games |
| Gold | Season Pass Tier 40 |
| Diamond | Achievement: 30-Day Streak |
| Champion | Season completion |

#### Badges
| Badge | Unlock Method |
|-------|---------------|
| Rookie | Default |
| Pro | Season Pass Tier 25 |
| Elite | Achievement: 1000 Score |
| Legend | Achievement: 100-Day Streak |
| Grandmaster | Complete all achievements |

#### Equipping Cosmetics
```http
POST /api/gamification/cosmetics/equip
{
  "cosmeticId": 5
}
```

---

### Personal Goals

Dynamic goals generated based on player history.

#### Goal Types
| Type | Description |
|------|-------------|
| BeatAverage | Score higher than your average |
| ImproveAccuracy | Place more accurately than usual |
| NewPersonalBest | Set a new high score |

#### Goal Display
Goals appear as toast notifications at game start with:
- Target description
- Current progress percentage
- Motivational message

---

## API Endpoints

### Get Gamification Status
```http
GET /api/gamification
```
Returns complete gamification state including streaks, challenges, achievements, season pass progress, and cosmetics.

### Challenges
```http
GET /api/gamification/challenges
```
Returns active daily and weekly challenges with progress.

### Achievements
```http
GET /api/gamification/achievements
```
Returns all achievements with unlock status.

### Season Pass
```http
GET /api/gamification/season-pass
```
Returns current season info, tier progress, and available rewards.

### Cosmetics
```http
GET /api/gamification/cosmetics
```
Returns owned cosmetics and equipped items.

### Personal Goals
```http
GET /api/gamification/goals
```
Returns current personal goals.

### Use Streak Freeze
```http
POST /api/gamification/streak-freeze
```
Manually use a streak freeze token.

### Set Timezone
```http
POST /api/gamification/timezone
{
  "timezone": "Europe/London"
}
```

---

## Backend Architecture

### Entities
- `Achievement` - Achievement definitions with conditions
- `UserAchievement` - User's unlocked achievements
- `Challenge` - Challenge definitions
- `UserChallenge` - User's challenge progress
- `Season` - Season definitions with duration
- `SeasonReward` - Rewards for each tier
- `ClaimedSeasonReward` - User's claimed rewards
- `Cosmetic` - Cosmetic item definitions
- `UserCosmetic` - User's owned cosmetics
- `PersonalGoal` - User's current goals

### Services
- `StreakService` - Streak calculation and freeze logic
- `ChallengeService` - Challenge progress tracking
- `AchievementService` - Achievement unlock detection
- `SeasonPassService` - Tier progression and rewards
- `CosmeticService` - Cosmetic management
- `PersonalGoalService` - Goal generation and tracking

### Background Jobs
| Job | Schedule | Purpose |
|-----|----------|---------|
| `DailyChallengeRotationJob` | Midnight UTC | Rotate daily challenges |
| `WeeklyChallengeRotationJob` | Monday midnight | Rotate weekly challenges |
| `SeasonTransitionJob` | Daily | Check for season end/start |
| `StreakExpiryNotificationJob` | Hourly | Notify users of expiring streaks |

---

## Frontend Components

### UI Components
| Component | Description |
|-----------|-------------|
| `StreakDisplay` | Animated flame/ice streak counter |
| `StreakFreezeButton` | Use streak freeze token |
| `ChallengeCard` | Challenge progress with radial indicator |
| `ChallengesList` | Tabbed daily/weekly challenges |
| `AchievementBadge` | Achievement with rarity glow |
| `AchievementsPanel` | Filterable achievement grid |
| `AchievementUnlockModal` | Celebration modal with confetti |
| `SeasonPassTrack` | Horizontal tier progression |
| `SeasonRewardCard` | Claimable reward card |
| `PersonalGoalCard` | Goal progress display |
| `PersonalGoalPrompt` | Game-start goal notification |
| `CosmeticSelector` | Tabbed cosmetic picker |
| `BoardThemePreview` | Theme preview display |
| `GameEndSummary` | Post-game XP and rewards |
| `GamificationDashboard` | Combined overview widget |

### State Management
The gamification system uses a dedicated Zustand store:
```typescript
// src/stores/gamificationStore.ts
interface GamificationStore {
  // State
  streakData: StreakData | null;
  challenges: Challenge[];
  achievements: Achievement[];
  seasonPass: SeasonPass | null;
  cosmetics: Cosmetic[];
  personalGoals: PersonalGoal[];
  
  // Actions
  fetchGamificationStatus(): Promise<void>;
  processGameEndGamification(result: GameEndResult): void;
  useStreakFreeze(): Promise<void>;
  claimSeasonReward(rewardId: number): Promise<void>;
  equipCosmetic(cosmeticId: number): Promise<void>;
}
```

---

## Configuration

### Backend Settings
```json
{
  "GamificationSettings": {
    "SeedOnStartup": true,
    "DailyChallengeCount": 3,
    "WeeklyChallengeCount": 3,
    "SeasonDurationDays": 7,
    "MaxSeasonTiers": 50,
    "MaxStreakFreezeTokens": 3,
    "StreakNotificationCooldownHours": 24
  }
}
```

### Seed Data
On startup (in development), the system seeds:
- 18 achievements across 5 categories
- 23 cosmetics (8 themes, 7 frames, 8 badges)
- 20 challenge templates (10 daily, 10 weekly)
- Initial season with tier rewards

---

## Testing

### Backend Tests
- **Unit Tests**: Service logic, achievement conditions
- **Integration Tests**: API endpoints, database operations

### Frontend Tests
- **Component Tests**: UI rendering, interactions
- **Hook Tests**: State management, API integration

Total: 1,400+ tests covering the gamification system.
