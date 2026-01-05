/**
 * Verify Email Page Tests
 *
 * Tests for the email verification page that handles
 * email verification tokens from URL params.
 */

import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import VerifyEmailPage from '@/app/(auth)/verify-email/page';

// Mock next/navigation
const mockPush = jest.fn();
let mockSearchParams = new URLSearchParams();

jest.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
  useSearchParams: () => mockSearchParams,
}));

// Mock auth client
const mockVerifyEmail = jest.fn();

jest.mock('@/lib/api/auth-client', () => ({
  verifyEmail: (...args: unknown[]) => mockVerifyEmail(...args),
}));

describe('VerifyEmailPage', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockSearchParams = new URLSearchParams();
  });

  describe('shows loading state initially', () => {
    it('should show loading state when verification starts', () => {
      mockSearchParams = new URLSearchParams('email=test@example.com&token=abc123');
      mockVerifyEmail.mockImplementation(() => new Promise(() => {})); // Never resolves

      render(<VerifyEmailPage />);

      expect(screen.getByText(/verifying/i)).toBeInTheDocument();
    });
  });

  describe('shows success message on successful verification', () => {
    it('should show success message when verification succeeds', async () => {
      mockSearchParams = new URLSearchParams('email=test@example.com&token=abc123');
      mockVerifyEmail.mockResolvedValueOnce({ success: true });

      render(<VerifyEmailPage />);

      await waitFor(() => {
        expect(screen.getByText(/email verified/i)).toBeInTheDocument();
      });
    });

    it('should call verifyEmail with correct parameters', async () => {
      mockSearchParams = new URLSearchParams('email=test@example.com&token=abc123');
      mockVerifyEmail.mockResolvedValueOnce({ success: true });

      render(<VerifyEmailPage />);

      await waitFor(() => {
        expect(mockVerifyEmail).toHaveBeenCalledWith('test@example.com', 'abc123');
      });
    });
  });

  describe('shows error message on failed verification', () => {
    it('should show error message when verification fails', async () => {
      mockSearchParams = new URLSearchParams('email=test@example.com&token=invalid');
      mockVerifyEmail.mockResolvedValueOnce({
        success: false,
        error: 'Invalid or expired verification token',
      });

      render(<VerifyEmailPage />);

      await waitFor(() => {
        expect(screen.getByText(/invalid or expired verification token/i)).toBeInTheDocument();
      });
    });

    it('should show error message when API throws', async () => {
      mockSearchParams = new URLSearchParams('email=test@example.com&token=abc123');
      mockVerifyEmail.mockRejectedValueOnce(new Error('Network error'));

      render(<VerifyEmailPage />);

      await waitFor(() => {
        expect(screen.getByText(/network error/i)).toBeInTheDocument();
      });
    });
  });

  describe('shows link to login after success', () => {
    it('should render link to login page after successful verification', async () => {
      mockSearchParams = new URLSearchParams('email=test@example.com&token=abc123');
      mockVerifyEmail.mockResolvedValueOnce({ success: true });

      render(<VerifyEmailPage />);

      await waitFor(() => {
        const loginLink = screen.getByRole('link', { name: /sign in/i });
        expect(loginLink).toBeInTheDocument();
        expect(loginLink).toHaveAttribute('href', '/login');
      });
    });
  });

  describe('handles missing email/token params', () => {
    it('should show error when email param is missing', async () => {
      mockSearchParams = new URLSearchParams('token=abc123');

      render(<VerifyEmailPage />);

      await waitFor(() => {
        expect(screen.getByText(/invalid verification link/i)).toBeInTheDocument();
      });

      expect(mockVerifyEmail).not.toHaveBeenCalled();
    });

    it('should show error when token param is missing', async () => {
      mockSearchParams = new URLSearchParams('email=test@example.com');

      render(<VerifyEmailPage />);

      await waitFor(() => {
        expect(screen.getByText(/invalid verification link/i)).toBeInTheDocument();
      });

      expect(mockVerifyEmail).not.toHaveBeenCalled();
    });

    it('should show error when both params are missing', async () => {
      mockSearchParams = new URLSearchParams('');

      render(<VerifyEmailPage />);

      await waitFor(() => {
        expect(screen.getByText(/invalid verification link/i)).toBeInTheDocument();
      });

      expect(mockVerifyEmail).not.toHaveBeenCalled();
    });
  });
});
