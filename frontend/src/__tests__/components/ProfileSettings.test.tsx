/**
 * ProfileSettings Component Tests
 * 
 * Tests for the profile settings modal that allows users to
 * edit their username and avatar.
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ProfileSettings } from '@/components/profile/ProfileSettings';
import { updateProfile } from '@/lib/api/profile-client';
import type { UpdateProfileResponse } from '@/types';

// Mock the profile client
jest.mock('@/lib/api/profile-client', () => ({
  updateProfile: jest.fn(),
}));

// Mock the auth store
const mockUser = {
  id: 'user-123',
  displayName: 'Test User',
  username: 'testuser',
  email: 'test@example.com',
  provider: 'google' as const,
  highScore: 1000,
  gamesPlayed: 10,
  avatar: '😎',
};

const mockSetUser = jest.fn();
const mockToken = 'test-jwt-token';

jest.mock('@/lib/stores/auth-store', () => ({
  useUser: () => mockUser,
  useToken: () => mockToken,
  useAuthStore: jest.fn((selector: (state: { user: typeof mockUser; setUser: typeof mockSetUser }) => unknown) => {
    if (typeof selector === 'function') {
      return selector({ user: mockUser, setUser: mockSetUser });
    }
    return { user: mockUser, setUser: mockSetUser };
  }),
}));

const mockUpdateProfile = updateProfile as jest.MockedFunction<typeof updateProfile>;

describe('ProfileSettings', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('renders with current username', () => {
    it('should display the username input with current username', () => {
      render(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      const usernameInput = screen.getByLabelText(/username/i);
      expect(usernameInput).toHaveValue('testuser');
    });

    it('should display the current avatar as selected', () => {
      render(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      const selectedButton = screen.getByRole('button', { name: '😎' });
      expect(selectedButton).toHaveAttribute('aria-pressed', 'true');
    });

    it('should not render when isOpen is false', () => {
      render(<ProfileSettings isOpen={false} onClose={jest.fn()} />);

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });

    it('should render dialog when isOpen is true', () => {
      render(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });
  });

  describe('submits updated profile', () => {
    it('should call updateProfile with new username and avatar on submit', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: true,
        profile: {
          id: 1,
          username: 'newusername',
          avatar: '🚀',
        },
      };
      mockUpdateProfile.mockResolvedValueOnce(mockResponse);

      const onClose = jest.fn();
      render(<ProfileSettings isOpen={true} onClose={onClose} />);

      // Change username
      const usernameInput = screen.getByLabelText(/username/i);
      await userEvent.clear(usernameInput);
      await userEvent.type(usernameInput, 'newusername');

      // Select new avatar
      const newAvatarButton = screen.getByRole('button', { name: '🚀' });
      fireEvent.click(newAvatarButton);

      // Submit form
      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(mockUpdateProfile).toHaveBeenCalledWith(
          {
            username: 'newusername',
            avatar: '🚀',
          },
          mockToken
        );
      });
    });

    it('should close modal on successful submit', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: true,
        profile: {
          id: 1,
          username: 'testuser',
          avatar: '😎',
        },
      };
      mockUpdateProfile.mockResolvedValueOnce(mockResponse);

      const onClose = jest.fn();
      render(<ProfileSettings isOpen={true} onClose={onClose} />);

      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(onClose).toHaveBeenCalled();
      });
    });

    it('should show loading state while submitting', async () => {
      // Create a promise that we control
      let resolvePromise: (value: UpdateProfileResponse) => void;
      const pendingPromise = new Promise<UpdateProfileResponse>((resolve) => {
        resolvePromise = resolve;
      });
      mockUpdateProfile.mockReturnValueOnce(pendingPromise);

      render(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      // Button should be disabled during loading
      await waitFor(() => {
        expect(saveButton).toBeDisabled();
      });

      // Resolve the promise
      resolvePromise!({
        success: true,
        profile: { id: 1, username: 'testuser', avatar: '😎' },
      });
    });
  });

  describe('shows error message on failure', () => {
    it('should display error message when API returns error', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: false,
        errorMessage: 'Username is already taken',
      };
      mockUpdateProfile.mockResolvedValueOnce(mockResponse);

      render(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText(/username is already taken/i)).toBeInTheDocument();
      });
    });

    it('should not close modal when there is an error', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: false,
        errorMessage: 'Username is already taken',
      };
      mockUpdateProfile.mockResolvedValueOnce(mockResponse);

      const onClose = jest.fn();
      render(<ProfileSettings isOpen={true} onClose={onClose} />);

      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText(/username is already taken/i)).toBeInTheDocument();
      });

      expect(onClose).not.toHaveBeenCalled();
    });
  });

  describe('shows suggested username when API suggests one', () => {
    it('should display suggested username when API returns one', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: false,
        errorMessage: 'Username is already taken',
        suggestedUsername: 'testuser123',
      };
      mockUpdateProfile.mockResolvedValueOnce(mockResponse);

      render(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText(/testuser123/)).toBeInTheDocument();
      });
    });

    it('should allow clicking suggested username to use it', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: false,
        errorMessage: 'Username is already taken',
        suggestedUsername: 'testuser123',
      };
      mockUpdateProfile.mockResolvedValueOnce(mockResponse);

      render(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        const suggestionButton = screen.getByRole('button', { name: /use testuser123/i });
        fireEvent.click(suggestionButton);
      });

      const usernameInput = screen.getByLabelText(/username/i);
      expect(usernameInput).toHaveValue('testuser123');
    });
  });

  describe('cancel behavior', () => {
    it('should call onClose when cancel button is clicked', () => {
      const onClose = jest.fn();
      render(<ProfileSettings isOpen={true} onClose={onClose} />);

      const cancelButton = screen.getByRole('button', { name: /cancel/i });
      fireEvent.click(cancelButton);

      expect(onClose).toHaveBeenCalled();
    });

    it('should reset form when reopening modal', async () => {
      const { rerender } = render(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      // Change username
      const usernameInput = screen.getByLabelText(/username/i);
      await userEvent.clear(usernameInput);
      await userEvent.type(usernameInput, 'changedname');

      // Close modal
      rerender(<ProfileSettings isOpen={false} onClose={jest.fn()} />);

      // Reopen modal
      rerender(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      // Username should be reset to original
      const newUsernameInput = screen.getByLabelText(/username/i);
      expect(newUsernameInput).toHaveValue('testuser');
    });
  });
});
