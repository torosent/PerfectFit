'use client';

import { memo, useState, useMemo, useCallback, useId } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import Image from 'next/image';
import type { Cosmetic, CosmeticType, CosmeticRarity } from '@/types/gamification';

export interface CosmeticSelectorProps {
  /** All cosmetics */
  cosmetics: Cosmetic[];
  /** Currently equipped cosmetic IDs by type */
  equippedIds: Record<CosmeticType, number | null>;
  /** Callback when cosmetic is selected for equipping */
  onEquip: (cosmeticId: number) => void;
  /** Whether data is loading */
  isLoading?: boolean;
}

const COSMETIC_TYPES: { key: CosmeticType; label: string; icon: string }[] = [
  { key: 'BoardTheme', label: 'Board Themes', icon: '🎨' },
  { key: 'AvatarFrame', label: 'Avatar Frames', icon: '🖼️' },
  { key: 'Badge', label: 'Badges', icon: '🏅' },
];

/**
 * Get border color based on rarity
 */
function getRarityBorder(rarity: CosmeticRarity): string {
  switch (rarity) {
    case 'Legendary': return 'border-yellow-500 shadow-yellow-500/30';
    case 'Epic': return 'border-purple-500 shadow-purple-500/30';
    case 'Rare': return 'border-blue-500 shadow-blue-500/30';
    case 'Common':
    default: return 'border-gray-500';
  }
}

/**
 * Get rarity label color
 */
function getRarityColor(rarity: CosmeticRarity): string {
  switch (rarity) {
    case 'Legendary': return 'text-yellow-400';
    case 'Epic': return 'text-purple-400';
    case 'Rare': return 'text-blue-400';
    case 'Common':
    default: return 'text-gray-400';
  }
}

/**
 * Cosmetic selection grid with tabs
 */
function CosmeticSelectorComponent({
  cosmetics,
  equippedIds,
  onEquip,
  isLoading = false,
}: CosmeticSelectorProps) {
  const [activeType, setActiveType] = useState<CosmeticType>('BoardTheme');
  const uniqueId = useId();
  
  const filteredCosmetics = useMemo(() => {
    return cosmetics.filter(c => c.type === activeType);
  }, [cosmetics, activeType]);
  
  // Sort: owned first, then by rarity
  const sortedCosmetics = useMemo(() => {
    const rarityOrder: Record<CosmeticRarity, number> = {
      Legendary: 0,
      Epic: 1,
      Rare: 2,
      Common: 3,
    };
    
    return [...filteredCosmetics].sort((a, b) => {
      if (a.isOwned && !b.isOwned) return -1;
      if (!a.isOwned && b.isOwned) return 1;
      return rarityOrder[a.rarity] - rarityOrder[b.rarity];
    });
  }, [filteredCosmetics]);
  
  const handleTypeClick = useCallback((type: CosmeticType) => {
    setActiveType(type);
  }, []);
  
  // Generate tab and panel IDs for accessibility
  const getTabId = (key: string) => `${uniqueId}-tab-${key}`;
  const getPanelId = (key: string) => `${uniqueId}-panel-${key}`;
  
  return (
    <div className="rounded-xl bg-white/5 backdrop-blur-md overflow-hidden">
      {/* Header */}
      <div className="p-4 border-b border-white/10">
        <h3 className="text-lg font-bold text-white">Cosmetics</h3>
      </div>
      
      {/* Type tabs */}
      <div role="tablist" aria-label="Cosmetic types" className="flex border-b border-white/10">
        {COSMETIC_TYPES.map(type => (
          <button
            key={type.key}
            id={getTabId(type.key)}
            role="tab"
            aria-selected={activeType === type.key}
            aria-controls={getPanelId(type.key)}
            aria-label={type.label}
            onClick={() => handleTypeClick(type.key)}
            className={`flex-1 py-3 px-4 text-sm font-medium transition-colors ${
              activeType === type.key
                ? 'text-cyan-400 bg-cyan-500/10 border-b-2 border-cyan-400'
                : 'text-gray-400 hover:text-white hover:bg-white/5'
            }`}
          >
            <span className="mr-1">{type.icon}</span>
            <span className="hidden sm:inline">{type.label}</span>
          </button>
        ))}
      </div>
      
      {/* Cosmetics grid */}
      <div
        id={getPanelId(activeType)}
        role="tabpanel"
        aria-labelledby={getTabId(activeType)}
        className="p-4"
      >
        {/* Loading state */}
        {isLoading && (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3">
            {[...Array(8)].map((_, i) => (
              <div key={i} className="aspect-square rounded-xl bg-white/5 animate-pulse" />
            ))}
          </div>
        )}
        
        {/* Empty state */}
        {!isLoading && sortedCosmetics.length === 0 && (
          <div className="py-8 text-center">
            <span className="text-4xl mb-3 block">✨</span>
            <p className="text-gray-400">No cosmetics available</p>
          </div>
        )}
        
        {/* Cosmetics grid */}
        {!isLoading && sortedCosmetics.length > 0 && (
          <AnimatePresence mode="wait">
            <motion.div
              key={activeType}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3"
            >
              {sortedCosmetics.map((cosmetic, index) => {
                const isEquipped = equippedIds[cosmetic.type] === cosmetic.id;
                
                return (
                  <CosmeticCard
                    key={cosmetic.id}
                    cosmetic={cosmetic}
                    isEquipped={isEquipped}
                    onEquip={() => onEquip(cosmetic.id)}
                    delay={index * 0.03}
                  />
                );
              })}
            </motion.div>
          </AnimatePresence>
        )}
      </div>
    </div>
  );
}

