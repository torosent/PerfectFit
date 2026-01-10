/**
 * Admin Gamification API Client
 * CRUD operations for achievements, challenge templates, and cosmetics
 */
import type {
  AdminAchievement,
  CreateAchievementRequest,
  UpdateAchievementRequest,
  AdminChallengeTemplate,
  CreateChallengeTemplateRequest,
  UpdateChallengeTemplateRequest,
  AdminCosmetic,
  CreateCosmeticRequest,
  UpdateCosmeticRequest,
  PaginatedResponse,
} from '@/types';
import { API_BASE_URL } from './constants';
import { AdminApiError } from './admin-client';

const GAMIFICATION_BASE = `${API_BASE_URL}/api/admin/gamification`;

/**
 * Create standard headers for admin gamification API requests
 */
function getAdminGamificationHeaders(token: string): HeadersInit {
  return {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token}`,
  };
}

/**
 * Handle admin gamification API response and throw AdminApiError on failure
 */
async function handleAdminGamificationResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let errorMessage = `API error: ${response.status} ${response.statusText}`;
    let details: unknown;

    try {
      const errorBody = await response.json();
      errorMessage = errorBody.message || errorBody.error || errorMessage;
      details = errorBody;
    } catch {
      // Response body wasn't JSON, use default message
    }

    throw new AdminApiError(errorMessage, response.status, details);
  }

  // Handle 204 No Content
  if (response.status === 204) {
    return {} as T;
  }

  return response.json();
}

// ============================================================================
// ACHIEVEMENTS
// ============================================================================

/**
 * Get paginated list of all achievements (admin only)
 * @param token - JWT authentication token
 * @param page - Page number (1-indexed)
 * @param pageSize - Number of items per page
 * @returns Paginated list of achievements
 */
export async function getAdminAchievements(
  token: string,
  page: number = 1,
  pageSize: number = 20
): Promise<PaginatedResponse<AdminAchievement>> {
  const response = await fetch(
    `${GAMIFICATION_BASE}/achievements?page=${page}&pageSize=${pageSize}`,
    {
      method: 'GET',
      headers: getAdminGamificationHeaders(token),
    }
  );

  return handleAdminGamificationResponse<PaginatedResponse<AdminAchievement>>(response);
}

/**
 * Get a single achievement by ID (admin only)
 * @param id - Achievement ID
 * @param token - JWT authentication token
 * @returns Achievement details
 */
export async function getAdminAchievement(
  id: number,
  token: string
): Promise<AdminAchievement> {
  const response = await fetch(`${GAMIFICATION_BASE}/achievements/${id}`, {
    method: 'GET',
    headers: getAdminGamificationHeaders(token),
  });

  return handleAdminGamificationResponse<AdminAchievement>(response);
}

/**
 * Create a new achievement (admin only)
 * @param request - Achievement creation data
 * @param token - JWT authentication token
 * @returns Created achievement
 */
export async function createAdminAchievement(
  request: CreateAchievementRequest,
  token: string
): Promise<AdminAchievement> {
  const response = await fetch(`${GAMIFICATION_BASE}/achievements`, {
    method: 'POST',
    headers: getAdminGamificationHeaders(token),
    body: JSON.stringify(request),
  });

  return handleAdminGamificationResponse<AdminAchievement>(response);
}

/**
 * Update an existing achievement (admin only)
 * @param id - Achievement ID
 * @param request - Achievement update data
 * @param token - JWT authentication token
 * @returns Updated achievement
 */
export async function updateAdminAchievement(
  id: number,
  request: UpdateAchievementRequest,
  token: string
): Promise<AdminAchievement> {
  const response = await fetch(`${GAMIFICATION_BASE}/achievements/${id}`, {
    method: 'PUT',
    headers: getAdminGamificationHeaders(token),
    body: JSON.stringify(request),
  });

  return handleAdminGamificationResponse<AdminAchievement>(response);
}

/**
 * Delete an achievement (admin only)
 * @param id - Achievement ID to delete
 * @param token - JWT authentication token
 * @throws AdminApiError with status 409 if achievement is referenced by users
 */
export async function deleteAdminAchievement(
  id: number,
  token: string
): Promise<void> {
  const response = await fetch(`${GAMIFICATION_BASE}/achievements/${id}`, {
    method: 'DELETE',
    headers: getAdminGamificationHeaders(token),
  });

  await handleAdminGamificationResponse<void>(response);
}

// ============================================================================
// CHALLENGE TEMPLATES
// ============================================================================

/**
 * Get paginated list of all challenge templates (admin only)
 * @param token - JWT authentication token
 * @param page - Page number (1-indexed)
 * @param pageSize - Number of items per page
 * @returns Paginated list of challenge templates
 */
export async function getAdminChallengeTemplates(
  token: string,
  page: number = 1,
  pageSize: number = 20
): Promise<PaginatedResponse<AdminChallengeTemplate>> {
  const response = await fetch(
    `${GAMIFICATION_BASE}/challenge-templates?page=${page}&pageSize=${pageSize}`,
    {
      method: 'GET',
      headers: getAdminGamificationHeaders(token),
    }
  );

  return handleAdminGamificationResponse<PaginatedResponse<AdminChallengeTemplate>>(response);
}

/**
 * Get a single challenge template by ID (admin only)
 * @param id - Challenge template ID
 * @param token - JWT authentication token
 * @returns Challenge template details
 */
export async function getAdminChallengeTemplate(
  id: number,
  token: string
): Promise<AdminChallengeTemplate> {
  const response = await fetch(`${GAMIFICATION_BASE}/challenge-templates/${id}`, {
    method: 'GET',
    headers: getAdminGamificationHeaders(token),
  });

  return handleAdminGamificationResponse<AdminChallengeTemplate>(response);
}

/**
 * Create a new challenge template (admin only)
 * @param request - Challenge template creation data
 * @param token - JWT authentication token
 * @returns Created challenge template
 */
export async function createAdminChallengeTemplate(
  request: CreateChallengeTemplateRequest,
  token: string
): Promise<AdminChallengeTemplate> {
  const response = await fetch(`${GAMIFICATION_BASE}/challenge-templates`, {
    method: 'POST',
    headers: getAdminGamificationHeaders(token),
    body: JSON.stringify(request),
  });

  return handleAdminGamificationResponse<AdminChallengeTemplate>(response);
}

/**
 * Update an existing challenge template (admin only)
 * @param id - Challenge template ID
 * @param request - Challenge template update data
 * @param token - JWT authentication token
 * @returns Updated challenge template
 */
export async function updateAdminChallengeTemplate(
  id: number,
  request: UpdateChallengeTemplateRequest,
  token: string
): Promise<AdminChallengeTemplate> {
  const response = await fetch(`${GAMIFICATION_BASE}/challenge-templates/${id}`, {
    method: 'PUT',
    headers: getAdminGamificationHeaders(token),
    body: JSON.stringify(request),
  });

  return handleAdminGamificationResponse<AdminChallengeTemplate>(response);
}

/**
 * Delete a challenge template (admin only)
 * @param id - Challenge template ID to delete
 * @param token - JWT authentication token
 * @throws AdminApiError with status 409 if template has active challenges
 */
export async function deleteAdminChallengeTemplate(
  id: number,
  token: string
): Promise<void> {
  const response = await fetch(`${GAMIFICATION_BASE}/challenge-templates/${id}`, {
    method: 'DELETE',
    headers: getAdminGamificationHeaders(token),
  });

  await handleAdminGamificationResponse<void>(response);
}

// ============================================================================
// COSMETICS
// ============================================================================

/**
 * Get paginated list of all cosmetics (admin only)
 * @param token - JWT authentication token
 * @param page - Page number (1-indexed)
 * @param pageSize - Number of items per page
 * @returns Paginated list of cosmetics
 */
export async function getAdminCosmetics(
  token: string,
  page: number = 1,
  pageSize: number = 20
): Promise<PaginatedResponse<AdminCosmetic>> {
  const response = await fetch(
    `${GAMIFICATION_BASE}/cosmetics?page=${page}&pageSize=${pageSize}`,
    {
      method: 'GET',
      headers: getAdminGamificationHeaders(token),
    }
  );

  return handleAdminGamificationResponse<PaginatedResponse<AdminCosmetic>>(response);
}

/**
 * Get a single cosmetic by ID (admin only)
 * @param id - Cosmetic ID
 * @param token - JWT authentication token
 * @returns Cosmetic details
 */
export async function getAdminCosmetic(
  id: number,
  token: string
): Promise<AdminCosmetic> {
  const response = await fetch(`${GAMIFICATION_BASE}/cosmetics/${id}`, {
    method: 'GET',
    headers: getAdminGamificationHeaders(token),
  });

  return handleAdminGamificationResponse<AdminCosmetic>(response);
}

/**
 * Create a new cosmetic (admin only)
 * @param request - Cosmetic creation data
 * @param token - JWT authentication token
 * @returns Created cosmetic
 */
export async function createAdminCosmetic(
  request: CreateCosmeticRequest,
  token: string
): Promise<AdminCosmetic> {
  const response = await fetch(`${GAMIFICATION_BASE}/cosmetics`, {
    method: 'POST',
    headers: getAdminGamificationHeaders(token),
    body: JSON.stringify(request),
  });

  return handleAdminGamificationResponse<AdminCosmetic>(response);
}

/**
 * Update an existing cosmetic (admin only)
 * @param id - Cosmetic ID
 * @param request - Cosmetic update data
 * @param token - JWT authentication token
 * @returns Updated cosmetic
 */
export async function updateAdminCosmetic(
  id: number,
  request: UpdateCosmeticRequest,
  token: string
): Promise<AdminCosmetic> {
  const response = await fetch(`${GAMIFICATION_BASE}/cosmetics/${id}`, {
    method: 'PUT',
    headers: getAdminGamificationHeaders(token),
    body: JSON.stringify(request),
  });

  return handleAdminGamificationResponse<AdminCosmetic>(response);
}

/**
 * Delete a cosmetic (admin only)
 * @param id - Cosmetic ID to delete
 * @param token - JWT authentication token
 * @throws AdminApiError with status 409 if cosmetic is owned by users or used as achievement reward
 */
export async function deleteAdminCosmetic(
  id: number,
  token: string
): Promise<void> {
  const response = await fetch(`${GAMIFICATION_BASE}/cosmetics/${id}`, {
    method: 'DELETE',
    headers: getAdminGamificationHeaders(token),
  });

  await handleAdminGamificationResponse<void>(response);
}
