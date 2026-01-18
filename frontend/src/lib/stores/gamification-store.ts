/**
 * Gamification Store
 *
 * Zustand store for managing gamification state including challenges,
 * achievements, season pass, cosmetics, and personal goals.
 */

import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type {
  GamificationStatus,
  Challenge,
  ChallengeType,
  Achievement,
  SeasonPassInfo,
  Cosmetic,
  CosmeticType,
  EquippedCosmetics,
  PersonalGoal,
  AchievementUnlock,
  GameEndGamification,
} from '@/types/gamification';
import * as gamificationClient from '@/lib/api/gamification-client';
import { useAuthStore } from './auth-store';

/**
 * Gamification store state interface
 */
export interface GamificationState {
  // Data
  status: GamificationStatus | null;
  challenges: Challenge[];
  achievements: Achievement[];
  seasonPass: SeasonPassInfo | null;
  cosmetics: Cosmetic[];
  equippedCosmetics: EquippedCosmetics | null;
  personalGoals: PersonalGoal[];

  // UI State
  isLoading: boolean;
  error: string | null;
  newAchievements: AchievementUnlock[];
  showAchievementModal: boolean;
}

/**
 * Gamification store actions interface
 */
export interface GamificationActions {
  // Fetch actions
  fetchGamificationStatus: () => Promise<void>;
  fetchChallenges: (type?: ChallengeType) => Promise<void>;
  fetchAchievements: () => Promise<void>;
  fetchSeasonPass: () => Promise<void>;
  fetchCosmetics: (type?: CosmeticType) => Promise<void>;
  fetchPersonalGoals: () => Promise<void>;

  // Mutation actions
  useStreakFreeze: () => Promise<boolean>;
  equipCosmetic: (cosmeticId: string) => Promise<boolean>;
  claimReward: (rewardId: string) => Promise<boolean>;
  setTimezone: (timezone: string) => Promise<boolean>;

  // Game end integration
  processGameEndGamification: (result: GameEndGamification) => void;

  // Achievement modal
  showNextAchievement: () => AchievementUnlock | null;
  dismissAchievement: () => void;

  // Reset
  reset: () => void;
}

/**
 * Combined gamification store type
 */
export type GamificationStore = GamificationState & GamificationActions;

/**
 * Initial gamification state
 */
const initialState: GamificationState = {
  status: null,
  challenges: [],
  achievements: [],
  seasonPass: null,
  cosmetics: [],
  equippedCosmetics: null,
  personalGoals: [],
  isLoading: false,
  error: null,
  newAchievements: [],
  showAchievementModal: false,
};

const fallbackEquipped: EquippedCosmetics = {
  boardTheme: null,
  avatarFrame: null,
  badge: null,
};

/**
 * Get authentication token from auth store
 */
function getToken(): string | null {
  return useAuthStore.getState().token;
}

/**
 * Zustand store for gamification state management
 * Uses persist middleware to store equipped cosmetics in localStorage
 */
