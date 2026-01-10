/**
 * Gamification API Client
 *
 * API client functions for the gamification system including
 * challenges, achievements, season pass, cosmetics, and personal goals.
 */

import { API_BASE_URL } from './constants';
import type {
  GamificationStatus,
  Challenge,
  ChallengeType,
  AchievementsResult,
  SeasonPassResult,
  CosmeticsResult,
  CosmeticType,
  PersonalGoal,
  ApiResult,
  ClaimRewardResult,
  EquipCosmeticRequest,
  ClaimRewardRequest,
  SetTimezoneRequest,
} from '@/types/gamification';

/**
 * Custom error class for gamification API errors
 */
export class GamificationApiError extends Error {
  constructor(
    message: string,
    public statusCode: number,
    public details?: unknown
  ) {
    super(message);
    this.name = 'GamificationApiError';
  }
}

/**
 * Generic fetch wrapper with authentication and error handling
 * @param endpoint - API endpoint path
 * @param token - JWT authentication token
 * @param options - Additional fetch options
 * @returns Parsed JSON response
 */
async function apiFetch<T>(
  endpoint: string,
  token: string,
  options: RequestInit = {}
): Promise<T> {
  const url = `${API_BASE_URL}${endpoint}`;

  const defaultHeaders: HeadersInit = {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token}`,
  };

  const response = await fetch(url, {
    ...options,
    headers: {
      ...defaultHeaders,
      ...options.headers,
    },
  });

  if (!response.ok) {
    let errorMessage = `API error: ${response.status}`;
    let details: unknown;

    try {
      const errorBody = await response.json();
      // Backend uses 'errorMessage' field, fallback to 'message' and 'error' for compatibility
      errorMessage = errorBody.errorMessage || errorBody.message || errorBody.error || errorMessage;
      details = errorBody;
    } catch {
      // Response body wasn't JSON, use default message
    }

    throw new GamificationApiError(errorMessage, response.status, details);
  }

  return response.json();
}

// ============================================================================
// GET Endpoints
// ============================================================================

/**
 * Get the full gamification status for the current user
 * @param token - JWT authentication token
 * @returns Complete gamification status
 */
export async function getGamificationStatus(token: string): Promise<GamificationStatus> {
  return apiFetch<GamificationStatus>('/api/gamification', token);
}

/**
 * Get challenges for the current user
 * @param token - JWT authentication token
 * @param type - Optional filter by challenge type (Daily or Weekly)
 * @returns List of challenges
 */
export async function getChallenges(token: string, type?: ChallengeType): Promise<Challenge[]> {
  const params = type ? `?type=${type}` : '';
  return apiFetch<Challenge[]>(`/api/gamification/challenges${params}`, token);
}

/**
 * Get achievements for the current user
 * @param token - JWT authentication token
 * @returns Achievements result with progress
 */
export async function getAchievements(token: string): Promise<AchievementsResult> {
  return apiFetch<AchievementsResult>('/api/gamification/achievements', token);
}

/**
 * Get the current season pass status
 * @param token - JWT authentication token
 * @returns Season pass result with information and active status
 */
export async function getSeasonPass(token: string): Promise<SeasonPassResult> {
  return apiFetch<SeasonPassResult>('/api/gamification/season-pass', token);
}

/**
 * Get cosmetics for the current user
 * @param token - JWT authentication token
 * @param type - Optional filter by cosmetic type
 * @returns Cosmetics result with equipped items
 */
export async function getCosmetics(token: string, type?: CosmeticType): Promise<CosmeticsResult> {
  const params = type ? `?type=${type}` : '';
  return apiFetch<CosmeticsResult>(`/api/gamification/cosmetics${params}`, token);
}

/**
 * Get personal goals for the current user
 * @param token - JWT authentication token
 * @returns List of personal goals
 */
export async function getPersonalGoals(token: string): Promise<PersonalGoal[]> {
  return apiFetch<PersonalGoal[]>('/api/gamification/goals', token);
}

// ============================================================================
// POST Endpoints
// ============================================================================

/**
 * Use a streak freeze token to protect the current streak
 * @param token - JWT authentication token
 * @returns Result of the operation
 */
export async function useStreakFreeze(token: string): Promise<ApiResult> {
  return apiFetch<ApiResult>('/api/gamification/streak-freeze', token, {
    method: 'POST',
  });
}

/**
 * Equip a cosmetic item
 * @param token - JWT authentication token
 * @param cosmeticId - ID of the cosmetic to equip
 * @returns Result of the operation
 */
export async function equipCosmetic(token: string, cosmeticId: string): Promise<ApiResult> {
  const body: EquipCosmeticRequest = { cosmeticId };
  return apiFetch<ApiResult>('/api/gamification/equip', token, {
    method: 'POST',
    body: JSON.stringify(body),
  });
}

/**
 * Claim a season pass reward
 * @param token - JWT authentication token
 * @param rewardId - ID of the reward to claim
 * @returns Result of the claim with reward details
 */
export async function claimSeasonReward(token: string, rewardId: string): Promise<ClaimRewardResult> {
  const body: ClaimRewardRequest = { rewardId };
  return apiFetch<ClaimRewardResult>('/api/gamification/claim-reward', token, {
    method: 'POST',
    body: JSON.stringify(body),
  });
}

/**
 * Set the user's timezone for challenge/streak calculations
 * @param token - JWT authentication token
 * @param timezone - IANA timezone string (e.g., "America/New_York")
 * @returns Result of the operation
 */
export async function setTimezone(token: string, timezone: string): Promise<ApiResult> {
  const body: SetTimezoneRequest = { timezone };
  return apiFetch<ApiResult>('/api/gamification/timezone', token, {
    method: 'POST',
    body: JSON.stringify(body),
  });
}

// ============================================================================
// Utility Functions
// ============================================================================

/**
 * Get the user's current timezone
 * @returns IANA timezone string
 */
export function getUserTimezone(): string {
  return Intl.DateTimeFormat().resolvedOptions().timeZone;
}

/**
 * Initialize the user's timezone on the server
 * @param token - JWT authentication token
 * @returns Promise that resolves when timezone is set
 */
export async function initializeTimezone(token: string): Promise<void> {
  const timezone = getUserTimezone();
  try {
    await setTimezone(token, timezone);
  } catch (error) {
    console.warn('Failed to set timezone:', error);
  }
}
