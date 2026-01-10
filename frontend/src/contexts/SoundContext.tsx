'use client';

import { createContext, useContext, useSyncExternalStore, useEffect, useCallback, useRef, useMemo, type ReactNode } from 'react';

/**
 * Sound types available in the game
 */
export type SoundType = 'place' | 'lineClear' | 'combo' | 'gameOver' | 'perfectClear' | 'highScore';

interface SoundContextValue {
  /** Whether sound is muted */
  isMuted: boolean;
  /** Toggle mute state */
  toggleMute: () => void;
  /** Set mute state directly */
  setMuted: (muted: boolean) => void;
  /** Play a sound effect */
  playSound: (type: SoundType) => void;
  /** Play combo sound with escalating pitch based on combo count */
  playComboSound: (comboCount: number) => void;
  /** Whether user has interacted (sounds can play) */
  hasUserInteracted: boolean;
  /** Mark user as having interacted */
  markUserInteraction: () => void;
}

const SoundContext = createContext<SoundContextValue | null>(null);

const SOUND_MUTE_KEY = 'perfectfit-sound-muted';
const USER_INTERACTED_KEY = 'perfectfit-user-interacted';
const LOCAL_STORAGE_EVENT = 'perfectfit-local-storage';

function subscribeSoundStorage(callback: () => void): () => void {
  if (typeof window === 'undefined') return () => {};

  const onStorage = (event: StorageEvent) => {
    if (event.key === SOUND_MUTE_KEY || event.key === USER_INTERACTED_KEY) callback();
  };

  const onLocal = (event: Event) => {
    const customEvent = event as CustomEvent<{ key?: string }>;
    if (customEvent.detail?.key === SOUND_MUTE_KEY || customEvent.detail?.key === USER_INTERACTED_KEY) callback();
  };

  window.addEventListener('storage', onStorage);
  window.addEventListener(LOCAL_STORAGE_EVENT, onLocal);

  return () => {
    window.removeEventListener('storage', onStorage);
    window.removeEventListener(LOCAL_STORAGE_EVENT, onLocal);
  };
}

function notifySoundStorageChange(key: string): void {
  if (typeof window === 'undefined') return;
  window.dispatchEvent(new CustomEvent(LOCAL_STORAGE_EVENT, { detail: { key } }));
}

function getMutedSnapshot(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem(SOUND_MUTE_KEY);
}

function getInteractedSnapshot(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem(USER_INTERACTED_KEY);
}

function getSoundServerSnapshot(): string | null {
  return null;
}

/**
 * Create an oscillator-based sound effect using Web Audio API
 */
function createOscillatorSound(
  audioContext: AudioContext,
  frequency: number,
  duration: number,
  type: OscillatorType = 'sine',
  volume: number = 0.3,
  attack: number = 0.01,
  decay: number = 0.1,
): void {
  const oscillator = audioContext.createOscillator();
  const gainNode = audioContext.createGain();
  
  oscillator.connect(gainNode);
  gainNode.connect(audioContext.destination);
  
  oscillator.type = type;
  oscillator.frequency.setValueAtTime(frequency, audioContext.currentTime);
  
  // Envelope: attack -> sustain -> decay
  gainNode.gain.setValueAtTime(0, audioContext.currentTime);
  gainNode.gain.linearRampToValueAtTime(volume, audioContext.currentTime + attack);
  gainNode.gain.linearRampToValueAtTime(volume * 0.7, audioContext.currentTime + attack + duration * 0.3);
  gainNode.gain.linearRampToValueAtTime(0, audioContext.currentTime + attack + duration);
  
  oscillator.start(audioContext.currentTime);
  oscillator.stop(audioContext.currentTime + attack + duration + decay);
}

/**
 * Play a sequence of notes
 */
function playNoteSequence(
  audioContext: AudioContext,
  notes: Array<{ frequency: number; duration: number; delay: number; type?: OscillatorType; volume?: number }>,
): void {
  notes.forEach(({ frequency, duration, delay, type = 'sine', volume = 0.2 }) => {
    setTimeout(() => {
      createOscillatorSound(audioContext, frequency, duration, type, volume);
    }, delay * 1000);
  });
}

/**
 * Sound effect definitions
 */
