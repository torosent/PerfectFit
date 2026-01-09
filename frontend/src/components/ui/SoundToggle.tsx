'use client';

import { memo } from 'react';
import { motion } from 'motion/react';
import { useSound } from '@/contexts/SoundContext';

export interface SoundToggleProps {
  /** Additional CSS classes */
  className?: string;
  /** Show label text */
  showLabel?: boolean;
}

/**
 * Sound toggle button for muting/unmuting game sounds
 */
function SoundToggleComponent({ className = '', showLabel = false }: SoundToggleProps) {
  const { isMuted, toggleMute, markUserInteraction } = useSound();

  const handleClick = () => {
    markUserInteraction();
    toggleMute();
  };

  return (
    <motion.button
      onClick={handleClick}
      whileHover={{ scale: 1.05 }}
      whileTap={{ scale: 0.95 }}
      className={`
        flex items-center gap-2 px-3 py-2 rounded-lg
        transition-colors duration-200
        ${className}
      `}
      style={{
        backgroundColor: 'rgba(255, 255, 255, 0.1)',
        borderWidth: 1,
        borderStyle: 'solid',
        borderColor: 'rgba(255, 255, 255, 0.2)',
      }}
      aria-label={isMuted ? 'Unmute sounds' : 'Mute sounds'}
      title={isMuted ? 'Sound: Off' : 'Sound: On'}
    >
      <motion.span
        key={isMuted ? 'muted' : 'unmuted'}
        initial={{ scale: 0.5, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        transition={{ duration: 0.2 }}
        className="text-lg"
      >
        {isMuted ? '🔇' : '🔊'}
      </motion.span>
      
      {showLabel && (
        <span className="text-sm font-medium text-white">
          {isMuted ? 'Off' : 'On'}
        </span>
      )}
    </motion.button>
  );
}

export const SoundToggle = memo(SoundToggleComponent);
SoundToggle.displayName = 'SoundToggle';
