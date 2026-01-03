/**
 * Admin API Client Tests
 *
 * Tests for admin client functions: getUsers, getUser, deleteUser, bulkDeleteGuests, getAuditLogs
 */

import {
  getUsers,
  getUser,
  deleteUser,
  bulkDeleteGuests,
  getAuditLogs,
  AdminApiError,
} from '@/lib/api/admin-client';
import { API_BASE_URL } from '@/lib/api';
import type {
  AdminUser,
  PaginatedResponse,
  AuditLog,
  BulkDeleteResponse,
} from '@/types';

// Mock fetch globally
const mockFetch = jest.fn();
global.fetch = mockFetch;

describe('admin-client', () => {
  const mockToken = 'test-admin-jwt-token';

  beforeEach(() => {
    mockFetch.mockClear();
  });

  describe('getUsers', () => {
    const mockUsersResponse: PaginatedResponse<AdminUser> = {
      items: [
        {
          id: 1,
          email: 'user1@example.com',
          displayName: 'User One',
          username: 'userone',
          avatar: '🎮',
          provider: 'google',
          role: 'User',
          createdAt: '2025-01-01T00:00:00Z',
          lastLoginAt: '2025-01-02T00:00:00Z',
          highScore: 1000,
          gamesPlayed: 10,
          isDeleted: false,
          deletedAt: null,
        },
        {
          id: 2,
          email: 'admin@example.com',
          displayName: 'Admin User',
          username: 'admin',
          avatar: '👑',
          provider: 'microsoft',
          role: 'Admin',
          createdAt: '2024-12-01T00:00:00Z',
          lastLoginAt: '2025-01-02T10:00:00Z',
          highScore: 5000,
          gamesPlayed: 100,
          isDeleted: false,
          deletedAt: null,
        },
      ],
      page: 1,
      pageSize: 20,
      totalCount: 2,
      totalPages: 1,
    };

    it('should fetch users with default pagination', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockUsersResponse),
      });

      const result = await getUsers(mockToken);

      expect(mockFetch).toHaveBeenCalledTimes(1);
      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/admin/users?page=1&pageSize=20`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${mockToken}`,
          },
        }
      );
      expect(result).toEqual(mockUsersResponse);
    });

    it('should fetch users with custom pagination', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockUsersResponse),
      });

      await getUsers(mockToken, 2, 50);

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/admin/users?page=2&pageSize=50`,
        expect.any(Object)
      );
    });

    it('should return paginated response with correct structure', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockUsersResponse),
      });

      const result = await getUsers(mockToken);

      expect(result.items).toHaveLength(2);
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(20);
      expect(result.totalCount).toBe(2);
      expect(result.totalPages).toBe(1);
      expect(result.items[0].email).toBe('user1@example.com');
    });

    it('should throw AdminApiError on 401 unauthorized', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        statusText: 'Unauthorized',
        json: () => Promise.resolve({ message: 'Unauthorized' }),
      });

      await expect(getUsers(mockToken)).rejects.toThrow(AdminApiError);
    });

    it('should include error message from response on 401', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        statusText: 'Unauthorized',
        json: () => Promise.resolve({ message: 'Unauthorized' }),
      });

      await expect(getUsers(mockToken)).rejects.toThrow('Unauthorized');
    });

    it('should throw AdminApiError on 403 forbidden', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 403,
        statusText: 'Forbidden',
        json: () => Promise.resolve({ message: 'Admin access required' }),
      });

      await expect(getUsers(mockToken)).rejects.toThrow(AdminApiError);
    });
  });

  describe('getUser', () => {
    const mockUser: AdminUser = {
      id: 1,
      email: 'user@example.com',
      displayName: 'Test User',
      username: 'testuser',
      avatar: '🚀',
      provider: 'google',
      role: 'User',
      createdAt: '2025-01-01T00:00:00Z',
      lastLoginAt: '2025-01-02T00:00:00Z',
      highScore: 2500,
      gamesPlayed: 25,
      isDeleted: false,
      deletedAt: null,
    };

    it('should fetch a single user by id', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockUser),
      });

      const result = await getUser(1, mockToken);

      expect(mockFetch).toHaveBeenCalledTimes(1);
      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/admin/users/1`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${mockToken}`,
          },
        }
      );
      expect(result).toEqual(mockUser);
    });

    it('should throw AdminApiError on 404 not found', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 404,
        statusText: 'Not Found',
        json: () => Promise.resolve({ message: 'User not found' }),
      });

      await expect(getUser(999, mockToken)).rejects.toThrow(AdminApiError);
    });

    it('should include error message from response on 404', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 404,
        statusText: 'Not Found',
        json: () => Promise.resolve({ message: 'User not found' }),
      });

      await expect(getUser(999, mockToken)).rejects.toThrow('User not found');
    });
  });

  describe('deleteUser', () => {
    it('should send DELETE request for a user', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
        json: () => Promise.resolve({}),
      });

      await deleteUser(1, mockToken);

      expect(mockFetch).toHaveBeenCalledTimes(1);
      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/admin/users/1`,
        {
          method: 'DELETE',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${mockToken}`,
          },
        }
      );
    });

    it('should not throw on successful deletion', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
        json: () => Promise.resolve({}),
      });

      await expect(deleteUser(1, mockToken)).resolves.toBeUndefined();
    });

    it('should throw AdminApiError on failure', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 400,
        statusText: 'Bad Request',
        json: () => Promise.resolve({ message: 'Cannot delete admin users' }),
      });

      await expect(deleteUser(1, mockToken)).rejects.toThrow(AdminApiError);
    });
  });

  describe('bulkDeleteGuests', () => {
    const mockBulkResponse: BulkDeleteResponse = {
      deletedCount: 150,
      message: 'Successfully deleted 150 guest accounts',
    };

    it('should send DELETE request to bulk delete guests endpoint', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockBulkResponse),
      });

      const result = await bulkDeleteGuests(mockToken);

      expect(mockFetch).toHaveBeenCalledTimes(1);
      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/admin/users/guests`,
        {
          method: 'DELETE',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${mockToken}`,
          },
        }
      );
      expect(result).toEqual(mockBulkResponse);
    });

    it('should return bulk delete response with count and message', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockBulkResponse),
      });

      const result = await bulkDeleteGuests(mockToken);

      expect(result.deletedCount).toBe(150);
      expect(result.message).toBe('Successfully deleted 150 guest accounts');
    });

    it('should throw AdminApiError on failure', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        statusText: 'Internal Server Error',
        json: () => Promise.resolve({ message: 'Database error' }),
      });

      await expect(bulkDeleteGuests(mockToken)).rejects.toThrow(AdminApiError);
    });
  });

  describe('getAuditLogs', () => {
    const mockAuditLogsResponse: PaginatedResponse<AuditLog> = {
      items: [
        {
          id: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
          adminUserId: 1,
          adminEmail: 'admin@example.com',
          action: 'DeleteUser',
          targetUserId: 5,
          targetUserEmail: 'deleted@example.com',
          details: 'Deleted user account',
          timestamp: '2025-01-02T10:00:00Z',
        },
        {
          id: 'b2c3d4e5-f6a7-8901-bcde-f12345678901',
          adminUserId: 1,
          adminEmail: 'admin@example.com',
          action: 'BulkDeleteGuests',
          targetUserId: null,
          targetUserEmail: null,
          details: 'Deleted 150 guest accounts',
          timestamp: '2025-01-02T09:00:00Z',
        },
      ],
      page: 1,
      pageSize: 20,
      totalCount: 2,
      totalPages: 1,
    };

    it('should fetch audit logs with default pagination', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockAuditLogsResponse),
      });

      const result = await getAuditLogs(mockToken);

      expect(mockFetch).toHaveBeenCalledTimes(1);
      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/admin/audit-logs?page=1&pageSize=20`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${mockToken}`,
          },
        }
      );
      expect(result).toEqual(mockAuditLogsResponse);
    });

    it('should fetch audit logs with custom pagination', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockAuditLogsResponse),
      });

      await getAuditLogs(mockToken, 3, 100);

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/admin/audit-logs?page=3&pageSize=100`,
        expect.any(Object)
      );
    });

    it('should return paginated audit logs with correct structure', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockAuditLogsResponse),
      });

      const result = await getAuditLogs(mockToken);

      expect(result.items).toHaveLength(2);
      expect(result.items[0].action).toBe('DeleteUser');
      expect(result.items[0].adminEmail).toBe('admin@example.com');
      expect(result.items[1].action).toBe('BulkDeleteGuests');
    });

    it('should throw AdminApiError on failure', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 403,
        statusText: 'Forbidden',
        json: () => Promise.resolve({ message: 'Access denied' }),
      });

      await expect(getAuditLogs(mockToken)).rejects.toThrow(AdminApiError);
    });
  });

  describe('AdminApiError', () => {
    it('should include status code in error', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 403,
        statusText: 'Forbidden',
        json: () => Promise.resolve({ message: 'Admin access required' }),
      });

      try {
        await getUsers(mockToken);
      } catch (error) {
        expect(error).toBeInstanceOf(AdminApiError);
        expect((error as AdminApiError).statusCode).toBe(403);
        expect((error as AdminApiError).message).toBe('Admin access required');
      }
    });
  });
});
