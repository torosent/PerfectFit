/**
 * Register Page Tests
 *
 * Tests for the registration page with email, display name,
 * password, and confirm password inputs.
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import RegisterPage from '@/app/(auth)/register/page';

// Mock next/navigation
const mockPush = jest.fn();
jest.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}));

// Mock auth store
const mockLocalRegister = jest.fn();
const mockClearError = jest.fn();

let mockIsLoading = false;
let mockError: string | null = null;

jest.mock('@/lib/stores/auth-store', () => ({
  useIsAuthLoading: () => mockIsLoading,
  useAuthError: () => mockError,
  useAuthStore: jest.fn((selector) => {
    const state = {
      localRegister: mockLocalRegister,
      clearError: mockClearError,
    };
    if (typeof selector === 'function') {
      return selector(state);
    }
    return state;
  }),
}));

describe('RegisterPage', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockIsLoading = false;
    mockError = null;
  });

  describe('renders email, display name, password, confirm password inputs', () => {
    it('should render email input field', () => {
      render(<RegisterPage />);

      const emailInput = screen.getByLabelText(/email/i);
      expect(emailInput).toBeInTheDocument();
      expect(emailInput).toHaveAttribute('type', 'email');
    });

    it('should render display name input field', () => {
      render(<RegisterPage />);

      const displayNameInput = screen.getByLabelText(/display name/i);
      expect(displayNameInput).toBeInTheDocument();
      expect(displayNameInput).toHaveAttribute('type', 'text');
    });

    it('should render password input field', () => {
      render(<RegisterPage />);

      const passwordInput = screen.getByLabelText(/^password$/i);
      expect(passwordInput).toBeInTheDocument();
      expect(passwordInput).toHaveAttribute('type', 'password');
    });

    it('should render confirm password input field', () => {
      render(<RegisterPage />);

      const confirmPasswordInput = screen.getByLabelText(/confirm password/i);
      expect(confirmPasswordInput).toBeInTheDocument();
      expect(confirmPasswordInput).toHaveAttribute('type', 'password');
    });

    it('should render register button', () => {
      render(<RegisterPage />);

      const registerButton = screen.getByRole('button', { name: /create account/i });
      expect(registerButton).toBeInTheDocument();
    });
  });

  describe('shows validation error for empty fields', () => {
    it('should show error when submitting with empty email', async () => {
      render(<RegisterPage />);

      const displayNameInput = screen.getByLabelText(/display name/i);
      const passwordInput = screen.getByLabelText(/^password$/i);
      const confirmPasswordInput = screen.getByLabelText(/confirm password/i);

      await userEvent.type(displayNameInput, 'Test User');
      await userEvent.type(passwordInput, 'Password1');
      await userEvent.type(confirmPasswordInput, 'Password1');

      const registerButton = screen.getByRole('button', { name: /create account/i });
      fireEvent.click(registerButton);

      await waitFor(() => {
        expect(screen.getByText(/email is required/i)).toBeInTheDocument();
      });

      expect(mockLocalRegister).not.toHaveBeenCalled();
    });

    it('should show error when submitting with empty display name', async () => {
      render(<RegisterPage />);

      const emailInput = screen.getByLabelText(/email/i);
      const passwordInput = screen.getByLabelText(/^password$/i);
      const confirmPasswordInput = screen.getByLabelText(/confirm password/i);

      await userEvent.type(emailInput, 'test@example.com');
      await userEvent.type(passwordInput, 'Password1');
      await userEvent.type(confirmPasswordInput, 'Password1');

      const registerButton = screen.getByRole('button', { name: /create account/i });
      fireEvent.click(registerButton);

      await waitFor(() => {
        expect(screen.getByText(/display name is required/i)).toBeInTheDocument();
      });

      expect(mockLocalRegister).not.toHaveBeenCalled();
    });

    it('should show error when submitting with empty password', async () => {
      render(<RegisterPage />);

      const emailInput = screen.getByLabelText(/email/i);
      const displayNameInput = screen.getByLabelText(/display name/i);

      await userEvent.type(emailInput, 'test@example.com');
      await userEvent.type(displayNameInput, 'Test User');

      const registerButton = screen.getByRole('button', { name: /create account/i });
      fireEvent.click(registerButton);

      await waitFor(() => {
        expect(screen.getByText(/password is required/i)).toBeInTheDocument();
      });

      expect(mockLocalRegister).not.toHaveBeenCalled();
    });
  });

  describe('shows password strength indicator', () => {
    it('should render password strength indicator', () => {
      render(<RegisterPage />);

      expect(screen.getByTestId('password-strength-indicator')).toBeInTheDocument();
    });

    it('should show password requirements', () => {
      render(<RegisterPage />);

      expect(screen.getByText(/8\+ characters/i)).toBeInTheDocument();
      expect(screen.getByText(/uppercase letter/i)).toBeInTheDocument();
      expect(screen.getByText(/lowercase letter/i)).toBeInTheDocument();
      expect(screen.getByText(/number/i)).toBeInTheDocument();
    });
  });

  describe('shows error when passwords dont match', () => {
    it('should show error when passwords do not match on submit', async () => {
      render(<RegisterPage />);

      const emailInput = screen.getByLabelText(/email/i);
      const displayNameInput = screen.getByLabelText(/display name/i);
      const passwordInput = screen.getByLabelText(/^password$/i);
      const confirmPasswordInput = screen.getByLabelText(/confirm password/i);

      await userEvent.type(emailInput, 'test@example.com');
      await userEvent.type(displayNameInput, 'Test User');
      await userEvent.type(passwordInput, 'Password1');
      await userEvent.type(confirmPasswordInput, 'Password2');

      const registerButton = screen.getByRole('button', { name: /create account/i });
      fireEvent.click(registerButton);

      await waitFor(() => {
        expect(screen.getByText(/passwords do not match/i)).toBeInTheDocument();
      });

      expect(mockLocalRegister).not.toHaveBeenCalled();
    });

    it('should show error when password does not meet requirements', async () => {
      render(<RegisterPage />);

      const emailInput = screen.getByLabelText(/email/i);
      const displayNameInput = screen.getByLabelText(/display name/i);
      const passwordInput = screen.getByLabelText(/^password$/i);
      const confirmPasswordInput = screen.getByLabelText(/confirm password/i);

      await userEvent.type(emailInput, 'test@example.com');
      await userEvent.type(displayNameInput, 'Test User');
      await userEvent.type(passwordInput, 'weak');
      await userEvent.type(confirmPasswordInput, 'weak');

      const registerButton = screen.getByRole('button', { name: /create account/i });
      fireEvent.click(registerButton);

      await waitFor(() => {
        expect(screen.getByText(/password does not meet requirements/i)).toBeInTheDocument();
      });

      expect(mockLocalRegister).not.toHaveBeenCalled();
    });
  });

  describe('shows success message after registration', () => {
    it('should show success message when registration is successful', async () => {
      mockLocalRegister.mockResolvedValueOnce({
        success: true,
        message: 'Registration successful. Please check your email to verify your account.',
      });

      render(<RegisterPage />);

      const emailInput = screen.getByLabelText(/email/i);
      const displayNameInput = screen.getByLabelText(/display name/i);
      const passwordInput = screen.getByLabelText(/^password$/i);
      const confirmPasswordInput = screen.getByLabelText(/confirm password/i);

      await userEvent.type(emailInput, 'test@example.com');
      await userEvent.type(displayNameInput, 'Test User');
      await userEvent.type(passwordInput, 'Password1');
      await userEvent.type(confirmPasswordInput, 'Password1');

      const registerButton = screen.getByRole('button', { name: /create account/i });
      fireEvent.click(registerButton);

      await waitFor(() => {
        // Look for the success alert with the check your email message
        expect(screen.getByRole('alert')).toBeInTheDocument();
        expect(screen.getByText(/check your email!/i)).toBeInTheDocument();
      });
    });
  });

  describe('shows API error message on failure', () => {
    it('should show API error when registration fails', async () => {
      mockLocalRegister.mockResolvedValueOnce({
        success: false,
        error: 'Email already registered',
      });

      render(<RegisterPage />);

      const emailInput = screen.getByLabelText(/email/i);
      const displayNameInput = screen.getByLabelText(/display name/i);
      const passwordInput = screen.getByLabelText(/^password$/i);
      const confirmPasswordInput = screen.getByLabelText(/confirm password/i);

      await userEvent.type(emailInput, 'test@example.com');
      await userEvent.type(displayNameInput, 'Test User');
      await userEvent.type(passwordInput, 'Password1');
      await userEvent.type(confirmPasswordInput, 'Password1');

      const registerButton = screen.getByRole('button', { name: /create account/i });
      fireEvent.click(registerButton);

      await waitFor(() => {
        expect(screen.getByText(/email already registered/i)).toBeInTheDocument();
      });
    });

    it('should display error from auth store', () => {
      mockError = 'Network error';
      render(<RegisterPage />);

      expect(screen.getByText(/network error/i)).toBeInTheDocument();
    });
  });

  describe('renders link to login page', () => {
    it('should render link to login page', () => {
      render(<RegisterPage />);

      const loginLink = screen.getByRole('link', { name: /sign in/i });
      expect(loginLink).toBeInTheDocument();
      expect(loginLink).toHaveAttribute('href', '/login');
    });
  });

  describe('disables form when loading', () => {
    it('should disable form inputs when loading', () => {
      mockIsLoading = true;
      render(<RegisterPage />);

      const emailInput = screen.getByLabelText(/email/i);
      const displayNameInput = screen.getByLabelText(/display name/i);
      const passwordInput = screen.getByLabelText(/^password$/i);
      const confirmPasswordInput = screen.getByLabelText(/confirm password/i);
      const registerButton = screen.getByRole('button', { name: /creating account/i });

      expect(emailInput).toBeDisabled();
      expect(displayNameInput).toBeDisabled();
      expect(passwordInput).toBeDisabled();
      expect(confirmPasswordInput).toBeDisabled();
      expect(registerButton).toBeDisabled();
    });
  });

  describe('successful registration flow', () => {
    it('should call localRegister with correct parameters', async () => {
      mockLocalRegister.mockResolvedValueOnce({
        success: true,
        message: 'Registration successful',
      });

      render(<RegisterPage />);

      const emailInput = screen.getByLabelText(/email/i);
      const displayNameInput = screen.getByLabelText(/display name/i);
      const passwordInput = screen.getByLabelText(/^password$/i);
      const confirmPasswordInput = screen.getByLabelText(/confirm password/i);

      await userEvent.type(emailInput, 'test@example.com');
      await userEvent.type(displayNameInput, 'Test User');
      await userEvent.type(passwordInput, 'Password1');
      await userEvent.type(confirmPasswordInput, 'Password1');

      const registerButton = screen.getByRole('button', { name: /create account/i });
      fireEvent.click(registerButton);

      await waitFor(() => {
        expect(mockLocalRegister).toHaveBeenCalledWith(
          'test@example.com',
          'Password1',
          'Test User',
          undefined
        );
      });
    });
  });
});
