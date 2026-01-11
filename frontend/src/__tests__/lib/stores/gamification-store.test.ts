/**
 * Gamification Store Tests
 *
 * Tests for the gamification Zustand store including state management,
 * API integration, and achievement handling.
 */

import { act, renderHook } from '@testing-library/react';
import type {
  GamificationStatus,
  Challenge,
  AchievementsResult,
  SeasonPassResult,
  CosmeticsResult,
  PersonalGoal,
  ApiResult,
  ClaimRewardResult,
  AchievementUnlock,
  GameEndGamification,
  StreakInfo,
} from '@/types/gamification';

// Create mock functions
const mockGetGamificationStatus = jest.fn<Promise<GamificationStatus>, [string]>();
const mockGetChallenges = jest.fn<Promise<Challenge[]>, [string, string?]>();
const mockGetAchievements = jest.fn<Promise<AchievementsResult>, [string]>();
const mockGetSeasonPass = jest.fn<Promise<SeasonPassResult>, [string]>();
const mockGetCosmetics = jest.fn<Promise<CosmeticsResult>, [string, string?]>();
const mockGetPersonalGoals = jest.fn<Promise<PersonalGoal[]>, [string]>();
const mockUseStreakFreeze = jest.fn<Promise<ApiResult>, [string]>();
const mockEquipCosmetic = jest.fn<Promise<ApiResult>, [string, string]>();
const mockClaimSeasonReward = jest.fn<Promise<ClaimRewardResult>, [string, string]>();
const mockSetTimezone = jest.fn<Promise<ApiResult>, [string, string]>();

// Mock the gamification API client
jest.mock('@/lib/api/gamification-client', () => ({
  getGamificationStatus: (...args: [string]) => mockGetGamificationStatus(...args),
  getChallenges: (...args: [string, string?]) => mockGetChallenges(...args),
  getAchievements: (...args: [string]) => mockGetAchievements(...args),
  getSeasonPass: (...args: [string]) => mockGetSeasonPass(...args),
  getCosmetics: (...args: [string, string?]) => mockGetCosmetics(...args),
  getPersonalGoals: (...args: [string]) => mockGetPersonalGoals(...args),
  useStreakFreeze: (...args: [string]) => mockUseStreakFreeze(...args),
  equipCosmetic: (...args: [string, string]) => mockEquipCosmetic(...args),
  claimSeasonReward: (...args: [string, string]) => mockClaimSeasonReward(...args),
  setTimezone: (...args: [string, string]) => mockSetTimezone(...args),
  GamificationApiError: class GamificationApiError extends Error {
    constructor(message: string, public statusCode: number, public details?: unknown) {
      super(message);
      this.name = 'GamificationApiError';
    }
  },
}));

// Mock auth store
jest.mock('@/lib/stores/auth-store', () => ({
  useAuthStore: {
    getState: () => ({
      token: 'test-token',
      isAuthenticated: true,
    }),
  },
}));

// Import store after mocking
import { useGamificationStore } from '@/lib/stores/gamification-store';

// Test data factories
const createMockStreakInfo = (overrides?: Partial<StreakInfo>): StreakInfo => ({
  currentStreak: 5,
  longestStreak: 10,
  freezeTokens: 2,
  isAtRisk: false,
  resetTime: '2026-01-10T00:00:00Z',
  ...overrides,
});

const createMockChallenge = (overrides?: Partial<Challenge>): Challenge => ({
  id: 1,
  name: 'Daily Challenge',
  description: 'Complete 3 games today',
  type: 'Daily',
  targetValue: 3,
  currentProgress: 1,
  xpReward: 100,
  isCompleted: false,
  endsAt: '2026-01-10T00:00:00Z',
  ...overrides,
});

const createMockGamificationStatus = (overrides?: Partial<GamificationStatus>): GamificationStatus => ({
  streak: createMockStreakInfo(),
  activeChallenges: [createMockChallenge()],
  recentAchievements: [],
  seasonPass: null,
  equippedCosmetics: {
    boardTheme: null,
    avatarFrame: null,
    badge: null,
  },
  activeGoals: [],
  ...overrides,
});