/**
 * Individual cosmetic card
 */
function CosmeticCard({
  cosmetic,
  isEquipped,
  onEquip,
  delay = 0,
}: {
  cosmetic: Cosmetic;
  isEquipped: boolean;
  onEquip: () => void;
  delay?: number;
}) {
  const { name, rarity, previewUrl, isOwned } = cosmetic;
  const rarityBorder = getRarityBorder(rarity);
  const rarityColor = getRarityColor(rarity);
  
  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.9 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ delay }}
      className={`group relative rounded-xl border-2 overflow-hidden transition-all ${rarityBorder} ${
        !isOwned ? 'opacity-60' : ''
      } ${isEquipped ? 'ring-2 ring-cyan-400 ring-offset-2 ring-offset-slate-900' : ''}`}
    >
      {/* Preview image */}
      <div className="relative aspect-square bg-white/5 flex items-center justify-center p-2">
        {previewUrl ? (
          <Image
            src={previewUrl}
            alt={name}
            fill
            sizes="(max-width: 768px) 30vw, 220px"
            className={`object-contain ${!isOwned ? 'grayscale' : ''}`}
          />
        ) : (
          <span className="text-3xl">✨</span>
        )}
        
        {/* Lock overlay for unowned */}
        {!isOwned && (
          <div className="absolute inset-0 flex items-center justify-center bg-black/50">
            <span className="text-2xl">🔒</span>
          </div>
        )}
        
        {/* Equipped badge */}
        {isEquipped && (
          <div className="absolute top-2 right-2 w-6 h-6 rounded-full bg-cyan-500 flex items-center justify-center">
            <span className="text-xs text-white">✓</span>
          </div>
        )}
      </div>
      
      {/* Info */}
      <div className="p-2 bg-black/30">
        <p className="text-sm font-medium text-white truncate">{name}</p>
        <p className={`text-xs ${rarityColor}`}>{rarity}</p>
      </div>
      
      {/* Equip button (only for owned items) */}
      {isOwned && !isEquipped && (
        <motion.button
          onClick={onEquip}
          className="absolute inset-0 bg-black/50 opacity-0 hover:opacity-100 group-focus-within:opacity-100 focus:opacity-100 focus:ring-2 focus:ring-primary focus:outline-none flex items-center justify-center transition-opacity"
          whileHover={{ opacity: 1 }}
        >
          <span className="px-3 py-1 rounded-full bg-cyan-500 text-white text-sm font-medium">
            Equip
          </span>
        </motion.button>
      )}
    </motion.div>
  );
}

export const CosmeticSelector = memo(CosmeticSelectorComponent);
CosmeticSelector.displayName = 'CosmeticSelector';
