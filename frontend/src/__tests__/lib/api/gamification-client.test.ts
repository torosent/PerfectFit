/**
 * Gamification API Client Tests
 *
 * Tests for the gamification API client functions that interact with
 * backend endpoints for challenges, achievements, season pass, cosmetics, etc.
 */

import {
  getGamificationStatus,
  getChallenges,
  getAchievements,
  getSeasonPass,
  getCosmetics,
  getPersonalGoals,
  useStreakFreeze,
  equipCosmetic,
  claimSeasonReward,
  setTimezone,
  getUserTimezone,
  GamificationApiError,
} from '@/lib/api/gamification-client';
import { API_BASE_URL } from '@/lib/api';
import type {
  GamificationStatus,
  Challenge,
  AchievementsResult,
  SeasonPassResult,
  CosmeticsResult,
  PersonalGoal,
  ApiResult,
  ClaimRewardResult,
} from '@/types/gamification';

// Mock fetch globally
const mockFetch = jest.fn();
global.fetch = mockFetch;

// Mock localStorage
const mockLocalStorage: { [key: string]: string } = {};
beforeAll(() => {
  Object.defineProperty(window, 'localStorage', {
    value: {
      getItem: jest.fn((key: string) => mockLocalStorage[key] || null),
      setItem: jest.fn((key: string, value: string) => {
        mockLocalStorage[key] = value;
      }),
      removeItem: jest.fn((key: string) => {
        delete mockLocalStorage[key];
      }),
      clear: jest.fn(() => {
        Object.keys(mockLocalStorage).forEach((key) => delete mockLocalStorage[key]);
      }),
    },
    writable: true,
  });
});

