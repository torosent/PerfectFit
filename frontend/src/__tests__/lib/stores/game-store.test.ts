import { act } from '@testing-library/react';
import type { GameEndGamification } from '@/types/gamification';
import { useAuthStore } from '@/lib/stores/auth-store';
import { useGameStore } from '@/lib/stores/game-store';

// Mocks
const mockEndGame = jest.fn();
const mockProcessGameEndGamification = jest.fn();

jest.mock('@/lib/api/game-client', () => ({
  endGame: (...args: unknown[]) => mockEndGame(...args),
  createGame: jest.fn(),
  getGame: jest.fn(),
  placePiece: jest.fn(),
}));

jest.mock('@/lib/stores/gamification-store', () => ({
  useGamificationStore: {
    getState: () => ({ processGameEndGamification: mockProcessGameEndGamification }),
  },
}));

describe('Game Store - endCurrentGame', () => {
  beforeEach(() => {
    jest.clearAllMocks();

    act(() => {
      useAuthStore.setState({
        user: {
          id: 'user-1',
          displayName: 'Test User',
          provider: 'local',
          highScore: 100,
          gamesPlayed: 1,
        } as any,
        token: 'token-123',
        isAuthenticated: true,
        isLoading: false,
        isInitialized: true,
        error: null,
        lockoutEnd: null,
      });

      useGameStore.setState({
        gameState: {
          id: 'game-1',
          grid: [],
          currentPieces: [],
          score: 0,
          combo: 0,
          status: 'Playing',
          linesCleared: 0,
        } as any,
        isLoading: false,
        error: null,
      });
    });
  });

  it('updates auth store gamesPlayed and highScore when gamification is returned', async () => {
    const gamification: GameEndGamification = {
      streak: {
        currentStreak: 2,
        longestStreak: 5,
        freezeTokens: 0,
        isAtRisk: false,
        resetTime: new Date().toISOString(),
      },
      challengeUpdates: [],
      newAchievements: [],
      seasonProgress: {
        xpEarned: 10,
        totalXP: 110,
        newTier: 0,
        tierUp: false,
        newRewardsCount: 0,
      },
      goalUpdates: [],
      gamesPlayed: 2,
      highScore: 999,
    };

    mockEndGame.mockResolvedValue({
      gameState: {
        id: 'game-1',
        grid: [],
        currentPieces: [],
        score: 999,
        combo: 0,
        status: 'Ended',
        linesCleared: 0,
      },
      gamification,
    });

    await act(async () => {
      await useGameStore.getState().endCurrentGame();
    });

    const user = useAuthStore.getState().user;
    expect(user?.gamesPlayed).toBe(gamification.gamesPlayed);
    expect(user?.highScore).toBe(gamification.highScore);
    expect(mockProcessGameEndGamification).toHaveBeenCalledWith(gamification);
  });
});