export const useGamificationStore = create<GamificationStore>()(
  persist(
    (set, get) => ({
      ...initialState,

      /**
       * Fetch complete gamification status
       */
      fetchGamificationStatus: async () => {
        const token = getToken();
        if (!token) return;

        set({ isLoading: true, error: null });

        try {
          const status = await gamificationClient.getGamificationStatus(token);
          set({
            status,
            equippedCosmetics: status.equippedCosmetics,
            isLoading: false,
            error: null,
          });
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Failed to fetch gamification status';
          set({ error: message, isLoading: false });
        }
      },

      /**
       * Fetch challenges with optional type filter
       */
      fetchChallenges: async (type?: ChallengeType) => {
        const token = getToken();
        if (!token) return;

        set({ isLoading: true, error: null });

        try {
          const challenges = await gamificationClient.getChallenges(token, type);
          set({
            challenges,
            isLoading: false,
            error: null,
          });
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Failed to fetch challenges';
          set({ error: message, isLoading: false });
        }
      },

      /**
       * Fetch achievements
       */
      fetchAchievements: async () => {
        const token = getToken();
        if (!token) return;

        set({ isLoading: true, error: null });

        try {
          const result = await gamificationClient.getAchievements(token);
          set({
            achievements: result.achievements,
            isLoading: false,
            error: null,
          });
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Failed to fetch achievements';
          set({ error: message, isLoading: false });
        }
      },

      /**
       * Fetch season pass
       */
      fetchSeasonPass: async () => {
        const token = getToken();
        if (!token) return;

        set({ isLoading: true, error: null });

        try {
          const result = await gamificationClient.getSeasonPass(token);
          set({
            seasonPass: result.seasonPass,
            isLoading: false,
            error: null,
          });
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Failed to fetch season pass';
          set({ error: message, isLoading: false });
        }
      },

      /**
       * Fetch cosmetics with optional type filter
       */
      fetchCosmetics: async (type?: CosmeticType) => {
        const token = getToken();
        if (!token) return;

        set({ isLoading: true, error: null });

        try {
          const result = await gamificationClient.getCosmetics(token, type);
          set({
            cosmetics: result.cosmetics,
            isLoading: false,
            error: null,
          });
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Failed to fetch cosmetics';
          set({ error: message, isLoading: false });
        }
      },

      /**
       * Fetch personal goals
       */
      fetchPersonalGoals: async () => {
        const token = getToken();
        if (!token) return;

        set({ isLoading: true, error: null });

        try {
          const goals = await gamificationClient.getPersonalGoals(token);
          set({
            personalGoals: goals,
            isLoading: false,
            error: null,
          });
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Failed to fetch personal goals';
          set({ error: message, isLoading: false });
        }
      },

      /**
       * Use a streak freeze token
       */
      useStreakFreeze: async () => {
        const token = getToken();
        if (!token) return false;

        try {
          const result = await gamificationClient.useStreakFreeze(token);
          if (result.success) {
            // Refresh status to get updated freeze token count
            get().fetchGamificationStatus();
          }
          return result.success;
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Failed to use streak freeze';
          set({ error: message });
          return false;
        }
      },

      /**
       * Equip a cosmetic item
       */
      equipCosmetic: async (cosmeticId: string) => {
        const token = getToken();
        if (!token) return false;

        try {
          const result = await gamificationClient.equipCosmetic(token, cosmeticId);
          if (result.success) {
            // Refresh cosmetics to get updated equipped state
            get().fetchCosmetics();
          }
          return result.success;
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Failed to equip cosmetic';
          set({ error: message });
          return false;
        }
      },

      /**
       * Claim a season pass reward
       */
      claimReward: async (rewardId: string) => {
        const token = getToken();
        if (!token) return false;

        try {
          const result = await gamificationClient.claimSeasonReward(token, rewardId);
          if (result.success) {
            // Refresh season pass to get updated rewards
            get().fetchSeasonPass();
          }
          return result.success;
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Failed to claim reward';
          set({ error: message });
          return false;
        }
      },

      /**
       * Set user's timezone
       */
      setTimezone: async (timezone: string) => {
        const token = getToken();
        if (!token) return false;

        try {
          const result = await gamificationClient.setTimezone(token, timezone);
          return result.success;
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Failed to set timezone';
          set({ error: message });
          return false;
        }
      },

      /**
       * Process gamification data from game end
       * Updates state and queues achievements for display
       */
      processGameEndGamification: (result: GameEndGamification) => {
        const authState = useAuthStore.getState();
        if (authState.user) {
          authState.setUser({
            ...authState.user,
            gamesPlayed: result.gamesPlayed,
            highScore: result.highScore,
          });
        }

        set((state) => {
          const newAchievementIds = new Set(result.newAchievements.map((a) => a.achievementId));
          const unlockedAt = new Date().toISOString();

          const defaultStatus = state.status ?? {
            streak: result.streak,
            activeChallenges: state.challenges,
            recentAchievements: state.achievements,
            seasonPass: state.seasonPass,
            equippedCosmetics: state.equippedCosmetics ?? fallbackEquipped,
            activeGoals: state.personalGoals,
          };

          const mergedStreak = defaultStatus.streak
            ? {
                ...defaultStatus.streak,
                currentStreak: result.streak.currentStreak,
                longestStreak: result.streak.longestStreak,
                freezeTokens: result.streak.freezeTokens ?? defaultStatus.streak.freezeTokens,
                isAtRisk: result.streak.isAtRisk ?? defaultStatus.streak.isAtRisk,
                resetTime: result.streak.resetTime ?? defaultStatus.streak.resetTime,
              }
            : result.streak;

          // Update challenges with progress
          const baseChallenges = state.challenges.length > 0
            ? state.challenges
            : defaultStatus.activeChallenges;

          const updatedChallenges = baseChallenges.map((challenge) => {
            const update = result.challengeUpdates.find((u) => u.challengeId === challenge.id);
            if (update) {
              return {
                ...challenge,
                currentProgress: update.newProgress,
                isCompleted: update.justCompleted || challenge.isCompleted,
              };
            }
            return challenge;
          });

          // Update achievements with unlocks
          const updatedAchievements = state.achievements.map((achievement) =>
            newAchievementIds.has(achievement.id)
              ? {
                  ...achievement,
                  isUnlocked: true,
                  progress: 100,
                  unlockedAt: achievement.unlockedAt ?? unlockedAt,
                }
              : achievement
          );

          const newlyUnlockedAchievements = updatedAchievements.filter((a) => newAchievementIds.has(a.id));
          const recentAchievements = newlyUnlockedAchievements.length > 0
            ? [
                ...newlyUnlockedAchievements,
                ...defaultStatus.recentAchievements.filter((a) => !newAchievementIds.has(a.id)),
              ]
            : defaultStatus.recentAchievements;

          // Update season pass XP
          const updatedSeasonPass = state.seasonPass ? {
            ...state.seasonPass,
            currentXP: result.seasonProgress.totalXP,
            currentTier: result.seasonProgress.newTier,
          } : null;

          // Update personal goals
          const baseGoals = state.personalGoals.length > 0
            ? state.personalGoals
            : defaultStatus.activeGoals;

          const updatedGoals = baseGoals.map((goal) => {
            const update = result.goalUpdates.find((u) => u.goalId === goal.id);
            if (update) {
              return {
                ...goal,
                currentValue: update.newProgress,
                isCompleted: update.justCompleted || goal.isCompleted,
              };
            }
            return goal;
          });

          // Update status - merge to preserve fields that may be null from API
          const newStatus = {
            ...defaultStatus,
            streak: mergedStreak,
            activeChallenges: updatedChallenges,
            recentAchievements,
            seasonPass: updatedSeasonPass ?? defaultStatus.seasonPass,
            activeGoals: updatedGoals,
          };

          return {
            status: newStatus,
            challenges: updatedChallenges,
            achievements: updatedAchievements,
            seasonPass: updatedSeasonPass,
            personalGoals: updatedGoals,
            newAchievements: [...state.newAchievements, ...result.newAchievements],
            showAchievementModal: result.newAchievements.length > 0 || state.showAchievementModal,
          };
        });
      },

      /**
       * Get the next achievement to display
       * Returns and removes the first achievement from the queue
       */
      showNextAchievement: () => {
        const { newAchievements } = get();
        
        if (newAchievements.length === 0) {
          return null;
        }

        const [next, ...rest] = newAchievements;
        set({ newAchievements: rest });
        return next;
      },

      /**
       * Dismiss the current achievement modal
       * Hides modal if queue is empty, keeps open otherwise
       */
      dismissAchievement: () => {
        const { newAchievements } = get();
        
        if (newAchievements.length === 0) {
          set({ showAchievementModal: false });
        }
        // If there are more achievements, keep modal open
      },

      /**
       * Reset all gamification state
       */
      reset: () => {
        set(initialState);
      },
    }),
    {
      name: 'perfectfit-gamification',
      storage: createJSONStorage(() => localStorage),
      // Only persist equipped cosmetics
      partialize: (state) => ({
        equippedCosmetics: state.equippedCosmetics,
      }),
    }
  )
);

/**
 * Selector hooks for specific parts of gamification state
 */
export const useStreak = () => useGamificationStore((state) => state.status?.streak);
export const useChallengesData = () => useGamificationStore((state) => state.challenges);
export const useAchievementsData = () => useGamificationStore((state) => state.achievements);
export const useSeasonPassData = () => useGamificationStore((state) => state.seasonPass);
export const useCosmeticsData = () => useGamificationStore((state) => state.cosmetics);
export const useEquippedCosmetics = () => useGamificationStore((state) => state.equippedCosmetics);
export const usePersonalGoalsData = () => useGamificationStore((state) => state.personalGoals);
export const useGamificationLoading = () => useGamificationStore((state) => state.isLoading);
export const useGamificationError = () => useGamificationStore((state) => state.error);
export const useNewAchievements = () => useGamificationStore((state) => state.newAchievements);
export const useShowAchievementModal = () => useGamificationStore((state) => state.showAchievementModal);
