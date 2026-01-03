'use client';

import { AVATAR_EMOJIS } from '@/lib/emojis';

export interface EmojiPickerProps {
  /** Currently selected emoji (or null if none selected) */
  selected: string | null;
  /** Callback when an emoji is selected */
  onSelect: (emoji: string) => void;
}

/**
 * Reusable emoji picker component that displays a grid of avatar emojis.
 * Shows all emojis from the curated AVATAR_EMOJIS list.
 */
export function EmojiPicker({ selected, onSelect }: EmojiPickerProps) {
  return (
    <div 
      role="group" 
      aria-label="Select an avatar emoji"
      className="grid grid-cols-8 sm:grid-cols-10 gap-1.5 p-2 max-h-48 overflow-y-auto rounded-lg"
      style={{ backgroundColor: 'rgba(10, 37, 64, 0.5)' }}
    >
      {AVATAR_EMOJIS.map((emoji) => (
        <button
          key={emoji}
          type="button"
          onClick={() => onSelect(emoji)}
          aria-label={emoji}
          aria-pressed={selected === emoji}
          className={`
            text-xl sm:text-2xl p-1.5 rounded-lg transition-all
            hover:scale-110 active:scale-95
            focus:outline-none focus:ring-2 focus:ring-teal-500/50
            ${selected === emoji 
              ? 'bg-teal-500/40 ring-2 ring-teal-400' 
              : 'hover:bg-white/10'
            }
          `}
        >
          {emoji}
        </button>
      ))}
    </div>
  );
}
