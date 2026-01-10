/**
 * Gamification Types
 *
 * TypeScript type definitions for the gamification system including
 * challenges, achievements, season pass, cosmetics, and personal goals.
 * 
 * These types match the backend DTOs from PerfectFit.Web.DTOs.GamificationDtos
 */

// ============================================================================
// Enums / Union Types
// ============================================================================

/** Type of challenge - daily or weekly */
export type ChallengeType = 'Daily' | 'Weekly';

/** Achievement category */
export type AchievementCategory = 'Score' | 'Streak' | 'Games' | 'Challenge' | 'Special';

/** Cosmetic item type */
export type CosmeticType = 'BoardTheme' | 'AvatarFrame' | 'Badge';

/** Cosmetic rarity */
export type CosmeticRarity = 'Common' | 'Rare' | 'Epic' | 'Legendary';

/** Personal goal type */
export type GoalType = 'BeatAverage' | 'ImproveAccuracy' | 'NewPersonalBest';

/** Reward type for season pass rewards */
export type RewardType = 'Cosmetic' | 'StreakFreeze' | 'XPBoost';

// ============================================================================
// Streak
// ============================================================================

/** User's streak information (matches StreakDto) */
export interface StreakInfo {
  /** Current consecutive days played */
  currentStreak: number;
  /** Longest streak ever achieved */
  longestStreak: number;
  /** Number of streak freeze tokens available */
  freezeTokens: number;
  /** Whether the streak is at risk of being lost */
  isAtRisk: boolean;
  /** When the streak will reset if no game is played (ISO date string) */
  resetTime: string;
}

// ============================================================================
// Challenges
// ============================================================================

/** A challenge that can be completed for rewards (matches ChallengeDto) */
export interface Challenge {
  /** Unique identifier */
  id: number;
  /** Challenge name */
  name: string;
  /** Challenge description */
  description: string;
  /** Whether this is a daily or weekly challenge */
  type: ChallengeType;
  /** Target value to complete the challenge */
  targetValue: number;
  /** Current progress toward the target */
  currentProgress: number;
  /** XP reward for completing the challenge */
  xpReward: number;
  /** Whether the challenge has been completed */
  isCompleted: boolean;
  /** When the challenge expires (ISO date string) */
  endsAt: string;
}

// ============================================================================
// Achievements
// ============================================================================

/** An achievement that can be unlocked (matches AchievementDto) */
export interface Achievement {
  /** Unique identifier */
  id: number;
  /** Achievement name */
  name: string;
  /** Achievement description */
  description: string;
  /** Category of the achievement */
  category: AchievementCategory;
  /** URL to the achievement icon */
  iconUrl: string;
  /** Whether the achievement has been unlocked */
  isUnlocked: boolean;
  /** Progress toward unlocking (0-100) */
  progress: number;
  /** When the achievement was unlocked (ISO date string) */
  unlockedAt: string | null;
  /** Whether this is a secret/hidden achievement */
  isSecret: boolean;
}

/** Result from achievements endpoint (matches AchievementsDto) */
export interface AchievementsResult {
  /** List of all achievements */
  achievements: Achievement[];
  /** Number of achievements unlocked by the user */
  totalUnlocked: number;
  /** Total number of achievements available */
  totalAchievements: number;
}

// ============================================================================
// Season Pass
// ============================================================================

/** A reward in the season pass (matches SeasonRewardDto) */
export interface SeasonReward {
  /** Unique identifier */
  id: number;
  /** Tier level required for this reward */
  tier: number;
  /** Type of reward */
  rewardType: RewardType;
  /** Value of the reward (cosmetic ID or token count) */
  rewardValue: number;
  /** XP required to unlock this tier */
  xpRequired: number;
  /** Whether the reward has been claimed */
  isClaimed: boolean;
  /** Whether the reward can be claimed (tier reached and not claimed) */
  canClaim: boolean;
}

/** Season pass information (matches SeasonPassInfoDto) */
export interface SeasonPassInfo {
  /** Unique identifier for the season */
  seasonId: number;
  /** Display name of the season */
  seasonName: string;
  /** Season number */
  seasonNumber: number;
  /** User's current XP in this season */
  currentXP: number;
  /** User's current tier in this season */
  currentTier: number;
  /** When the season ends (ISO date string) */
  endsAt: string;
  /** List of rewards in this season */
  rewards: SeasonReward[];
}

/** Season pass result wrapper (matches SeasonPassDto) */
export interface SeasonPassResult {
  /** Season pass information (null if no active season) */
  seasonPass: SeasonPassInfo | null;
  /** Whether there is an active season */
  hasActiveSeason: boolean;
}

// ============================================================================
// Cosmetics
// ============================================================================

/** A cosmetic item (matches CosmeticDto) */
export interface Cosmetic {
  /** Unique identifier */
  id: number;
  /** Display name */
  name: string;
  /** Description */
  description: string;
  /** Type of cosmetic */
  type: CosmeticType;
  /** Rarity of the cosmetic */
  rarity: CosmeticRarity;
  /** URL to the full asset */
  assetUrl: string;
  /** URL to the preview image */
  previewUrl: string;
  /** Whether the user owns this cosmetic */
  isOwned: boolean;
  /** Whether the cosmetic is currently equipped */
  isEquipped: boolean;
  /** Whether this is the default cosmetic */
  isDefault: boolean;
}

