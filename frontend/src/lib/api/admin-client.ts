import type {
  AdminUser,
  PaginatedResponse,
  AuditLog,
  BulkDeleteResponse,
} from '@/types';
import { API_BASE_URL } from './constants';

/**
 * Custom error class for Admin API errors
 */
export class AdminApiError extends Error {
  constructor(
    message: string,
    public statusCode: number,
    public details?: unknown
  ) {
    super(message);
    this.name = 'AdminApiError';
  }
}

/**
 * Create standard headers for admin API requests
 */
function getAdminHeaders(token: string): HeadersInit {
  return {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token}`,
  };
}

/**
 * Handle admin API response and throw AdminApiError on failure
 */
async function handleAdminResponse<T>(response: Response): Promise<T> {
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

/**
 * Get paginated list of all users (admin only)
 * @param token - JWT authentication token
 * @param page - Page number (1-indexed)
 * @param pageSize - Number of items per page
 * @returns Paginated list of users
 */
export async function getUsers(
  token: string,
  page: number = 1,
  pageSize: number = 20
): Promise<PaginatedResponse<AdminUser>> {
  const response = await fetch(
    `${API_BASE_URL}/api/admin/users?page=${page}&pageSize=${pageSize}`,
    {
      method: 'GET',
      headers: getAdminHeaders(token),
    }
  );

  return handleAdminResponse<PaginatedResponse<AdminUser>>(response);
}

/**
 * Get a single user by ID (admin only)
 * @param id - User ID
 * @param token - JWT authentication token
 * @returns User details
 */
export async function getUser(id: number, token: string): Promise<AdminUser> {
  const response = await fetch(`${API_BASE_URL}/api/admin/users/${id}`, {
    method: 'GET',
    headers: getAdminHeaders(token),
  });

  return handleAdminResponse<AdminUser>(response);
}

/**
 * Delete a user by ID (soft delete, admin only)
 * @param id - User ID to delete
 * @param token - JWT authentication token
 */
export async function deleteUser(id: number, token: string): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/admin/users/${id}`, {
    method: 'DELETE',
    headers: getAdminHeaders(token),
  });

  await handleAdminResponse<void>(response);
}

/**
 * Bulk delete all guest accounts (admin only)
 * @param token - JWT authentication token
 * @returns Response with count of deleted guests
 */
export async function bulkDeleteGuests(
  token: string
): Promise<BulkDeleteResponse> {
  const response = await fetch(`${API_BASE_URL}/api/admin/users/bulk/guests`, {
    method: 'DELETE',
    headers: getAdminHeaders(token),
  });

  return handleAdminResponse<BulkDeleteResponse>(response);
}

/**
 * Get paginated audit logs (admin only)
 * @param token - JWT authentication token
 * @param page - Page number (1-indexed)
 * @param pageSize - Number of items per page
 * @returns Paginated list of audit logs
 */
export async function getAuditLogs(
  token: string,
  page: number = 1,
  pageSize: number = 20
): Promise<PaginatedResponse<AuditLog>> {
  const response = await fetch(
    `${API_BASE_URL}/api/admin/audit-logs?page=${page}&pageSize=${pageSize}`,
    {
      method: 'GET',
      headers: getAdminHeaders(token),
    }
  );

  return handleAdminResponse<PaginatedResponse<AuditLog>>(response);
}
