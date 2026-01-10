/**
 * Gamification Hooks Tests
 *
 * Tests for the custom React hooks that provide gamification functionality.
 */

import { renderHook, act, waitFor } from '@testing-library/react';
import type {
  GamificationStatus,
  Challenge,
  Achievement,
  SeasonPassInfo,
  Cosmetic,
  PersonalGoal,
  StreakInfo,
} from '@/types/gamification';

// Mock the stores
const mockGamificationState = {
  status: null as GamificationStatus | null,
  challenges: [] as Challenge[],
  achievements: [] as Achievement[],
  seasonPass: null as SeasonPassInfo | null,
  cosmetics: [] as Cosmetic[],
  equippedCosmetics: null,
  personalGoals: [] as PersonalGoal[],
  isLoading: false,
  error: null as string | null,
  fetchGamificationStatus: jest.fn(),
  fetchChallenges: jest.fn(),
  fetchAchievements: jest.fn(),
  fetchSeasonPass: jest.fn(),
  fetchCosmetics: jest.fn(),
  fetchPersonalGoals: jest.fn(),
  useStreakFreeze: jest.fn(),
  equipCosmetic: jest.fn(),
  claimReward: jest.fn(),
};

const mockAuthState = {
  isAuthenticated: true,
};

jest.mock('@/lib/stores/gamification-store', () => ({
  useGamificationStore: (selector: (state: typeof mockGamificationState) => unknown) => 
    selector(mockGamificationState),
}));

jest.mock('@/lib/stores/auth-store', () => ({
  useAuthStore: (selector: (state: typeof mockAuthState) => unknown) => 
    selector(mockAuthState),
}));

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

const createMockAchievement = (overrides?: Partial<Achievement>): Achievement => ({
  id: 1,
  name: 'First Steps',
  description: 'Play your first game',
  category: 'Games',
  iconUrl: '/icons/first-steps.png',
  isUnlocked: false,
  progress: 0,
  unlockedAt: null,
  isSecret: false,
  ...overrides,
});

const createMockSeasonPassInfo = (overrides?: Partial<SeasonPassInfo>): SeasonPassInfo => ({
  seasonId: 1,
  seasonName: 'Winter Season',
  seasonNumber: 1,
  currentXP: 500,
  currentTier: 5,
  endsAt: '2026-03-01T00:00:00Z',
  rewards: [],
  ...overrides,
});

const createMockCosmetic = (overrides?: Partial<Cosmetic>): Cosmetic => ({
  id: 1,
  name: 'Classic Theme',
  description: 'The classic board theme',
  type: 'BoardTheme',
  rarity: 'Common',
  assetUrl: '/themes/classic.png',
  previewUrl: '/themes/classic-preview.png',
  isOwned: true,
  isEquipped: false,
  isDefault: true,
  ...overrides,
});

