'use client';

import { useState, useCallback } from 'react';
import { motion } from 'motion/react';
import { AchievementsTable } from './AchievementsTable';
import { ChallengeTemplatesTable } from './ChallengeTemplatesTable';
import { CosmeticsTable } from './CosmeticsTable';
import {
  seedSampleData,
  resetAchievements,
  resetChallenges,
  resetCosmetics,
  type SeedDataResponse,
  type ResetResponse,
} from '@/lib/api/admin-gamification-client';
import { useToken } from '@/lib/stores/auth-store';

export interface GamificationTabProps {
  refreshTrigger?: number;
}

type SubTab = 'achievements' | 'challenge-templates' | 'cosmetics';

const SUB_TABS: { key: SubTab; label: string }[] = [
  { key: 'achievements', label: 'Achievements' },
  { key: 'challenge-templates', label: 'Challenge Templates' },
  { key: 'cosmetics', label: 'Cosmetics' },
];

type ResetType = 'achievements' | 'challenges' | 'cosmetics';

/**
 * GamificationTab - Container component with sub-tabs for managing gamification entities.
 * 
 * Sub-tabs:
 * - Achievements: CRUD for achievements
 * - Challenge Templates: CRUD for challenge templates
 * - Cosmetics: CRUD for cosmetic items
 */
