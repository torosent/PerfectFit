'use client';

import { useState } from 'react';
import { motion } from 'motion/react';
import { AchievementsTable } from './AchievementsTable';
import { ChallengeTemplatesTable } from './ChallengeTemplatesTable';
import { CosmeticsTable } from './CosmeticsTable';

export interface GamificationTabProps {
  refreshTrigger?: number;
}

type SubTab = 'achievements' | 'challenge-templates' | 'cosmetics';

const SUB_TABS: { key: SubTab; label: string }[] = [
  { key: 'achievements', label: 'Achievements' },
  { key: 'challenge-templates', label: 'Challenge Templates' },
  { key: 'cosmetics', label: 'Cosmetics' },
];

/**
 * GamificationTab - Container component with sub-tabs for managing gamification entities.
 * 
 * Sub-tabs:
 * - Achievements: CRUD for achievements
 * - Challenge Templates: CRUD for challenge templates
 * - Cosmetics: CRUD for cosmetic items
 */
export function GamificationTab({ refreshTrigger }: GamificationTabProps) {
  const [activeSubTab, setActiveSubTab] = useState<SubTab>('achievements');

  return (
    <div className="space-y-6">
      {/* Sub-tab navigation */}
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

      {/* Sub-tab content */}
      <motion.div
        key={activeSubTab}
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.2 }}
      >
        {activeSubTab === 'achievements' && (
          <AchievementsTable refreshTrigger={refreshTrigger} />
        )}
        {activeSubTab === 'challenge-templates' && (
          <ChallengeTemplatesTable refreshTrigger={refreshTrigger} />
        )}
        {activeSubTab === 'cosmetics' && (
          <CosmeticsTable refreshTrigger={refreshTrigger} />
        )}
      </motion.div>
    </div>
  );
}
