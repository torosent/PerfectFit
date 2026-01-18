/**
 * ProfileSettings Component Tests
 * 
 * Tests for the profile settings modal that allows users to
 * edit their display name and avatar.
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
  id: 123,
  displayName: 'testuser',
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
  useIsGuest: () => false,
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

  describe('renders with current display name', () => {
    it('should display the display name input with current value', () => {
      render(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      const displayNameInput = screen.getByLabelText(/display name/i);
      expect(displayNameInput).toHaveValue('testuser');
    });

    it('should display the current avatar as selected', () => {
      render(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      // Open emoji picker popup
      const changeAvatarButton = screen.getByRole('button', { name: /change/i });
      fireEvent.click(changeAvatarButton);

      // Check current avatar is marked as selected
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
    it('should call updateProfile with new display name and avatar on submit', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: true,
        profile: {
          id: 1,
          displayName: 'newdisplayname',
          avatar: '🚀',
        },
      };
      mockUpdateProfile.mockResolvedValueOnce(mockResponse);

      const onClose = jest.fn();
      render(<ProfileSettings isOpen={true} onClose={onClose} />);

      // Change display name
      const displayNameInput = screen.getByLabelText(/display name/i);
      await userEvent.clear(displayNameInput);
      await userEvent.type(displayNameInput, 'newdisplayname');

      // Open emoji picker popup and select new avatar
      const changeAvatarButton = screen.getByRole('button', { name: /change/i });
      fireEvent.click(changeAvatarButton);
      const newAvatarButton = screen.getByRole('button', { name: '🚀' });
      fireEvent.click(newAvatarButton);

      // Submit form
      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(mockUpdateProfile).toHaveBeenCalledWith(
          {
            displayName: 'newdisplayname',
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
          displayName: 'newdisplayname',
          avatar: '😎',
        },
      };
      mockUpdateProfile.mockResolvedValueOnce(mockResponse);

      const onClose = jest.fn();
      render(<ProfileSettings isOpen={true} onClose={onClose} />);

      // Change display name to trigger API call
      const displayNameInput = screen.getByLabelText(/display name/i);
      await userEvent.clear(displayNameInput);
      await userEvent.type(displayNameInput, 'newdisplayname');

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

      // Change display name to trigger API call
      const displayNameInput = screen.getByLabelText(/display name/i);
      await userEvent.clear(displayNameInput);
      await userEvent.type(displayNameInput, 'newdisplayname');

      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      // Button should be disabled during loading
      await waitFor(() => {
        expect(saveButton).toBeDisabled();
      });

      // Resolve the promise
      resolvePromise!({
        success: true,
        profile: { id: 1, displayName: 'newdisplayname', avatar: '😎' },
      });
    });
  });

  describe('shows error message on failure', () => {
    it('should display error message when API returns error', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: false,
        errorMessage: 'Display name is already taken',
      };
      mockUpdateProfile.mockResolvedValueOnce(mockResponse);

      render(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      // Change display name to trigger API call
      const displayNameInput = screen.getByLabelText(/display name/i);
      await userEvent.clear(displayNameInput);
      await userEvent.type(displayNameInput, 'newdisplayname');

      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText(/display name is already taken/i)).toBeInTheDocument();
      });
    });

    it('should not close modal when there is an error', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: false,
        errorMessage: 'Display name is already taken',
      };
      mockUpdateProfile.mockResolvedValueOnce(mockResponse);

      const onClose = jest.fn();
      render(<ProfileSettings isOpen={true} onClose={onClose} />);

      // Change display name to trigger API call
      const displayNameInput = screen.getByLabelText(/display name/i);
      await userEvent.clear(displayNameInput);
      await userEvent.type(displayNameInput, 'newdisplayname');

      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText(/display name is already taken/i)).toBeInTheDocument();
      });

      expect(onClose).not.toHaveBeenCalled();
    });
  });

  describe('shows suggested display name when API suggests one', () => {
    it('should display suggested display name when API returns one', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: false,
        errorMessage: 'Display name is already taken',
        suggestedDisplayName: 'testuser123',
      };
      mockUpdateProfile.mockResolvedValueOnce(mockResponse);

      render(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      // Change display name to trigger API call
      const displayNameInput = screen.getByLabelText(/display name/i);
      await userEvent.clear(displayNameInput);
      await userEvent.type(displayNameInput, 'newdisplayname');

      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText(/testuser123/)).toBeInTheDocument();
      });
    });

    it('should allow clicking suggested display name to use it', async () => {
      const mockResponse: UpdateProfileResponse = {
        success: false,
        errorMessage: 'Display name is already taken',
        suggestedDisplayName: 'testuser123',
      };
      mockUpdateProfile.mockResolvedValueOnce(mockResponse);

      render(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      // Change display name to trigger API call
      const displayNameInput = screen.getByLabelText(/display name/i);
      await userEvent.clear(displayNameInput);
      await userEvent.type(displayNameInput, 'newdisplayname');

      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        const suggestionButton = screen.getByRole('button', { name: /use testuser123/i });
        fireEvent.click(suggestionButton);
      });

      const displayNameInputAfter = screen.getByLabelText(/display name/i);
      expect(displayNameInputAfter).toHaveValue('testuser123');
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

      // Change display name
      const displayNameInput = screen.getByLabelText(/display name/i);
      await userEvent.clear(displayNameInput);
      await userEvent.type(displayNameInput, 'changedname');

      // Close modal
      rerender(<ProfileSettings isOpen={false} onClose={jest.fn()} />);

      // Reopen modal
      rerender(<ProfileSettings isOpen={true} onClose={jest.fn()} />);

      // Display name should be reset to original
      const newDisplayNameInput = screen.getByLabelText(/display name/i);
      expect(newDisplayNameInput).toHaveValue('testuser');
    });
  });
});