const soundEffects = {
  place: (ctx: AudioContext) => {
    // Short satisfying thunk
    createOscillatorSound(ctx, 220, 0.08, 'square', 0.15);
    createOscillatorSound(ctx, 330, 0.05, 'sine', 0.1);
  },
  
  lineClear: (ctx: AudioContext) => {
    // Satisfying ascending chime
    playNoteSequence(ctx, [
      { frequency: 523, duration: 0.1, delay: 0, type: 'sine', volume: 0.2 },     // C5
      { frequency: 659, duration: 0.1, delay: 0.05, type: 'sine', volume: 0.2 },  // E5
      { frequency: 784, duration: 0.15, delay: 0.1, type: 'sine', volume: 0.25 }, // G5
      { frequency: 1047, duration: 0.2, delay: 0.15, type: 'sine', volume: 0.2 }, // C6
    ]);
  },
  
  combo: (ctx: AudioContext, comboCount: number) => {
    // Escalating pitch based on combo
    const baseFrequency = 440 + (comboCount * 50);
    playNoteSequence(ctx, [
      { frequency: baseFrequency, duration: 0.08, delay: 0, type: 'triangle', volume: 0.2 },
      { frequency: baseFrequency * 1.25, duration: 0.08, delay: 0.05, type: 'triangle', volume: 0.2 },
      { frequency: baseFrequency * 1.5, duration: 0.12, delay: 0.1, type: 'triangle', volume: 0.25 },
    ]);
  },
  
  gameOver: (ctx: AudioContext) => {
    // Descending sad melody
    playNoteSequence(ctx, [
      { frequency: 440, duration: 0.2, delay: 0, type: 'sine', volume: 0.2 },
      { frequency: 392, duration: 0.2, delay: 0.2, type: 'sine', volume: 0.18 },
      { frequency: 349, duration: 0.2, delay: 0.4, type: 'sine', volume: 0.16 },
      { frequency: 330, duration: 0.4, delay: 0.6, type: 'sine', volume: 0.15 },
    ]);
  },
  
  perfectClear: (ctx: AudioContext) => {
    // Epic fanfare
    playNoteSequence(ctx, [
      { frequency: 523, duration: 0.15, delay: 0, type: 'square', volume: 0.15 },
      { frequency: 659, duration: 0.15, delay: 0.1, type: 'square', volume: 0.15 },
      { frequency: 784, duration: 0.15, delay: 0.2, type: 'square', volume: 0.15 },
      { frequency: 1047, duration: 0.3, delay: 0.3, type: 'sine', volume: 0.25 },
      { frequency: 1319, duration: 0.4, delay: 0.5, type: 'sine', volume: 0.2 },
    ]);
  },
  
  highScore: (ctx: AudioContext) => {
    // Celebratory jingle
    playNoteSequence(ctx, [
      { frequency: 587, duration: 0.1, delay: 0, type: 'sine', volume: 0.2 },     // D5
      { frequency: 659, duration: 0.1, delay: 0.08, type: 'sine', volume: 0.2 },  // E5
      { frequency: 784, duration: 0.1, delay: 0.16, type: 'sine', volume: 0.2 },  // G5
      { frequency: 880, duration: 0.15, delay: 0.24, type: 'sine', volume: 0.25 }, // A5
      { frequency: 1047, duration: 0.3, delay: 0.36, type: 'sine', volume: 0.25 }, // C6
    ]);
  },
};

/**
 * Sound provider component
 * Manages audio context and sound preferences
 */
