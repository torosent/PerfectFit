import type { UpdateProfileRequest, UpdateProfileResponse } from '@/types';
import { API_BASE_URL } from './index';

/**
 * Update the current user's profile (username and/or avatar)
 * @param request - The profile update request containing username and/or avatar
 * @param token - JWT authentication token
 * @returns The profile update response
 */
export async function updateProfile(
  request: UpdateProfileRequest,
  token: string
): Promise<UpdateProfileResponse> {
  const response = await fetch(`${API_BASE_URL}/api/auth/profile`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response.json();
    return error as UpdateProfileResponse;
  }

  return response.json();
}
