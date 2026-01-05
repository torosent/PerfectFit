/**
 * AdminGuard Component Tests
 *
 * Tests for the admin route protection component that checks
 * if user is authenticated and has admin role.
 */

import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import { AdminGuard } from '@/components/admin/AdminGuard';

// Mock next/navigation
const mockPush = jest.fn();
jest.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}));

// Mock auth store with configurable state
let mockAuthState = {
  isAuthenticated: false,
  isInitialized: true,
  user: null as { role?: string } | null,
  initializeAuth: jest.fn(),
};

jest.mock('@/lib/stores/auth-store', () => ({
  useAuthStore: (selector: (state: typeof mockAuthState) => unknown) => {
    return selector(mockAuthState);
  },
  useIsAuthenticated: () => mockAuthState.isAuthenticated,
  useIsAdmin: () => mockAuthState.user?.role === 'Admin',
  useIsAuthInitialized: () => mockAuthState.isInitialized,
  useUser: () => mockAuthState.user,
}));

describe('AdminGuard', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Reset to default state
    mockAuthState = {
      isAuthenticated: false,
      isInitialized: true,
      user: null,
      initializeAuth: jest.fn(),
    };
  });

  describe('redirects non-admins', () => {
    it('should redirect to home when user is not authenticated', async () => {
      mockAuthState = {
        isAuthenticated: false,
        isInitialized: true,
        user: null,
        initializeAuth: jest.fn(),
      };

      render(
        <AdminGuard>
          <div>Admin Content</div>
        </AdminGuard>
      );

      await waitFor(() => {
        expect(mockPush).toHaveBeenCalledWith('/');
      });
      expect(screen.queryByText('Admin Content')).not.toBeInTheDocument();
    });

    it('should redirect to home when user is authenticated but not admin', async () => {
      mockAuthState = {
        isAuthenticated: true,
        isInitialized: true,
        user: { role: 'User' },
        initializeAuth: jest.fn(),
      };

      render(
        <AdminGuard>
          <div>Admin Content</div>
        </AdminGuard>
      );

      await waitFor(() => {
        expect(mockPush).toHaveBeenCalledWith('/');
      });
      expect(screen.queryByText('Admin Content')).not.toBeInTheDocument();
    });
  });

  describe('renders children for admins', () => {
    it('should render children when user is authenticated and has admin role', () => {
      mockAuthState = {
        isAuthenticated: true,
        isInitialized: true,
        user: { role: 'Admin' },
        initializeAuth: jest.fn(),
      };

      render(
        <AdminGuard>
          <div>Admin Content</div>
        </AdminGuard>
      );

      expect(screen.getByText('Admin Content')).toBeInTheDocument();
      expect(mockPush).not.toHaveBeenCalled();
    });
  });

  describe('loading state', () => {
    it('should show loading state while auth is initializing', () => {
      mockAuthState = {
        isAuthenticated: false,
        isInitialized: false,
        user: null,
        initializeAuth: jest.fn(),
      };

      render(
        <AdminGuard>
          <div>Admin Content</div>
        </AdminGuard>
      );

      expect(screen.getByText(/loading/i)).toBeInTheDocument();
      expect(screen.queryByText('Admin Content')).not.toBeInTheDocument();
      expect(mockPush).not.toHaveBeenCalled();
    });
  });
});