describe('Gamification API Client', () => {
  const testToken = 'test-jwt-token';

  beforeEach(() => {
    mockFetch.mockClear();
    mockLocalStorage['token'] = testToken;
  });

  afterEach(() => {
    delete mockLocalStorage['token'];
  });

  describe('getGamificationStatus', () => {
    it('should fetch gamification status with auth header', async () => {
      const mockStatus: GamificationStatus = {
        streak: {
          currentStreak: 5,
          longestStreak: 10,
          freezeTokens: 2,
          isAtRisk: false,
          resetTime: '2026-01-10T00:00:00Z',
        },
        activeChallenges: [],
        recentAchievements: [],
        seasonPass: {
          seasonId: 1,
          seasonName: 'Season 1',
          seasonNumber: 1,
          currentXP: 500,
          currentTier: 5,
          endsAt: '2026-02-01T00:00:00Z',
          rewards: [],
        },
        equippedCosmetics: {
          boardTheme: null,
          avatarFrame: null,
          badge: null,
        },
        activeGoals: [],
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockStatus),
      });

      const result = await getGamificationStatus(testToken);

      expect(mockFetch).toHaveBeenCalledTimes(1);
      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification`,
        expect.objectContaining({
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
            Authorization: `Bearer ${testToken}`,
          }),
        })
      );
      expect(result).toEqual(mockStatus);
    });

    it('should throw GamificationApiError on HTTP error', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: () => Promise.resolve({ message: 'Unauthorized' }),
      });

      await expect(getGamificationStatus(testToken)).rejects.toThrow(
        GamificationApiError
      );
    });
  });

  describe('getChallenges', () => {
    const mockChallenges: Challenge[] = [
      {
        id: 1,
        name: 'Play 5 Games',
        description: 'Play 5 games today',
        type: 'Daily',
        targetValue: 5,
        currentProgress: 2,
        xpReward: 100,
        isCompleted: false,
        endsAt: '2026-01-10T00:00:00Z',
      },
    ];

    it('should fetch all challenges without filter', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockChallenges),
      });

      const result = await getChallenges(testToken);

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/challenges`,
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: `Bearer ${testToken}`,
          }),
        })
      );
      expect(result).toEqual(mockChallenges);
    });

    it('should fetch challenges with type filter', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockChallenges),
      });

      await getChallenges(testToken, 'Daily');

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/challenges?type=Daily`,
        expect.any(Object)
      );
    });

    it('should fetch weekly challenges with type filter', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve([]),
      });

      await getChallenges(testToken, 'Weekly');

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/challenges?type=Weekly`,
        expect.any(Object)
      );
    });
  });

  describe('getAchievements', () => {
    it('should fetch achievements result', async () => {
      const mockAchievements: AchievementsResult = {
        achievements: [
          {
            id: 1,
            name: 'First Win',
            description: 'Win your first game',
            category: 'Games',
            iconUrl: '/icons/first-win.png',
            isUnlocked: true,
            progress: 100,
            unlockedAt: '2026-01-01T00:00:00Z',
            isSecret: false,
          },
        ],
        totalUnlocked: 1,
        totalAchievements: 10,
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockAchievements),
      });

      const result = await getAchievements(testToken);

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/achievements`,
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: `Bearer ${testToken}`,
          }),
        })
      );
      expect(result).toEqual(mockAchievements);
      expect(result.totalUnlocked).toBe(1);
      expect(result.totalAchievements).toBe(10);
    });
  });

  describe('getSeasonPass', () => {
    it('should fetch season pass data', async () => {
      const mockSeasonPassResult: SeasonPassResult = {
        seasonPass: {
          seasonId: 1,
          seasonName: 'Winter Wonderland',
          seasonNumber: 1,
          currentXP: 1500,
          currentTier: 15,
          endsAt: '2026-03-01T00:00:00Z',
          rewards: [
            {
              id: 1,
              tier: 1,
              rewardType: 'Cosmetic',
              rewardValue: 1,
              xpRequired: 100,
              isClaimed: true,
              canClaim: false,
            },
          ],
        },
        hasActiveSeason: true,
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockSeasonPassResult),
      });

      const result = await getSeasonPass(testToken);

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/season-pass`,
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: `Bearer ${testToken}`,
          }),
        })
      );
      expect(result).toEqual(mockSeasonPassResult);
      expect(result.seasonPass?.seasonName).toBe('Winter Wonderland');
      expect(result.hasActiveSeason).toBe(true);
    });
  });

  describe('getCosmetics', () => {
    const mockCosmetics: CosmeticsResult = {
      cosmetics: [
        {
          id: 1,
          name: 'Classic Board',
          description: 'The classic game board',
          type: 'BoardTheme',
          rarity: 'Common',
          assetUrl: '/assets/boards/classic.png',
          previewUrl: '/previews/boards/classic.png',
          isOwned: true,
          isEquipped: true,
          isDefault: true,
        },
      ],
    };

    it('should fetch all cosmetics without filter', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockCosmetics),
      });

      const result = await getCosmetics(testToken);

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/cosmetics`,
        expect.any(Object)
      );
      expect(result).toEqual(mockCosmetics);
    });

    it('should fetch cosmetics with type filter', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockCosmetics),
      });

      await getCosmetics(testToken, 'BoardTheme');

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/cosmetics?type=BoardTheme`,
        expect.any(Object)
      );
    });

    it('should fetch avatar frames with type filter', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({ cosmetics: [] }),
      });

      await getCosmetics(testToken, 'AvatarFrame');

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/cosmetics?type=AvatarFrame`,
        expect.any(Object)
      );
    });

    it('should fetch badges with type filter', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({ cosmetics: [] }),
      });

      await getCosmetics(testToken, 'Badge');

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/cosmetics?type=Badge`,
        expect.any(Object)
      );
    });
  });

  describe('getPersonalGoals', () => {
    it('should fetch personal goals', async () => {
      const mockGoals: PersonalGoal[] = [
        {
          id: 1,
          type: 'BeatAverage',
          description: 'Beat your average score',
          targetValue: 1000,
          currentValue: 800,
          progressPercentage: 80,
          isCompleted: false,
          expiresAt: '2026-01-10T00:00:00Z',
        },
      ];

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockGoals),
      });

      const result = await getPersonalGoals(testToken);

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/goals`,
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: `Bearer ${testToken}`,
          }),
        })
      );
      expect(result).toEqual(mockGoals);
    });
  });

  describe('useStreakFreeze', () => {
    it('should make POST request to streak-freeze endpoint', async () => {
      const mockResult: ApiResult = {
        success: true,
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResult),
      });

      const result = await useStreakFreeze(testToken);

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/streak-freeze`,
        expect.objectContaining({
          method: 'POST',
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
            Authorization: `Bearer ${testToken}`,
          }),
        })
      );
      expect(result.success).toBe(true);
    });

    it('should return error when no freeze tokens available', async () => {
      const mockResult: ApiResult = {
        success: false,
        errorMessage: 'No freeze tokens available',
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResult),
      });

      const result = await useStreakFreeze(testToken);

      expect(result.success).toBe(false);
      expect(result.errorMessage).toBe('No freeze tokens available');
    });
  });

  describe('equipCosmetic', () => {
    it('should send POST request with correct body', async () => {
      const mockResult: ApiResult = {
        success: true,
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResult),
      });

      const cosmeticId = '00000000-0000-0000-0000-000000000001';
      const result = await equipCosmetic(testToken, cosmeticId);

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/equip`,
        expect.objectContaining({
          method: 'POST',
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
            Authorization: `Bearer ${testToken}`,
          }),
          body: JSON.stringify({ cosmeticId }),
        })
      );
      expect(result.success).toBe(true);
    });

    it('should return error when cosmetic not owned', async () => {
      const mockResult: ApiResult = {
        success: false,
        errorMessage: 'Cosmetic not owned',
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResult),
      });

      const result = await equipCosmetic(testToken, '00000000-0000-0000-0000-000000000002');

      expect(result.success).toBe(false);
      expect(result.errorMessage).toBe('Cosmetic not owned');
    });
  });

  describe('claimSeasonReward', () => {
    it('should send POST request with correct body', async () => {
      const mockResult: ClaimRewardResult = {
        success: true,
        rewardType: 'Cosmetic',
        rewardValue: 42,
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResult),
      });

      const rewardId = '00000000-0000-0000-0000-000000000010';
      const result = await claimSeasonReward(testToken, rewardId);

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/claim-reward`,
        expect.objectContaining({
          method: 'POST',
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
            Authorization: `Bearer ${testToken}`,
          }),
          body: JSON.stringify({ rewardId }),
        })
      );
      expect(result.success).toBe(true);
      expect(result.rewardType).toBe('Cosmetic');
      expect(result.rewardValue).toBe(42);
    });

    it('should return error when reward not available', async () => {
      const mockResult: ClaimRewardResult = {
        success: false,
        errorMessage: 'Reward not available or already claimed',
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResult),
      });

      const result = await claimSeasonReward(testToken, '00000000-0000-0000-0000-000000000099');

      expect(result.success).toBe(false);
      expect(result.errorMessage).toBe('Reward not available or already claimed');
    });
  });

  describe('setTimezone', () => {
    it('should send POST request with correct body', async () => {
      const mockResult: ApiResult = {
        success: true,
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResult),
      });

      const timezone = 'America/New_York';
      const result = await setTimezone(testToken, timezone);

      expect(mockFetch).toHaveBeenCalledWith(
        `${API_BASE_URL}/api/gamification/timezone`,
        expect.objectContaining({
          method: 'POST',
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
            Authorization: `Bearer ${testToken}`,
          }),
          body: JSON.stringify({ timezone }),
        })
      );
      expect(result.success).toBe(true);
    });
  });

  describe('getUserTimezone', () => {
    it('should return a valid timezone string', () => {
      const timezone = getUserTimezone();

      expect(typeof timezone).toBe('string');
      expect(timezone.length).toBeGreaterThan(0);
      // Should be a valid IANA timezone like "America/New_York" or "UTC"
      expect(timezone).toMatch(/^[A-Za-z_]+\/[A-Za-z_]+$|^UTC$/);
    });
  });

  describe('error handling', () => {
    it('should throw GamificationApiError with status code on HTTP error', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 404,
        json: () => Promise.resolve({ message: 'Not found' }),
      });

      try {
        await getGamificationStatus(testToken);
        fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeInstanceOf(GamificationApiError);
        const apiError = error as InstanceType<typeof GamificationApiError>;
        expect(apiError.statusCode).toBe(404);
        expect(apiError.message).toBe('Not found');
      }
    });

    it('should handle non-JSON error response', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: () => Promise.reject(new Error('Not JSON')),
      });

      try {
        await getGamificationStatus(testToken);
        fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeInstanceOf(GamificationApiError);
        const apiError = error as InstanceType<typeof GamificationApiError>;
        expect(apiError.statusCode).toBe(500);
      }
    });

    it('should throw GamificationApiError on network failure', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Network error'));

      await expect(getGamificationStatus(testToken)).rejects.toThrow();
    });
  });
});
