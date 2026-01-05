/**
 * Profile API Client Tests
 * 
 * Tests for the updateProfile function that updates user profile (displayName/avatar)
 */

import { updateProfile } from '@/lib/api/profile-client';
import { API_BASE_URL } from '@/lib/api';
import type { UpdateProfileRequest, UpdateProfileResponse } from '@/types';

// Mock fetch globally
const mockFetch = jest.fn();
global.fetch = mockFetch;

describe('updateProfile', () => {
  beforeEach(() => {
    mockFetch.mockClear();
  });

  describe('sends correct request', () => {
    it('should send PUT request to /api/auth/profile with correct headers', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: true,
        profile: {
          id: 1,
          displayName: 'newuser',
          avatar: '😎',
        },
      };
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse),
      });

      const request: UpdateProfileRequest = {
        displayName: 'newuser',
        avatar: '😎',
      };
      const token = 'test-jwt-token';

      await updateProfile(request, token);

      expect(mockFetch).toHaveBeenCalledTimes(1);
      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/auth/profile`,
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer test-jwt-token',
          },
          body: JSON.stringify(request),
        }
      );
    });

    it('should send request with only displayName when avatar not provided', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: true,
        profile: {
          id: 1,
          displayName: 'newuser',
        },
      };
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse),
      });

      const request: UpdateProfileRequest = {
        displayName: 'newuser',
      };
      const token = 'test-jwt-token';

      await updateProfile(request, token);

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          body: JSON.stringify({ displayName: 'newuser' }),
        })
      );
    });

    it('should send request with only avatar when displayName not provided', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: true,
        profile: {
          id: 1,
          displayName: 'existinguser',
          avatar: '🎮',
        },
      };
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse),
      });

      const request: UpdateProfileRequest = {
        avatar: '🎮',
      };
      const token = 'test-jwt-token';

      await updateProfile(request, token);

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          body: JSON.stringify({ avatar: '🎮' }),
        })
      );
    });
  });

  describe('handles success response', () => {
    it('should return success response with profile data', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: true,
        profile: {
          id: 123,
          displayName: 'testuser',
          avatar: '🚀',
        },
      };
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse),
      });

      const request: UpdateProfileRequest = {
        displayName: 'testuser',
        avatar: '🚀',
      };
      const token = 'test-jwt-token';

      const result = await updateProfile(request, token);

      expect(result.success).toBe(true);
      expect(result.profile).toEqual({
        id: 123,
        displayName: 'testuser',
        avatar: '🚀',
      });
      expect(result.errorMessage).toBeUndefined();
    });

    it('should return profile without avatar when avatar not set', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: true,
        profile: {
          id: 123,
          displayName: 'testuser',
        },
      };
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse),
      });

      const request: UpdateProfileRequest = {
        displayName: 'testuser',
      };
      const token = 'test-jwt-token';

      const result = await updateProfile(request, token);

      expect(result.success).toBe(true);
      expect(result.profile?.avatar).toBeUndefined();
    });
  });

  describe('handles error response', () => {
    it('should return error response when displayName is taken', async () => {
      const mockErrorResponse: UpdateProfileResponse = {
        success: false,
        errorMessage: 'Display name is already taken',
        suggestedDisplayName: 'testuser123',
      };
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 400,
        json: () => Promise.resolve(mockErrorResponse),
      });

      const request: UpdateProfileRequest = {
        displayName: 'testuser',
      };
      const token = 'test-jwt-token';

      const result = await updateProfile(request, token);

      expect(result.success).toBe(false);
      expect(result.errorMessage).toBe('Display name is already taken');
      expect(result.suggestedDisplayName).toBe('testuser123');
    });

    it('should return error response for invalid avatar', async () => {
      const mockErrorResponse: UpdateProfileResponse = {
        success: false,
        errorMessage: 'Invalid avatar emoji',
      };
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 400,
        json: () => Promise.resolve(mockErrorResponse),
      });

      const request: UpdateProfileRequest = {
        avatar: 'invalid',
      };
      const token = 'test-jwt-token';

      const result = await updateProfile(request, token);

      expect(result.success).toBe(false);
      expect(result.errorMessage).toBe('Invalid avatar emoji');
    });

    it('should return error response for unauthorized request', async () => {
      const mockErrorResponse: UpdateProfileResponse = {
        success: false,
        errorMessage: 'Unauthorized',
      };
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: () => Promise.resolve(mockErrorResponse),
      });

      const request: UpdateProfileRequest = {
        displayName: 'testuser',
      };
      const token = 'invalid-token';

      const result = await updateProfile(request, token);

      expect(result.success).toBe(false);
      expect(result.errorMessage).toBe('Unauthorized');
    });
  });
});