const createMockAchievementUnlock = (overrides?: Partial<AchievementUnlock>): AchievementUnlock => ({
  achievementId: 1,
  name: 'First Win',
  description: 'Win your first game',
  iconUrl: '/icons/first-win.png',
  rewardType: 'XPBoost',
  rewardValue: 100,
  ...overrides,
});

describe('Gamification Store', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Reset store state
    act(() => {
      useGamificationStore.getState().reset();
    });
  });

  describe('Initial State', () => {
    it('has correct initial state', () => {
      const state = useGamificationStore.getState();
      
      expect(state.status).toBeNull();
      expect(state.challenges).toEqual([]);
      expect(state.achievements).toEqual([]);
      expect(state.seasonPass).toBeNull();
      expect(state.cosmetics).toEqual([]);
      expect(state.equippedCosmetics).toBeNull();
      expect(state.personalGoals).toEqual([]);
      expect(state.isLoading).toBe(false);
      expect(state.error).toBeNull();
      expect(state.newAchievements).toEqual([]);
      expect(state.showAchievementModal).toBe(false);
    });
  });

  describe('fetchGamificationStatus', () => {
    it('fetches and stores gamification status', async () => {
      const mockStatus = createMockGamificationStatus();
      mockGetGamificationStatus.mockResolvedValueOnce(mockStatus);

      await act(async () => {
        await useGamificationStore.getState().fetchGamificationStatus();
      });

      expect(mockGetGamificationStatus).toHaveBeenCalledWith('test-token');
      expect(useGamificationStore.getState().status).toEqual(mockStatus);
      expect(useGamificationStore.getState().isLoading).toBe(false);
      expect(useGamificationStore.getState().error).toBeNull();
    });

    it('populates equippedCosmetics from status response', async () => {
      const mockEquipped = {
        boardTheme: {
          id: 1,
          name: 'Ocean Theme',
          type: 'BoardTheme' as const,
          assetUrl: '/themes/ocean.png',
          rarity: 'Rare' as const,
        },
        avatarFrame: null,
        badge: {
          id: 2,
          name: 'Gold Badge',
          type: 'Badge' as const,
          assetUrl: '/badges/gold.png',
          rarity: 'Epic' as const,
        },
      };
      const mockStatus = createMockGamificationStatus({
        equippedCosmetics: mockEquipped,
      });
      mockGetGamificationStatus.mockResolvedValueOnce(mockStatus);

      await act(async () => {
        await useGamificationStore.getState().fetchGamificationStatus();
      });

      expect(useGamificationStore.getState().equippedCosmetics).toEqual(mockEquipped);
    });

    it('sets loading state during fetch', async () => {
      let resolvePromise: (value: GamificationStatus) => void;
      const pendingPromise = new Promise<GamificationStatus>((resolve) => {
        resolvePromise = resolve;
      });
      mockGetGamificationStatus.mockReturnValueOnce(pendingPromise);

      const fetchPromise = act(async () => {
        await useGamificationStore.getState().fetchGamificationStatus();
      });

      expect(useGamificationStore.getState().isLoading).toBe(true);

      resolvePromise!(createMockGamificationStatus());
      await fetchPromise;

      expect(useGamificationStore.getState().isLoading).toBe(false);
    });

    it('handles fetch error', async () => {
      mockGetGamificationStatus.mockRejectedValueOnce(new Error('Network error'));

      await act(async () => {
        await useGamificationStore.getState().fetchGamificationStatus();
      });

      expect(useGamificationStore.getState().status).toBeNull();
      expect(useGamificationStore.getState().error).toBe('Network error');
      expect(useGamificationStore.getState().isLoading).toBe(false);
    });
  });

  describe('fetchChallenges', () => {
    it('fetches and stores challenges', async () => {
      const mockChallenges: Challenge[] = [
        createMockChallenge({ id: 1, type: 'Daily' }),
        createMockChallenge({ id: 2, type: 'Weekly' }),
      ];
      mockGetChallenges.mockResolvedValueOnce(mockChallenges);

      await act(async () => {
        await useGamificationStore.getState().fetchChallenges();
      });

      expect(mockGetChallenges).toHaveBeenCalledWith('test-token', undefined);
      expect(useGamificationStore.getState().challenges).toEqual(mockChallenges);
    });

    it('fetches challenges with type filter', async () => {
      const mockChallenges: Challenge[] = [
        createMockChallenge({ id: 1, type: 'Daily' }),
      ];
      mockGetChallenges.mockResolvedValueOnce(mockChallenges);

      await act(async () => {
        await useGamificationStore.getState().fetchChallenges('Daily');
      });

      expect(mockGetChallenges).toHaveBeenCalledWith('test-token', 'Daily');
    });
  });

  describe('fetchAchievements', () => {
    it('fetches and stores achievements', async () => {
      const mockAchievementsResult: AchievementsResult = {
        achievements: [
          {
            id: 1,
            name: 'First Steps',
            description: 'Play your first game',
            category: 'Games',
            iconUrl: '/icons/first-steps.png',
            isUnlocked: true,
            progress: 100,
            unlockedAt: '2026-01-01T00:00:00Z',
            isSecret: false,
          },
        ],
        totalUnlocked: 1,
        totalAchievements: 10,
      };
      mockGetAchievements.mockResolvedValueOnce(mockAchievementsResult);

      await act(async () => {
        await useGamificationStore.getState().fetchAchievements();
      });

      expect(mockGetAchievements).toHaveBeenCalledWith('test-token');
      expect(useGamificationStore.getState().achievements).toEqual(mockAchievementsResult.achievements);
    });
  });

  describe('fetchSeasonPass', () => {
    it('fetches and stores season pass', async () => {
      const mockSeasonPassResult: SeasonPassResult = {
        hasActiveSeason: true,
        seasonPass: {
          seasonId: 1,
          seasonName: 'Winter Season',
          seasonNumber: 1,
          currentXP: 500,
          currentTier: 5,
          endsAt: '2026-03-01T00:00:00Z',
          rewards: [],
        },
      };
      mockGetSeasonPass.mockResolvedValueOnce(mockSeasonPassResult);

      await act(async () => {
        await useGamificationStore.getState().fetchSeasonPass();
      });

      expect(mockGetSeasonPass).toHaveBeenCalledWith('test-token');
      expect(useGamificationStore.getState().seasonPass).toEqual(mockSeasonPassResult.seasonPass);
    });
  });

  describe('fetchCosmetics', () => {
    it('fetches and stores cosmetics', async () => {
      const mockCosmeticsResult: CosmeticsResult = {
        cosmetics: [
          {
            id: 1,
            name: 'Classic Theme',
            description: 'The classic board theme',
            type: 'BoardTheme',
            rarity: 'Common',
            assetUrl: '/themes/classic.png',
            previewUrl: '/themes/classic-preview.png',
            isOwned: true,
            isEquipped: true,
            isDefault: true,
          },
        ],
      };
      mockGetCosmetics.mockResolvedValueOnce(mockCosmeticsResult);

      await act(async () => {
        await useGamificationStore.getState().fetchCosmetics();
      });

      expect(mockGetCosmetics).toHaveBeenCalledWith('test-token', undefined);
      expect(useGamificationStore.getState().cosmetics).toEqual(mockCosmeticsResult.cosmetics);
    });

    it('fetches cosmetics with type filter', async () => {
      mockGetCosmetics.mockResolvedValueOnce({ cosmetics: [] });

      await act(async () => {
        await useGamificationStore.getState().fetchCosmetics('BoardTheme');
      });

      expect(mockGetCosmetics).toHaveBeenCalledWith('test-token', 'BoardTheme');
    });
  });

  describe('fetchPersonalGoals', () => {
    it('fetches and stores personal goals', async () => {
      const mockGoals: PersonalGoal[] = [
        {
          id: 1,
          type: 'BeatAverage',
          description: 'Beat your average score',
          targetValue: 1000,
          currentValue: 500,
          progressPercentage: 50,
          isCompleted: false,
          expiresAt: null,
        },
      ];
      mockGetPersonalGoals.mockResolvedValueOnce(mockGoals);

      await act(async () => {
        await useGamificationStore.getState().fetchPersonalGoals();
      });

      expect(mockGetPersonalGoals).toHaveBeenCalledWith('test-token');
      expect(useGamificationStore.getState().personalGoals).toEqual(mockGoals);
    });
  });

  describe('useStreakFreeze', () => {
    it('calls API and returns success', async () => {
      mockUseStreakFreeze.mockResolvedValueOnce({ success: true });

      let result: boolean;
      await act(async () => {
        result = await useGamificationStore.getState().useStreakFreeze();
      });

      expect(mockUseStreakFreeze).toHaveBeenCalledWith('test-token');
      expect(result!).toBe(true);
    });

    it('returns false on failure', async () => {
      mockUseStreakFreeze.mockResolvedValueOnce({ success: false, errorMessage: 'No tokens available' });

      let result: boolean;
      await act(async () => {
        result = await useGamificationStore.getState().useStreakFreeze();
      });

      expect(result!).toBe(false);
    });

    it('returns false on error', async () => {
      mockUseStreakFreeze.mockRejectedValueOnce(new Error('Network error'));

      let result: boolean;
      await act(async () => {
        result = await useGamificationStore.getState().useStreakFreeze();
      });

      expect(result!).toBe(false);
      expect(useGamificationStore.getState().error).toBe('Network error');
    });
  });

  describe('equipCosmetic', () => {
    it('calls API and updates equipped state on success', async () => {
      mockEquipCosmetic.mockResolvedValueOnce({ success: true });

      let result: boolean;
      await act(async () => {
        result = await useGamificationStore.getState().equipCosmetic('cosmetic-1');
      });

      expect(mockEquipCosmetic).toHaveBeenCalledWith('test-token', 'cosmetic-1');
      expect(result!).toBe(true);
    });

    it('returns false on failure', async () => {
      mockEquipCosmetic.mockResolvedValueOnce({ success: false, errorMessage: 'Not owned' });

      let result: boolean;
      await act(async () => {
        result = await useGamificationStore.getState().equipCosmetic('cosmetic-1');
      });

      expect(result!).toBe(false);
    });
  });

  describe('claimReward', () => {
    it('calls API and returns success', async () => {
      mockClaimSeasonReward.mockResolvedValueOnce({ 
        success: true, 
        rewardType: 'Cosmetic', 
        rewardValue: 1 
      });

      let result: boolean;
      await act(async () => {
        result = await useGamificationStore.getState().claimReward('reward-1');
      });

      expect(mockClaimSeasonReward).toHaveBeenCalledWith('test-token', 'reward-1');
      expect(result!).toBe(true);
    });

    it('returns false on failure', async () => {
      mockClaimSeasonReward.mockResolvedValueOnce({ success: false, errorMessage: 'Not eligible' });

      let result: boolean;
      await act(async () => {
        result = await useGamificationStore.getState().claimReward('reward-1');
      });

      expect(result!).toBe(false);
    });
  });

  describe('setTimezone', () => {
    it('calls API and returns success', async () => {
      mockSetTimezone.mockResolvedValueOnce({ success: true });

      let result: boolean;
      await act(async () => {
        result = await useGamificationStore.getState().setTimezone('America/New_York');
      });

      expect(mockSetTimezone).toHaveBeenCalledWith('test-token', 'America/New_York');
      expect(result!).toBe(true);
    });

    it('returns false on failure', async () => {
      mockSetTimezone.mockResolvedValueOnce({ success: false, errorMessage: 'Invalid timezone' });

      let result: boolean;
      await act(async () => {
        result = await useGamificationStore.getState().setTimezone('Invalid/Zone');
      });

      expect(result!).toBe(false);
    });
  });

  describe('processGameEndGamification', () => {
    it('queues new achievements for display', () => {
      const newAchievement = createMockAchievementUnlock();
      const gameEndResult: GameEndGamification = {
        streak: createMockStreakInfo(),
        challengeUpdates: [],
        newAchievements: [newAchievement],
        seasonProgress: {
          xpEarned: 100,
          totalXP: 500,
          newTier: 5,
          tierUp: false,
          newRewardsCount: 0,
        },
        goalUpdates: [],
      };

      act(() => {
        useGamificationStore.getState().processGameEndGamification(gameEndResult);
      });

      expect(useGamificationStore.getState().newAchievements).toEqual([newAchievement]);
      expect(useGamificationStore.getState().showAchievementModal).toBe(true);
    });

    it('updates streak info', () => {
      const updatedStreak = createMockStreakInfo({ currentStreak: 10 });
      const gameEndResult: GameEndGamification = {
        streak: updatedStreak,
        challengeUpdates: [],
        newAchievements: [],
        seasonProgress: {
          xpEarned: 100,
          totalXP: 500,
          newTier: 5,
          tierUp: false,
          newRewardsCount: 0,
        },
        goalUpdates: [],
      };

      // First set up initial status
      act(() => {
        useGamificationStore.setState({
          status: createMockGamificationStatus(),
        });
      });

      act(() => {
        useGamificationStore.getState().processGameEndGamification(gameEndResult);
      });

      expect(useGamificationStore.getState().status?.streak).toEqual(updatedStreak);
    });

    it('creates status when missing using game end result', () => {
      act(() => {
        useGamificationStore.setState({
          status: null,
          challenges: [],
          achievements: [],
          seasonPass: null,
          equippedCosmetics: null,
          personalGoals: [],
        });
      });

      const gameEndResult: GameEndGamification = {
        streak: createMockStreakInfo({ currentStreak: 3, longestStreak: 4, freezeTokens: 0 }),
        challengeUpdates: [],
        newAchievements: [],
        seasonProgress: {
          xpEarned: 50,
          totalXP: 150,
          newTier: 1,
          tierUp: false,
          newRewardsCount: 0,
        },
        goalUpdates: [],
        gamesPlayed: 2,
        highScore: 1234,
      } as any;

      act(() => {
        useGamificationStore.getState().processGameEndGamification(gameEndResult);
      });

      const status = useGamificationStore.getState().status;
      expect(status).not.toBeNull();
      expect(status?.streak.currentStreak).toBe(3);
      expect(status?.streak.longestStreak).toBe(4);
      expect(status?.equippedCosmetics).not.toBeNull();
    });

    it('updates challenge progress from challengeUpdates', () => {
      // Set up initial challenges
      const initialChallenges: Challenge[] = [
        createMockChallenge({ id: 1, currentProgress: 1, isCompleted: false }),
        createMockChallenge({ id: 2, currentProgress: 0, isCompleted: false }),
      ];

      act(() => {
        useGamificationStore.setState({
          challenges: initialChallenges,
          status: createMockGamificationStatus(),
        });
      });

      const gameEndResult: GameEndGamification = {
        streak: createMockStreakInfo(),
        challengeUpdates: [
          { challengeId: 1, challengeName: 'Daily Challenge', newProgress: 3, justCompleted: true, xpEarned: 100 },
          { challengeId: 2, challengeName: 'Weekly Challenge', newProgress: 2, justCompleted: false, xpEarned: null },
        ],
        newAchievements: [],
        seasonProgress: {
          xpEarned: 100,
          totalXP: 500,
          newTier: 5,
          tierUp: false,
          newRewardsCount: 0,
        },
        goalUpdates: [],
      };

      act(() => {
        useGamificationStore.getState().processGameEndGamification(gameEndResult);
      });

      const updatedChallenges = useGamificationStore.getState().challenges;
      expect(updatedChallenges[0].currentProgress).toBe(3);
      expect(updatedChallenges[0].isCompleted).toBe(true);
      expect(updatedChallenges[1].currentProgress).toBe(2);
      expect(updatedChallenges[1].isCompleted).toBe(false);
    });

    it('updates season pass XP and tier from seasonProgress', () => {
      // Set up initial season pass
      act(() => {
        useGamificationStore.setState({
          seasonPass: {
            seasonId: 1,
            seasonName: 'Winter Season',
            seasonNumber: 1,
            currentXP: 200,
            currentTier: 2,
            endsAt: '2026-03-01T00:00:00Z',
            rewards: [],
          },
          status: createMockGamificationStatus(),
        });
      });

      const gameEndResult: GameEndGamification = {
        streak: createMockStreakInfo(),
        challengeUpdates: [],
        newAchievements: [],
        seasonProgress: {
          xpEarned: 150,
          totalXP: 500,
          newTier: 5,
          tierUp: true,
          newRewardsCount: 3,
        },
        goalUpdates: [],
      };

      act(() => {
        useGamificationStore.getState().processGameEndGamification(gameEndResult);
      });

      const updatedSeasonPass = useGamificationStore.getState().seasonPass;
      expect(updatedSeasonPass?.currentXP).toBe(500);
      expect(updatedSeasonPass?.currentTier).toBe(5);
    });

    it('updates personal goals from goalUpdates', () => {
      // Set up initial goals
      const initialGoals: PersonalGoal[] = [
        {
          id: 1,
          type: 'BeatAverage',
          description: 'Beat your average score',
          targetValue: 1000,
          currentValue: 500,
          progressPercentage: 50,
          isCompleted: false,
          expiresAt: null,
        },
        {
          id: 2,
          type: 'NewPersonalBest',
          description: 'Set a new personal best',
          targetValue: 1,
          currentValue: 0,
          progressPercentage: 0,
          isCompleted: false,
          expiresAt: null,
        },
      ];

      act(() => {
        useGamificationStore.setState({
          personalGoals: initialGoals,
          status: createMockGamificationStatus(),
        });
      });

      const gameEndResult: GameEndGamification = {
        streak: createMockStreakInfo(),
        challengeUpdates: [],
        newAchievements: [],
        seasonProgress: {
          xpEarned: 100,
          totalXP: 500,
          newTier: 5,
          tierUp: false,
          newRewardsCount: 0,
        },
        goalUpdates: [
          { goalId: 1, description: 'Beat your average score', newProgress: 800, justCompleted: false },
          { goalId: 2, description: 'Set a new personal best', newProgress: 1, justCompleted: true },
        ],
      };

      act(() => {
        useGamificationStore.getState().processGameEndGamification(gameEndResult);
      });

      const updatedGoals = useGamificationStore.getState().personalGoals;
      expect(updatedGoals[0].currentValue).toBe(800);
      expect(updatedGoals[0].isCompleted).toBe(false);
      expect(updatedGoals[1].currentValue).toBe(1);
      expect(updatedGoals[1].isCompleted).toBe(true);
    });

    it('preserves existing achievements when adding new ones', () => {
      const existingAchievement = createMockAchievementUnlock({ achievementId: 1, name: 'First Achievement' });
      const newAchievement = createMockAchievementUnlock({ achievementId: 2, name: 'Second Achievement' });

      act(() => {
        useGamificationStore.setState({
          newAchievements: [existingAchievement],
          showAchievementModal: true,
          status: createMockGamificationStatus(),
        });
      });

      const gameEndResult: GameEndGamification = {
        streak: createMockStreakInfo(),
        challengeUpdates: [],
        newAchievements: [newAchievement],
        seasonProgress: {
          xpEarned: 100,
          totalXP: 500,
          newTier: 5,
          tierUp: false,
          newRewardsCount: 0,
        },
        goalUpdates: [],
      };

      act(() => {
        useGamificationStore.getState().processGameEndGamification(gameEndResult);
      });

      expect(useGamificationStore.getState().newAchievements).toEqual([existingAchievement, newAchievement]);
    });

    it('keeps modal open when already showing and no new achievements', () => {
      act(() => {
        useGamificationStore.setState({
          newAchievements: [createMockAchievementUnlock()],
          showAchievementModal: true,
          status: createMockGamificationStatus(),
        });
      });

      const gameEndResult: GameEndGamification = {
        streak: createMockStreakInfo(),
        challengeUpdates: [],
        newAchievements: [],
        seasonProgress: {
          xpEarned: 100,
          totalXP: 500,
          newTier: 5,
          tierUp: false,
          newRewardsCount: 0,
        },
        goalUpdates: [],
      };

      act(() => {
        useGamificationStore.getState().processGameEndGamification(gameEndResult);
      });

      expect(useGamificationStore.getState().showAchievementModal).toBe(true);
    });
  });

  describe('Achievement Modal', () => {
    it('showNextAchievement returns and removes first achievement from queue', () => {
      const achievement1 = createMockAchievementUnlock({ achievementId: 1 });
      const achievement2 = createMockAchievementUnlock({ achievementId: 2 });

      act(() => {
        useGamificationStore.setState({
          newAchievements: [achievement1, achievement2],
          showAchievementModal: true,
        });
      });

      let nextAchievement: AchievementUnlock | null;
      act(() => {
        nextAchievement = useGamificationStore.getState().showNextAchievement();
      });

      expect(nextAchievement!).toEqual(achievement1);
      expect(useGamificationStore.getState().newAchievements).toEqual([achievement2]);
    });

    it('showNextAchievement returns null when queue is empty', () => {
      let nextAchievement: AchievementUnlock | null;
      act(() => {
        nextAchievement = useGamificationStore.getState().showNextAchievement();
      });

      expect(nextAchievement!).toBeNull();
    });

    it('dismissAchievement hides modal when queue is empty', () => {
      const achievement = createMockAchievementUnlock();

      act(() => {
        useGamificationStore.setState({
          newAchievements: [achievement],
          showAchievementModal: true,
        });
      });

      // Show and remove the achievement
      act(() => {
        useGamificationStore.getState().showNextAchievement();
      });

      // Dismiss should hide modal
      act(() => {
        useGamificationStore.getState().dismissAchievement();
      });

      expect(useGamificationStore.getState().showAchievementModal).toBe(false);
    });

    it('dismissAchievement keeps modal open when queue has more', () => {
      const achievement1 = createMockAchievementUnlock({ achievementId: 1 });
      const achievement2 = createMockAchievementUnlock({ achievementId: 2 });

      act(() => {
        useGamificationStore.setState({
          newAchievements: [achievement1, achievement2],
          showAchievementModal: true,
        });
      });

      // Show and remove the first achievement
      act(() => {
        useGamificationStore.getState().showNextAchievement();
      });

      // Dismiss should keep modal open
      act(() => {
        useGamificationStore.getState().dismissAchievement();
      });

      expect(useGamificationStore.getState().showAchievementModal).toBe(true);
    });
  });

  describe('reset', () => {
    it('resets all state to initial values', async () => {
      // Set up some state
      mockGetGamificationStatus.mockResolvedValueOnce(createMockGamificationStatus());
      await act(async () => {
        await useGamificationStore.getState().fetchGamificationStatus();
      });

      // Reset
      act(() => {
        useGamificationStore.getState().reset();
      });

      const state = useGamificationStore.getState();
      expect(state.status).toBeNull();
      expect(state.challenges).toEqual([]);
      expect(state.achievements).toEqual([]);
      expect(state.isLoading).toBe(false);
      expect(state.error).toBeNull();
    });
  });

  describe('Selectors', () => {
    it('useStreak selector returns streak info', async () => {
      const mockStatus = createMockGamificationStatus({
        streak: createMockStreakInfo({ currentStreak: 7 }),
      });
      mockGetGamificationStatus.mockResolvedValueOnce(mockStatus);

      await act(async () => {
        await useGamificationStore.getState().fetchGamificationStatus();
      });

      // Import and test selector
      const { useStreak } = await import('@/lib/stores/gamification-store');
      const { result } = renderHook(() => useStreak());
      expect(result.current?.currentStreak).toBe(7);
    });
  });
});