export function SoundProvider({ children }: { children: ReactNode }) {
  const storedMuted = useSyncExternalStore(subscribeSoundStorage, getMutedSnapshot, getSoundServerSnapshot);
  const isMuted = storedMuted === 'true';

  const storedInteracted = useSyncExternalStore(subscribeSoundStorage, getInteractedSnapshot, getSoundServerSnapshot);
  const hasUserInteracted = storedInteracted === 'true';
  const audioContextRef = useRef<AudioContext | null>(null);

  // Track user interaction for autoplay policy - keep listening, don't use { once: true }
  useEffect(() => {
    const handleInteraction = () => {
      if (!hasUserInteracted) {
        localStorage.setItem(USER_INTERACTED_KEY, 'true');
        notifySoundStorageChange(USER_INTERACTED_KEY);
      }
      // Always try to resume audio context on interaction
      if (audioContextRef.current?.state === 'suspended') {
        audioContextRef.current.resume().catch(() => {
          // Ignore resume errors
        });
      }
    };

    // Listen continuously to handle cases where AudioContext gets suspended
    window.addEventListener('click', handleInteraction);
    window.addEventListener('touchstart', handleInteraction);
    window.addEventListener('keydown', handleInteraction);

    return () => {
      window.removeEventListener('click', handleInteraction);
      window.removeEventListener('touchstart', handleInteraction);
      window.removeEventListener('keydown', handleInteraction);
    };
  }, [hasUserInteracted]);

  type ExtendedAudioWindow = Window & typeof globalThis & {
    AudioContext: typeof AudioContext;
    webkitAudioContext?: typeof AudioContext;
  };

  // Initialize audio context lazily
  const getAudioContext = useCallback(() => {
    if (!audioContextRef.current) {
      const win = window as ExtendedAudioWindow;
      const ctor = win.AudioContext || win.webkitAudioContext!;
      audioContextRef.current = new ctor();
    }
    return audioContextRef.current;
  }, []);

  const toggleMute = useCallback(() => {
    const newValue = !isMuted;
    localStorage.setItem(SOUND_MUTE_KEY, String(newValue));
    notifySoundStorageChange(SOUND_MUTE_KEY);
  }, [isMuted]);

  const setMuted = useCallback((muted: boolean) => {
    localStorage.setItem(SOUND_MUTE_KEY, String(muted));
    notifySoundStorageChange(SOUND_MUTE_KEY);
  }, []);

  const markUserInteraction = useCallback(() => {
    localStorage.setItem(USER_INTERACTED_KEY, 'true');
    notifySoundStorageChange(USER_INTERACTED_KEY);
    if (audioContextRef.current?.state === 'suspended') {
      audioContextRef.current.resume().catch(() => {
        // Ignore resume errors
      });
    }
  }, []);

  const playSound = useCallback((type: SoundType) => {
    if (isMuted) return;
    
    try {
      const ctx = getAudioContext();
      
      // Ensure audio context is running before playing
      const attemptPlay = () => {
        switch (type) {
          case 'place':
            soundEffects.place(ctx);
            break;
          case 'lineClear':
            soundEffects.lineClear(ctx);
            break;
          case 'combo':
            soundEffects.combo(ctx, 1);
            break;
          case 'gameOver':
            soundEffects.gameOver(ctx);
            break;
          case 'perfectClear':
            soundEffects.perfectClear(ctx);
            break;
          case 'highScore':
            soundEffects.highScore(ctx);
            break;
        }
      };
      
      if (ctx.state === 'suspended') {
        // Resume and then play
        ctx.resume().then(() => {
          attemptPlay();
        }).catch(() => {
          // Ignore errors - browser may block audio
        });
      } else {
        attemptPlay();
      }
    } catch (error) {
      console.warn('Failed to play sound:', error);
    }
  }, [isMuted, getAudioContext]);

  const playComboSound = useCallback((comboCount: number) => {
    if (isMuted) return;
    
    try {
      const ctx = getAudioContext();
      
      const attemptPlay = () => {
        soundEffects.combo(ctx, comboCount);
      };
      
      if (ctx.state === 'suspended') {
        ctx.resume().then(() => {
          attemptPlay();
        }).catch(() => {
          // Ignore errors
        });
      } else {
        attemptPlay();
      }
    } catch (error) {
      console.warn('Failed to play combo sound:', error);
    }
  }, [isMuted, getAudioContext]);

  const value = useMemo(() => ({
    isMuted,
    toggleMute,
    setMuted,
    playSound,
    playComboSound,
    hasUserInteracted,
    markUserInteraction,
  }), [isMuted, toggleMute, setMuted, playSound, playComboSound, hasUserInteracted, markUserInteraction]);

  return (
    <SoundContext.Provider value={value}>
      {children}
    </SoundContext.Provider>
  );
}

/**
 * Hook to access sound context
 */
export function useSound(): SoundContextValue {
  const context = useContext(SoundContext);
  if (!context) {
    throw new Error('useSound must be used within a SoundProvider');
  }
  return context;
}

/**
 * Custom hook for game-specific sound effects
 */
export function useSoundEffects() {
  const { playSound, playComboSound, isMuted, toggleMute } = useSound();
  
  return {
    onPiecePlaced: () => playSound('place'),
    onLineClear: () => playSound('lineClear'),
    onCombo: (count: number) => playComboSound(count),
    onGameOver: () => playSound('gameOver'),
    onPerfectClear: () => playSound('perfectClear'),
    onHighScore: () => playSound('highScore'),
    isMuted,
    toggleMute,
  };
}
