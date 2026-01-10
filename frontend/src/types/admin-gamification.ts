/**
 * Admin Gamification Types
 * Types for managing achievements, challenge templates, and cosmetics in the admin portal
 */

// Re-use enum types from the main gamification types
import type {
  AchievementCategory,
  RewardType,
  ChallengeType,
  CosmeticType,
  CosmeticRarity,
} from './gamification';

// Re-export for convenience
export type {
  AchievementCategory,
  RewardType,
  ChallengeType,
  CosmeticType,
  CosmeticRarity,
} from './gamification';

/**
 * Goal type for challenge progress calculation
 */
export type ChallengeGoalType =
  | 'ScoreTotal'
  | 'ScoreSingleGame'
  | 'GameCount'
  | 'WinStreak'
  | 'Accuracy'
  | 'TimeBased';

/**
 * Admin view of an achievement
 */
export interface AdminAchievement {
  id: number;
  name: string;
  description: string;
  category: AchievementCategory;
  iconUrl: string;
  unlockCondition: string; // JSON string
  rewardType: RewardType;
  rewardValue: number;
  isSecret: boolean;
  displayOrder: number;
  rewardCosmeticCode: string | null;
}

/**
 * Request payload for creating a new achievement
 */
export interface CreateAchievementRequest {
  name: string;
  description: string;
  category: AchievementCategory;
  iconUrl: string;
  unlockCondition: string;
  rewardType: RewardType;
  rewardValue: number;
  isSecret?: boolean;
  displayOrder?: number;
  rewardCosmeticCode?: string | null;
}

/**
 * Request payload for updating an existing achievement
 * All fields are required to prevent accidentally resetting values
 */
export interface UpdateAchievementRequest {
  name: string;
  description: string;
  category: AchievementCategory;
  iconUrl: string;
  unlockCondition: string;
  rewardType: RewardType;
  rewardValue: number;
  isSecret: boolean;
  displayOrder: number;
  rewardCosmeticCode: string | null;
}

/**
 * Admin view of a challenge template
 */
export interface AdminChallengeTemplate {
  id: number;
  name: string;
  description: string;
  type: ChallengeType;
  targetValue: number;
  xpReward: number;
  isActive: boolean;
  goalType: ChallengeGoalType | null;
}

/**
 * Request payload for creating a new challenge template
 */
export interface CreateChallengeTemplateRequest {
  name: string;
  description: string;
  type: ChallengeType;
  targetValue: number;
  xpReward: number;
  goalType?: ChallengeGoalType | null;
}

/**
 * Request payload for updating an existing challenge template
 */
export interface UpdateChallengeTemplateRequest extends CreateChallengeTemplateRequest {
  isActive: boolean;
  goalType?: ChallengeGoalType | null;
}

/**
 * Admin view of a cosmetic item
 */
export interface AdminCosmetic {
  id: number;
  code: string;
  name: string;
  description: string;
  type: CosmeticType;
  assetUrl: string;
  previewUrl: string;
  rarity: CosmeticRarity;
  isDefault: boolean;
}

/**
 * Request payload for creating a new cosmetic
 */
export interface CreateCosmeticRequest {
  code: string;
  name: string;
  description: string;
  type: CosmeticType;
  assetUrl: string;
  previewUrl: string;
  rarity: CosmeticRarity;
  isDefault?: boolean;
}

/**
 * Request payload for updating an existing cosmetic
 * All fields are required to prevent accidentally resetting values
 */
export interface UpdateCosmeticRequest {
  code: string;
  name: string;
  description: string;
  type: CosmeticType;
  assetUrl: string;
  previewUrl: string;
  rarity: CosmeticRarity;
  isDefault: boolean;
}

/**
 * Response when attempting to delete an entity that is referenced by other entities
 */
export interface EntityInUseResponse {
  message: string;
  entityType: string;
  entityId: number;
  usageCount: number;
  usageDetails?: string;
}