/** Result from cosmetics endpoint (matches CosmeticsDto) */
export interface CosmeticsResult {
  /** List of cosmetics */
  cosmetics: Cosmetic[];
}

/** Basic cosmetic info for equipped items (matches CosmeticInfoDto) */
export interface CosmeticInfo {
  /** Unique identifier */
  id: number;
  /** Display name */
  name: string;
  /** Type of cosmetic */
  type: CosmeticType;
  /** URL to the asset */
  assetUrl: string;
  /** Rarity of the cosmetic */
  rarity: CosmeticRarity;
}

/** Currently equipped cosmetics for each slot (matches EquippedCosmeticsDto) */
export interface EquippedCosmetics {
  /** Equipped board theme */
  boardTheme: CosmeticInfo | null;
  /** Equipped avatar frame */
  avatarFrame: CosmeticInfo | null;
  /** Equipped badge */
  badge: CosmeticInfo | null;
}

// ============================================================================
// Personal Goals
// ============================================================================

/** A personal goal for the user (matches PersonalGoalDto) */
export interface PersonalGoal {
  /** Unique identifier */
  id: number;
  /** Type of goal */
  type: GoalType;
  /** Description of the goal */
  description: string;
  /** Target value to achieve */
  targetValue: number;
  /** Current progress value */
  currentValue: number;
  /** Progress percentage (0-100) */
  progressPercentage: number;
  /** Whether the goal has been completed */
  isCompleted: boolean;
  /** When the goal expires (ISO date string, null if no expiration) */
  expiresAt: string | null;
}

// ============================================================================
// Full Gamification Status
// ============================================================================

/** Complete gamification status for a user (matches GamificationStatusDto) */
export interface GamificationStatus {
  /** Streak information */
  streak: StreakInfo;
  /** Currently active challenges */
  activeChallenges: Challenge[];
  /** Recently unlocked achievements */
  recentAchievements: Achievement[];
  /** Current season pass status (null if no active season) */
  seasonPass: SeasonPassInfo | null;
  /** Currently equipped cosmetics */
  equippedCosmetics: EquippedCosmetics;
  /** Active personal goals */
  activeGoals: PersonalGoal[];
}

// ============================================================================
// Game End Gamification Results
// ============================================================================

/** Gamification data returned at the end of a game (matches GameEndGamificationDto) */
export interface GameEndGamification {
  /** Updated streak info */
  streak: StreakInfo;
  /** Challenge progress updates */
  challengeUpdates: ChallengeProgress[];
  /** Newly unlocked achievements */
  newAchievements: AchievementUnlock[];
  /** Season pass XP progress */
  seasonProgress: SeasonXPGain;
  /** Personal goal progress updates */
  goalUpdates: GoalProgress[];
}

/** Progress update for a challenge (matches ChallengeProgressDto) */
export interface ChallengeProgress {
  /** Challenge ID */
  challengeId: number;
  /** Challenge name */
  challengeName: string;
  /** New progress value */
  newProgress: number;
  /** Whether the challenge was just completed */
  justCompleted: boolean;
  /** XP earned if just completed */
  xpEarned: number | null;
}

/** Notification for a newly unlocked achievement (matches AchievementUnlockDto) */
export interface AchievementUnlock {
  /** Achievement ID */
  achievementId: number;
  /** Achievement name */
  name: string;
  /** Achievement description */
  description: string;
  /** URL to the achievement icon */
  iconUrl: string;
  /** Type of reward granted */
  rewardType: RewardType;
  /** Value of the reward */
  rewardValue: number;
}

/** XP gain in the season pass (matches SeasonXPDto) */
export interface SeasonXPGain {
  /** XP earned from this game */
  xpEarned: number;
  /** Total XP in the season after this game */
  totalXP: number;
  /** New tier level */
  newTier: number;
  /** Whether the user leveled up */
  tierUp: boolean;
  /** IDs of new rewards available to claim */
  newRewardsAvailable: number[];
}

/** Progress update for a personal goal (matches GoalProgressDto) */
export interface GoalProgress {
  /** Goal ID */
  goalId: number;
  /** Goal description */
  description: string;
  /** New progress value */
  newProgress: number;
  /** Whether the goal was just completed */
  justCompleted: boolean;
}

// ============================================================================
// API Request Types
// ============================================================================

/** Request to equip a cosmetic */
export interface EquipCosmeticRequest {
  /** ID of the cosmetic to equip */
  cosmeticId: string;
}

/** Request to claim a season reward */
export interface ClaimRewardRequest {
  /** ID of the reward to claim */
  rewardId: string;
}

/** Request to set user's timezone */
export interface SetTimezoneRequest {
  /** IANA timezone string (e.g., "America/New_York") */
  timezone: string;
}

// ============================================================================
// API Response Types
// ============================================================================

/** Generic API result (matches backend response DTOs) */
export interface ApiResult {
  /** Whether the operation succeeded */
  success: boolean;
  /** Optional error message */
  errorMessage?: string;
}

/** Result from claiming a season reward (matches ClaimRewardResponseDto) */
export interface ClaimRewardResult extends ApiResult {
  /** Type of reward claimed */
  rewardType?: RewardType;
  /** Value of the reward claimed (cosmetic ID or token count) */
  rewardValue?: number;
}
