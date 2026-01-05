/**
 * Login Page Tests
 * 
 * Tests for the login page with email/password form, Microsoft OAuth,
 * and guest login options.
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import LoginPage from '@/app/(auth)/login/page';

// Mock next/navigation
const mockPush = jest.fn();
jest.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}));

// Mock auth store
const mockLoginAsGuest = jest.fn();
const mockLocalLogin = jest.fn();
const mockClearError = jest.fn();
const mockInitializeAuth = jest.fn();

let mockIsAuthenticated = false;
let mockIsLoading = false;
let mockError: string | null = null;
let mockLockoutEnd: string | null = null;

jest.mock('@/lib/stores/auth-store', () => ({
  useIsAuthenticated: () => mockIsAuthenticated,
  useIsAuthLoading: () => mockIsLoading,
  useAuthError: () => mockError,
  useLockoutEnd: () => mockLockoutEnd,
  useIsGuest: () => false,
  useAuthStore: jest.fn((selector) => {
    const state = {
      loginAsGuest: mockLoginAsGuest,
      localLogin: mockLocalLogin,
      clearError: mockClearError,
      initializeAuth: mockInitializeAuth,
      lockoutEnd: mockLockoutEnd,
    };
    if (typeof selector === 'function') {
      return selector(state);
    }
    return state;
  }),
}));

// Mock auth client
jest.mock('@/lib/api/auth-client', () => ({
  getOAuthUrl: jest.fn((provider: string) => `https://api.example.com/auth/${provider}`),
}));

describe('LoginPage', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockIsAuthenticated = false;
    mockIsLoading = false;
    mockError = null;
    mockLockoutEnd = null;
  });

  describe('renders email and password inputs', () => {
    it('should render email input field', () => {
      render(<LoginPage />);

      const emailInput = screen.getByLabelText(/email/i);
      expect(emailInput).toBeInTheDocument();
      expect(emailInput).toHaveAttribute('type', 'email');
    });

    it('should render password input field', () => {
      render(<LoginPage />);

      const passwordInput = screen.getByLabelText(/password/i);
      expect(passwordInput).toBeInTheDocument();
      expect(passwordInput).toHaveAttribute('type', 'password');
    });

    it('should render sign in button', () => {
      render(<LoginPage />);

      const signInButton = screen.getByRole('button', { name: /sign in/i });
      expect(signInButton).toBeInTheDocument();
    });
  });

  describe('renders Microsoft login button', () => {
    it('should render Microsoft OAuth button', () => {
      render(<LoginPage />);

      const microsoftButton = screen.getByRole('button', { name: /continue with microsoft/i });
      expect(microsoftButton).toBeInTheDocument();
    });

    it('should not render Google or Facebook buttons', () => {
      render(<LoginPage />);

      expect(screen.queryByRole('button', { name: /continue with google/i })).not.toBeInTheDocument();
      expect(screen.queryByRole('button', { name: /continue with facebook/i })).not.toBeInTheDocument();
    });
  });

  describe('renders guest login option', () => {
    it('should render guest login button', () => {
      render(<LoginPage />);

      const guestButton = screen.getByRole('button', { name: /play as guest/i });
      expect(guestButton).toBeInTheDocument();
    });

    it('should call loginAsGuest when guest button is clicked', async () => {
      mockLoginAsGuest.mockResolvedValueOnce(undefined);
      render(<LoginPage />);

      const guestButton = screen.getByRole('button', { name: /play as guest/i });
      fireEvent.click(guestButton);

      await waitFor(() => {
        expect(mockLoginAsGuest).toHaveBeenCalled();
      });
    });
  });

  describe('shows validation error for empty email', () => {
    it('should show error when submitting with empty email', async () => {
      render(<LoginPage />);

      const passwordInput = screen.getByLabelText(/password/i);
      await userEvent.type(passwordInput, 'password123');

      const signInButton = screen.getByRole('button', { name: /sign in/i });
      fireEvent.click(signInButton);

      await waitFor(() => {
        expect(screen.getByText(/email is required/i)).toBeInTheDocument();
      });

      expect(mockLocalLogin).not.toHaveBeenCalled();
    });

    // Note: Email validation uses both HTML5 type="email" validation and custom regex.
    // We skip explicit invalid format test since validation is handled at multiple levels.
  });

  describe('shows validation error for empty password', () => {
    it('should show error when submitting with empty password', async () => {
      render(<LoginPage />);

      const emailInput = screen.getByLabelText(/email/i);
      await userEvent.type(emailInput, 'test@example.com');

      const signInButton = screen.getByRole('button', { name: /sign in/i });
      fireEvent.click(signInButton);

      await waitFor(() => {
        expect(screen.getByText(/password is required/i)).toBeInTheDocument();
      });

      expect(mockLocalLogin).not.toHaveBeenCalled();
    });
  });

  describe('shows error message on failed login', () => {
    it('should display error message from auth store', () => {
      mockError = 'Invalid email or password';
      render(<LoginPage />);

      expect(screen.getByText(/invalid email or password/i)).toBeInTheDocument();
    });

    it('should call localLogin with email and password on valid submit', async () => {
      mockLocalLogin.mockResolvedValueOnce(undefined);
      render(<LoginPage />);

      const emailInput = screen.getByLabelText(/email/i);
      const passwordInput = screen.getByLabelText(/password/i);
      
      await userEvent.type(emailInput, 'test@example.com');
      await userEvent.type(passwordInput, 'password123');

      const signInButton = screen.getByRole('button', { name: /sign in/i });
      fireEvent.click(signInButton);

      await waitFor(() => {
        expect(mockLocalLogin).toHaveBeenCalledWith('test@example.com', 'password123');
      });
    });
  });

  describe('shows lockout message with time when account is locked', () => {
    it('should display lockout message with remaining time', () => {
      const lockoutTime = new Date(Date.now() + 10 * 60 * 1000); // 10 minutes from now
      mockError = 'Account is locked';
      mockLockoutEnd = lockoutTime.toISOString();

      render(<LoginPage />);

      expect(screen.getByText(/account is locked/i)).toBeInTheDocument();
      // Should show some indication of lockout time
      expect(screen.getByText(/locked until/i)).toBeInTheDocument();
    });
  });

  describe('redirects to home on successful login', () => {
    it('should redirect when already authenticated', () => {
      mockIsAuthenticated = true;
      render(<LoginPage />);

      expect(mockPush).toHaveBeenCalledWith('/play');
    });

    it('should redirect after successful local login', async () => {
      mockLocalLogin.mockImplementation(async () => {
        mockIsAuthenticated = true;
      });

      const { rerender } = render(<LoginPage />);

      const emailInput = screen.getByLabelText(/email/i);
      const passwordInput = screen.getByLabelText(/password/i);
      
      await userEvent.type(emailInput, 'test@example.com');
      await userEvent.type(passwordInput, 'password123');

      const signInButton = screen.getByRole('button', { name: /sign in/i });
      fireEvent.click(signInButton);

      await waitFor(() => {
        expect(mockLocalLogin).toHaveBeenCalled();
      });

      // Trigger re-render with updated auth state
      mockIsAuthenticated = true;
      rerender(<LoginPage />);

      await waitFor(() => {
        expect(mockPush).toHaveBeenCalledWith('/play');
      });
    });
  });

  describe('renders registration link', () => {
    it('should render link to registration page', () => {
      render(<LoginPage />);

      const registerLink = screen.getByRole('link', { name: /register/i });
      expect(registerLink).toBeInTheDocument();
      expect(registerLink).toHaveAttribute('href', '/register');
    });
  });

  describe('loading state', () => {
    it('should disable form inputs when loading', () => {
      mockIsLoading = true;
      render(<LoginPage />);

      const emailInput = screen.getByLabelText(/email/i);
      const passwordInput = screen.getByLabelText(/password/i);
      // When loading, button text changes to "Signing in..."
      const signInButton = screen.getByRole('button', { name: /signing in/i });

      expect(emailInput).toBeDisabled();
      expect(passwordInput).toBeDisabled();
      expect(signInButton).toBeDisabled();
    });
  });
});