export function GamificationTab({ refreshTrigger }: GamificationTabProps) {
  const token = useToken();
  const [activeSubTab, setActiveSubTab] = useState<SubTab>('achievements');
  const [isSeeding, setIsSeeding] = useState(false);
  const [seedResult, setSeedResult] = useState<SeedDataResponse | null>(null);
  const [seedError, setSeedError] = useState<string | null>(null);
  const [localRefreshTrigger, setLocalRefreshTrigger] = useState(0);
  
  // Reset state
  const [isResetting, setIsResetting] = useState<ResetType | null>(null);
  const [resetResult, setResetResult] = useState<ResetResponse | null>(null);
  const [resetError, setResetError] = useState<string | null>(null);
  const [confirmReset, setConfirmReset] = useState<ResetType | null>(null);

  const handleSeedData = useCallback(async () => {
    if (!token) return;

    setIsSeeding(true);
    setSeedResult(null);
    setSeedError(null);

    try {
      const result = await seedSampleData(token);
      setSeedResult(result);
      // Trigger refresh of all tables
      setLocalRefreshTrigger((prev) => prev + 1);
    } catch (err) {
      setSeedError(err instanceof Error ? err.message : 'Failed to seed sample data');
    } finally {
      setIsSeeding(false);
    }
  }, [token]);

  const handleReset = useCallback(async (type: ResetType) => {
    if (!token) return;

    setIsResetting(type);
    setResetResult(null);
    setResetError(null);
    setConfirmReset(null);

    try {
      let result: ResetResponse;
      switch (type) {
        case 'achievements':
          result = await resetAchievements(token);
          break;
        case 'challenges':
          result = await resetChallenges(token);
          break;
        case 'cosmetics':
          result = await resetCosmetics(token);
          break;
      }
      setResetResult(result);
      // Trigger refresh of all tables
      setLocalRefreshTrigger((prev) => prev + 1);
    } catch (err) {
      setResetError(err instanceof Error ? err.message : `Failed to reset ${type}`);
    } finally {
      setIsResetting(null);
    }
  }, [token]);

  const combinedRefreshTrigger = (refreshTrigger ?? 0) + localRefreshTrigger;

  return (
    <div className="space-y-6">
      {/* Header with Seed and Reset Buttons */}
      <div className="flex justify-between items-center flex-wrap gap-4">
        <div className="flex gap-1 p-1 rounded-lg" style={{ background: 'rgba(10, 37, 64, 0.5)' }}>
          {SUB_TABS.map((tab) => (
            <button
              key={tab.key}
              onClick={() => setActiveSubTab(tab.key)}
              className={`px-4 py-2 rounded-md text-sm font-medium transition-all ${
                activeSubTab === tab.key
                  ? 'text-white shadow-sm'
                  : 'text-gray-400 hover:text-white hover:bg-white/5'
              }`}
              style={
                activeSubTab === tab.key
                  ? { background: 'linear-gradient(135deg, rgba(20, 184, 166, 0.8), rgba(14, 165, 233, 0.8))' }
                  : undefined
              }
            >
              {tab.label}
            </button>
          ))}
        </div>

        <div className="flex gap-2 flex-wrap">
          <button
            onClick={handleSeedData}
            disabled={isSeeding || isResetting !== null}
            className="px-4 py-2 rounded-lg text-white text-sm font-medium transition-colors disabled:opacity-50 bg-amber-600 hover:bg-amber-700"
            title="Generate sample achievements, cosmetics, and challenge templates"
          >
            {isSeeding ? '🌱 Seeding...' : '🌱 Seed Sample Data'}
          </button>
          
          {/* Reset Buttons */}
          {confirmReset === 'achievements' ? (
            <div className="flex gap-1">
              <button
                onClick={() => handleReset('achievements')}
                disabled={isResetting !== null}
                className="px-3 py-2 rounded-lg text-white text-sm font-medium bg-red-600 hover:bg-red-700 disabled:opacity-50"
              >
                Confirm
              </button>
              <button
                onClick={() => setConfirmReset(null)}
                className="px-3 py-2 rounded-lg text-gray-300 text-sm font-medium hover:bg-white/10"
              >
                Cancel
              </button>
            </div>
          ) : (
            <button
              onClick={() => setConfirmReset('achievements')}
              disabled={isSeeding || isResetting !== null}
              className="px-3 py-2 rounded-lg text-red-400 text-sm font-medium transition-colors disabled:opacity-50 hover:bg-red-500/10 border border-red-500/30"
              title="Delete all achievements and user progress"
            >
              {isResetting === 'achievements' ? '...' : '🗑️ Reset Achievements'}
            </button>
          )}

          {confirmReset === 'challenges' ? (
            <div className="flex gap-1">
              <button
                onClick={() => handleReset('challenges')}
                disabled={isResetting !== null}
                className="px-3 py-2 rounded-lg text-white text-sm font-medium bg-red-600 hover:bg-red-700 disabled:opacity-50"
              >
                Confirm
              </button>
              <button
                onClick={() => setConfirmReset(null)}
                className="px-3 py-2 rounded-lg text-gray-300 text-sm font-medium hover:bg-white/10"
              >
                Cancel
              </button>
            </div>
          ) : (
            <button
              onClick={() => setConfirmReset('challenges')}
              disabled={isSeeding || isResetting !== null}
              className="px-3 py-2 rounded-lg text-red-400 text-sm font-medium transition-colors disabled:opacity-50 hover:bg-red-500/10 border border-red-500/30"
              title="Delete all challenges, templates, and user progress"
            >
              {isResetting === 'challenges' ? '...' : '🗑️ Reset Challenges'}
            </button>
          )}

          {confirmReset === 'cosmetics' ? (
            <div className="flex gap-1">
              <button
                onClick={() => handleReset('cosmetics')}
                disabled={isResetting !== null}
                className="px-3 py-2 rounded-lg text-white text-sm font-medium bg-red-600 hover:bg-red-700 disabled:opacity-50"
              >
                Confirm
              </button>
              <button
                onClick={() => setConfirmReset(null)}
                className="px-3 py-2 rounded-lg text-gray-300 text-sm font-medium hover:bg-white/10"
              >
                Cancel
              </button>
            </div>
          ) : (
            <button
              onClick={() => setConfirmReset('cosmetics')}
              disabled={isSeeding || isResetting !== null}
              className="px-3 py-2 rounded-lg text-red-400 text-sm font-medium transition-colors disabled:opacity-50 hover:bg-red-500/10 border border-red-500/30"
              title="Delete all cosmetics and user ownership"
            >
              {isResetting === 'cosmetics' ? '...' : '🗑️ Reset Cosmetics'}
            </button>
          )}
        </div>
      </div>

      {/* Reset Result */}
      {resetResult && (
        <div className="p-4 rounded-lg bg-orange-500/10 border border-orange-500/30 text-orange-400">
          <div className="font-medium mb-1">🗑️ {resetResult.message}</div>
          <div className="text-sm">
            Items deleted: {resetResult.itemsDeleted} | Related records: {resetResult.relatedRecordsDeleted}
          </div>
          <button onClick={() => setResetResult(null)} className="mt-2 text-sm underline">
            Dismiss
          </button>
        </div>
      )}

      {/* Reset Error */}
      {resetError && (
        <div className="p-4 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400">
          {resetError}
          <button onClick={() => setResetError(null)} className="ml-2 underline">
            Dismiss
          </button>
        </div>
      )}

      {/* Seed Result */}
      {seedResult && (
        <div className="p-4 rounded-lg bg-green-500/10 border border-green-500/30 text-green-400">
          <div className="font-medium mb-2">✅ {seedResult.message}</div>
          <div className="text-sm grid grid-cols-2 sm:grid-cols-4 gap-2">
            <div>
              <span className="font-medium">Achievements:</span> +{seedResult.achievementsAdded}
              {seedResult.achievementsSkipped > 0 && (
                <span className="text-gray-500"> ({seedResult.achievementsSkipped} existed)</span>
              )}
            </div>
            <div>
              <span className="font-medium">Cosmetics:</span> +{seedResult.cosmeticsAdded}
              {seedResult.cosmeticsSkipped > 0 && (
                <span className="text-gray-500"> ({seedResult.cosmeticsSkipped} existed)</span>
              )}
            </div>
            <div>
              <span className="font-medium">Templates:</span> +{seedResult.challengeTemplatesAdded}
              {seedResult.challengeTemplatesSkipped > 0 && (
                <span className="text-gray-500"> ({seedResult.challengeTemplatesSkipped} existed)</span>
              )}
            </div>
            {seedResult.activeChallengesCreated > 0 && (
              <div>
                <span className="font-medium">Active Challenges:</span> +{seedResult.activeChallengesCreated}
              </div>
            )}
            {seedResult.seasonCreated && (
              <div>
                <span className="font-medium">Season:</span> Created with {seedResult.seasonRewardsAdded} rewards
              </div>
            )}
          </div>
          <button onClick={() => setSeedResult(null)} className="mt-2 text-sm underline">
            Dismiss
          </button>
        </div>
      )}

      {/* Seed Error */}
      {seedError && (
        <div className="p-4 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400">
          {seedError}
          <button onClick={() => setSeedError(null)} className="ml-2 underline">
            Dismiss
          </button>
        </div>
      )}

      {/* Sub-tab content */}
      <motion.div
        key={activeSubTab}
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.2 }}
      >
        {activeSubTab === 'achievements' && (
          <AchievementsTable refreshTrigger={combinedRefreshTrigger} />
        )}
        {activeSubTab === 'challenge-templates' && (
          <ChallengeTemplatesTable refreshTrigger={combinedRefreshTrigger} />
        )}
        {activeSubTab === 'cosmetics' && (
          <CosmeticsTable refreshTrigger={combinedRefreshTrigger} />
        )}
      </motion.div>
    </div>
  );
}