const createMockPersonalGoal = (overrides?: Partial<PersonalGoal>): PersonalGoal => ({
  id: 1,
  type: 'BeatAverage',
  description: 'Beat your average score',
  targetValue: 1000,
  currentValue: 500,
  progressPercentage: 50,
  isCompleted: false,
  expiresAt: null,
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

// Import hooks after mocking
import { useGamification } from '@/hooks/useGamification';
import { useStreaks } from '@/hooks/useStreaks';
import { useChallenges } from '@/hooks/useChallenges';
import { useAchievements } from '@/hooks/useAchievements';
import { useSeasonPass } from '@/hooks/useSeasonPass';
import { useCosmetics } from '@/hooks/useCosmetics';
import { usePersonalGoals } from '@/hooks/usePersonalGoals';

describe('Gamification Hooks', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Reset mock state
    mockGamificationState.status = null;
    mockGamificationState.challenges = [];
    mockGamificationState.achievements = [];
    mockGamificationState.seasonPass = null;
    mockGamificationState.cosmetics = [];
    mockGamificationState.personalGoals = [];
    mockGamificationState.isLoading = false;
    mockGamificationState.error = null;
    mockAuthState.isAuthenticated = true;
  });

  describe('useGamification', () => {
    it('fetches gamification status when authenticated', () => {
      mockAuthState.isAuthenticated = true;
      mockGamificationState.status = null;

      renderHook(() => useGamification());

      expect(mockGamificationState.fetchGamificationStatus).toHaveBeenCalled();
    });

    it('does not fetch when already has status', () => {
      mockAuthState.isAuthenticated = true;
      mockGamificationState.status = createMockGamificationStatus();

      renderHook(() => useGamification());

      expect(mockGamificationState.fetchGamificationStatus).not.toHaveBeenCalled();
    });

    it('does not fetch when not authenticated', () => {
      mockAuthState.isAuthenticated = false;
      mockGamificationState.status = null;

      renderHook(() => useGamification());

      expect(mockGamificationState.fetchGamificationStatus).not.toHaveBeenCalled();
    });

    it('returns status, isLoading, and error', () => {
      mockGamificationState.status = createMockGamificationStatus();
      mockGamificationState.isLoading = true;
      mockGamificationState.error = 'Test error';

      const { result } = renderHook(() => useGamification());

      expect(result.current.status).toEqual(mockGamificationState.status);
      expect(result.current.isLoading).toBe(true);
      expect(result.current.error).toBe('Test error');
    });
  });

  describe('useStreaks', () => {
    it('returns default values when no streak data', () => {
      mockGamificationState.status = null;

      const { result } = renderHook(() => useStreaks());

      expect(result.current.currentStreak).toBe(0);
      expect(result.current.longestStreak).toBe(0);
      expect(result.current.freezeTokens).toBe(0);
      expect(result.current.isAtRisk).toBe(false);
      expect(result.current.resetTime).toBeNull();
    });

    it('returns streak data when available', () => {
      mockGamificationState.status = createMockGamificationStatus({
        streak: createMockStreakInfo({
          currentStreak: 7,
          longestStreak: 15,
          freezeTokens: 3,
          isAtRisk: true,
          resetTime: '2026-01-10T00:00:00Z',
        }),
      });

      const { result } = renderHook(() => useStreaks());

      expect(result.current.currentStreak).toBe(7);
      expect(result.current.longestStreak).toBe(15);
      expect(result.current.freezeTokens).toBe(3);
      expect(result.current.isAtRisk).toBe(true);
      expect(result.current.resetTime).toBeInstanceOf(Date);
    });

    it('provides useFreeze function', () => {
      const { result } = renderHook(() => useStreaks());

      expect(typeof result.current.useFreeze).toBe('function');
    });
  });

  describe('useChallenges', () => {
    it('fetches challenges on mount', () => {
      renderHook(() => useChallenges());

      expect(mockGamificationState.fetchChallenges).toHaveBeenCalledWith(undefined);
    });

    it('fetches challenges with type filter', () => {
      renderHook(() => useChallenges('Daily'));

      expect(mockGamificationState.fetchChallenges).toHaveBeenCalledWith('Daily');
    });

    it('filters daily and weekly challenges', () => {
      mockGamificationState.challenges = [
        createMockChallenge({ id: 1, type: 'Daily' }),
        createMockChallenge({ id: 2, type: 'Daily' }),
        createMockChallenge({ id: 3, type: 'Weekly' }),
      ];

      const { result } = renderHook(() => useChallenges());

      expect(result.current.dailyChallenges).toHaveLength(2);
      expect(result.current.weeklyChallenges).toHaveLength(1);
    });

    it('counts completed challenges', () => {
      mockGamificationState.challenges = [
        createMockChallenge({ id: 1, isCompleted: true }),
        createMockChallenge({ id: 2, isCompleted: false }),
        createMockChallenge({ id: 3, isCompleted: true }),
      ];

      const { result } = renderHook(() => useChallenges());

      expect(result.current.completedCount).toBe(2);
      expect(result.current.totalCount).toBe(3);
    });
  });

  describe('useAchievements', () => {
    it('fetches achievements on mount', () => {
      renderHook(() => useAchievements());

      expect(mockGamificationState.fetchAchievements).toHaveBeenCalled();
    });

    it('counts unlocked achievements', () => {
      mockGamificationState.achievements = [
        createMockAchievement({ id: 1, isUnlocked: true }),
        createMockAchievement({ id: 2, isUnlocked: false }),
        createMockAchievement({ id: 3, isUnlocked: true }),
      ];

      const { result } = renderHook(() => useAchievements());

      expect(result.current.unlockedCount).toBe(2);
      expect(result.current.totalCount).toBe(3);
    });

    it('groups achievements by category', () => {
      mockGamificationState.achievements = [
        createMockAchievement({ id: 1, category: 'Score' }),
        createMockAchievement({ id: 2, category: 'Streak' }),
        createMockAchievement({ id: 3, category: 'Games' }),
        createMockAchievement({ id: 4, category: 'Score' }),
        createMockAchievement({ id: 5, category: 'Challenge' }),
        createMockAchievement({ id: 6, category: 'Special' }),
      ];

      const { result } = renderHook(() => useAchievements());

      expect(result.current.byCategory.score).toHaveLength(2);
      expect(result.current.byCategory.streak).toHaveLength(1);
      expect(result.current.byCategory.games).toHaveLength(1);
      expect(result.current.byCategory.challenge).toHaveLength(1);
      expect(result.current.byCategory.special).toHaveLength(1);
    });
  });

  describe('useSeasonPass', () => {
    it('fetches season pass on mount', () => {
      renderHook(() => useSeasonPass());

      expect(mockGamificationState.fetchSeasonPass).toHaveBeenCalled();
    });

    it('returns default values when no season pass', () => {
      mockGamificationState.seasonPass = null;

      const { result } = renderHook(() => useSeasonPass());

      expect(result.current.currentTier).toBe(0);
      expect(result.current.currentXP).toBe(0);
      expect(result.current.claimableRewards).toEqual([]);
    });

    it('returns season pass data when available', () => {
      mockGamificationState.seasonPass = createMockSeasonPassInfo({
        currentXP: 750,
        currentTier: 7,
      });

      const { result } = renderHook(() => useSeasonPass());

      expect(result.current.currentTier).toBe(7);
      expect(result.current.currentXP).toBe(750);
    });

    it('calculates claimable rewards', () => {
      mockGamificationState.seasonPass = createMockSeasonPassInfo({
        rewards: [
          { id: 1, tier: 1, rewardType: 'Cosmetic', rewardValue: 1, xpRequired: 100, isClaimed: true, canClaim: false },
          { id: 2, tier: 2, rewardType: 'XPBoost', rewardValue: 50, xpRequired: 200, isClaimed: false, canClaim: true },
          { id: 3, tier: 3, rewardType: 'Cosmetic', rewardValue: 2, xpRequired: 300, isClaimed: false, canClaim: false },
        ],
      });

      const { result } = renderHook(() => useSeasonPass());

      expect(result.current.claimableRewards).toHaveLength(1);
      expect(result.current.claimableRewards[0].id).toBe(2);
    });

    it('calculates progress to next tier', () => {
      mockGamificationState.seasonPass = createMockSeasonPassInfo({
        currentXP: 150,
        rewards: [
          { id: 1, tier: 1, rewardType: 'Cosmetic', rewardValue: 1, xpRequired: 100, isClaimed: true, canClaim: false },
          { id: 2, tier: 2, rewardType: 'XPBoost', rewardValue: 50, xpRequired: 200, isClaimed: false, canClaim: false },
        ],
      });

      const { result } = renderHook(() => useSeasonPass());

      // Progress = (150 / 200) * 100 = 75%
      expect(result.current.progressToNextTier).toBe(75);
    });
  });

  describe('useCosmetics', () => {
    it('fetches cosmetics on mount', () => {
      renderHook(() => useCosmetics());

      expect(mockGamificationState.fetchCosmetics).toHaveBeenCalledWith(undefined);
    });

    it('fetches cosmetics with type filter', () => {
      renderHook(() => useCosmetics('BoardTheme'));

      expect(mockGamificationState.fetchCosmetics).toHaveBeenCalledWith('BoardTheme');
    });

    it('filters owned cosmetics', () => {
      mockGamificationState.cosmetics = [
        createMockCosmetic({ id: 1, isOwned: true }),
        createMockCosmetic({ id: 2, isOwned: false }),
        createMockCosmetic({ id: 3, isOwned: true }),
      ];

      const { result } = renderHook(() => useCosmetics());

      expect(result.current.ownedCosmetics).toHaveLength(2);
    });

    it('groups cosmetics by type', () => {
      mockGamificationState.cosmetics = [
        createMockCosmetic({ id: 1, type: 'BoardTheme' }),
        createMockCosmetic({ id: 2, type: 'AvatarFrame' }),
        createMockCosmetic({ id: 3, type: 'Badge' }),
        createMockCosmetic({ id: 4, type: 'BoardTheme' }),
      ];

      const { result } = renderHook(() => useCosmetics());

      expect(result.current.byType.boardThemes).toHaveLength(2);
      expect(result.current.byType.avatarFrames).toHaveLength(1);
      expect(result.current.byType.badges).toHaveLength(1);
    });

    it('provides equipCosmetic function', () => {
      const { result } = renderHook(() => useCosmetics());

      expect(typeof result.current.equipCosmetic).toBe('function');
    });
  });

  describe('usePersonalGoals', () => {
    it('fetches personal goals on mount', () => {
      renderHook(() => usePersonalGoals());

      expect(mockGamificationState.fetchPersonalGoals).toHaveBeenCalled();
    });

    it('filters active and completed goals', () => {
      mockGamificationState.personalGoals = [
        createMockPersonalGoal({ id: 1, isCompleted: false }),
        createMockPersonalGoal({ id: 2, isCompleted: true }),
        createMockPersonalGoal({ id: 3, isCompleted: false }),
      ];

      const { result } = renderHook(() => usePersonalGoals());

      expect(result.current.activeGoals).toHaveLength(2);
      expect(result.current.completedGoals).toHaveLength(1);
    });
  });
});
