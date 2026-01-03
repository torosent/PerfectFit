import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { UserMenu } from '@/components/auth/UserMenu';
import { useAuthStore } from '@/lib/stores/auth-store';
import type { UserProfile } from '@/types';

// Mock the auth store
jest.mock('@/lib/stores/auth-store', () => ({
  useAuthStore: jest.fn(),
  useUser: jest.fn(),
  useIsGuest: jest.fn(),
}));

// Mock ProfileSettings to avoid testing it here
jest.mock('@/components/profile', () => ({
  ProfileSettings: ({ isOpen, onClose }: { isOpen: boolean; onClose: () => void }) => (
    isOpen ? (
      <div data-testid="profile-settings-modal">
        <button onClick={onClose}>Close</button>
      </div>
    ) : null
  ),
}));

const mockUser: UserProfile = {
  id: 'user-123',
  displayName: 'John Doe',
  email: 'john@example.com',
  provider: 'google',
  highScore: 1000,
  gamesPlayed: 50,
  avatar: undefined,
};

const mockUserWithAvatar: UserProfile = {
  ...mockUser,
  avatar: '🎮',
};

const mockLogout = jest.fn();

describe('UserMenu', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (useAuthStore as unknown as jest.Mock).mockReturnValue(mockLogout);
    (require('@/lib/stores/auth-store').useUser as jest.Mock).mockReturnValue(mockUser);
    (require('@/lib/stores/auth-store').useIsGuest as jest.Mock).mockReturnValue(false);
  });

  it('renders nothing when user is null', () => {
    (require('@/lib/stores/auth-store').useUser as jest.Mock).mockReturnValue(null);
    
    const { container } = render(<UserMenu />);
    
    expect(container.firstChild).toBeNull();
  });

  it('renders avatar button with initials when no emoji avatar', () => {
    render(<UserMenu />);
    
    const avatarButton = screen.getByRole('button', { name: /user menu/i });
    expect(avatarButton).toBeInTheDocument();
    expect(screen.getByText('JD')).toBeInTheDocument(); // Initials for John Doe
  });

  it('renders avatar button with emoji when avatar is set', () => {
    (require('@/lib/stores/auth-store').useUser as jest.Mock).mockReturnValue(mockUserWithAvatar);
    
    render(<UserMenu />);
    
    expect(screen.getByText('🎮')).toBeInTheDocument();
  });

  it('opens dropdown menu when avatar button is clicked', () => {
    render(<UserMenu />);
    
    const avatarButton = screen.getByRole('button', { name: /user menu/i });
    fireEvent.click(avatarButton);
    
    expect(screen.getByRole('menu')).toBeInTheDocument();
    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.getByText('john@example.com')).toBeInTheDocument();
  });

  it('displays user stats in dropdown', () => {
    render(<UserMenu />);
    
    fireEvent.click(screen.getByRole('button', { name: /user menu/i }));
    
    expect(screen.getByText('High Score')).toBeInTheDocument();
    expect(screen.getByText('1,000')).toBeInTheDocument();
    expect(screen.getByText('Games Played')).toBeInTheDocument();
    expect(screen.getByText('50')).toBeInTheDocument();
  });

  it('shows Guest badge when user is guest', () => {
    (require('@/lib/stores/auth-store').useIsGuest as jest.Mock).mockReturnValue(true);
    
    render(<UserMenu />);
    
    fireEvent.click(screen.getByRole('button', { name: /user menu/i }));
    
    expect(screen.getByText('Guest')).toBeInTheDocument();
  });

  it('opens profile settings modal when Edit Profile is clicked', () => {
    render(<UserMenu />);
    
    // Open dropdown
    fireEvent.click(screen.getByRole('button', { name: /user menu/i }));
    
    // Click Edit Profile
    fireEvent.click(screen.getByRole('menuitem', { name: /edit profile/i }));
    
    // Profile settings modal should be open
    expect(screen.getByTestId('profile-settings-modal')).toBeInTheDocument();
  });

  it('closes dropdown when Edit Profile is clicked', () => {
    render(<UserMenu />);
    
    // Open dropdown
    fireEvent.click(screen.getByRole('button', { name: /user menu/i }));
    expect(screen.getByRole('menu')).toBeInTheDocument();
    
    // Click Edit Profile
    fireEvent.click(screen.getByRole('menuitem', { name: /edit profile/i }));
    
    // Dropdown should be closed
    expect(screen.queryByRole('menu')).not.toBeInTheDocument();
  });

  it('calls logout when Sign out is clicked', async () => {
    render(<UserMenu />);
    
    // Open dropdown
    fireEvent.click(screen.getByRole('button', { name: /user menu/i }));
    
    // Click Sign out
    fireEvent.click(screen.getByRole('menuitem', { name: /sign out/i }));
    
    await waitFor(() => {
      expect(mockLogout).toHaveBeenCalled();
    });
  });

  it('closes dropdown when clicking outside', () => {
    render(
      <div>
        <div data-testid="outside">Outside</div>
        <UserMenu />
      </div>
    );
    
    // Open dropdown
    fireEvent.click(screen.getByRole('button', { name: /user menu/i }));
    expect(screen.getByRole('menu')).toBeInTheDocument();
    
    // Click outside
    fireEvent.mouseDown(screen.getByTestId('outside'));
    
    expect(screen.queryByRole('menu')).not.toBeInTheDocument();
  });

  it('closes dropdown when Escape is pressed', () => {
    render(<UserMenu />);
    
    // Open dropdown
    fireEvent.click(screen.getByRole('button', { name: /user menu/i }));
    expect(screen.getByRole('menu')).toBeInTheDocument();
    
    // Press Escape
    fireEvent.keyDown(document, { key: 'Escape' });
    
    expect(screen.queryByRole('menu')).not.toBeInTheDocument();
  });

  it('generates correct initials for single word names', () => {
    (require('@/lib/stores/auth-store').useUser as jest.Mock).mockReturnValue({
      ...mockUser,
      displayName: 'Player',
    });
    
    render(<UserMenu />);
    
    expect(screen.getByText('PL')).toBeInTheDocument();
  });
});
