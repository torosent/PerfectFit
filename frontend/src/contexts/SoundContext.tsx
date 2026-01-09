'use client';

import { createContext, useContext, useState, useEffect, useCallback, useRef, useMemo, type ReactNode } from 'react';

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
  const [isMuted, setIsMuted] = useState(true); // Default muted until we check localStorage
  const [hasUserInteracted, setHasUserInteracted] = useState(false);
  const audioContextRef = useRef<AudioContext | null>(null);

  // Load mute preference from localStorage
  useEffect(() => {
    const stored = localStorage.getItem(SOUND_MUTE_KEY);
    // Default to unmuted if no preference stored
    setIsMuted(stored === 'true');
  }, []);

  // Track user interaction for autoplay policy
  useEffect(() => {
    const handleInteraction = () => {
      setHasUserInteracted(true);
      // Resume audio context if it was suspended
      if (audioContextRef.current?.state === 'suspended') {
        audioContextRef.current.resume();
      }
    };

    window.addEventListener('click', handleInteraction, { once: true });
    window.addEventListener('touchstart', handleInteraction, { once: true });
    window.addEventListener('keydown', handleInteraction, { once: true });

    return () => {
      window.removeEventListener('click', handleInteraction);
      window.removeEventListener('touchstart', handleInteraction);
      window.removeEventListener('keydown', handleInteraction);
    };
  }, []);

  type ExtendedAudioWindow = Window & typeof globalThis & {
    AudioContext: typeof AudioContext;
    webkitAudioContext?: typeof AudioContext;
  };

  const getAudioContextConstructor = (): typeof AudioContext => {
    const win = window as ExtendedAudioWindow;
    return win.AudioContext || win.webkitAudioContext!;
  };

  // Initialize audio context lazily
  const getAudioContext = useCallback(() => {
    if (!audioContextRef.current) {
      audioContextRef.current = new (getAudioContextConstructor())();
    }
    return audioContextRef.current;
  }, []);

  const toggleMute = useCallback(() => {
    setIsMuted((prev) => {
      const newValue = !prev;
      localStorage.setItem(SOUND_MUTE_KEY, String(newValue));
      return newValue;
    });
  }, []);

  const setMuted = useCallback((muted: boolean) => {
    setIsMuted(muted);
    localStorage.setItem(SOUND_MUTE_KEY, String(muted));
  }, []);

  const markUserInteraction = useCallback(() => {
    setHasUserInteracted(true);
    if (audioContextRef.current?.state === 'suspended') {
      audioContextRef.current.resume();
    }
  }, []);

  const playSound = useCallback((type: SoundType) => {
    if (isMuted || !hasUserInteracted) return;
    
    try {
      const ctx = getAudioContext();
      if (ctx.state === 'suspended') {
        ctx.resume();
      }
      
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
    } catch (error) {
      console.warn('Failed to play sound:', error);
    }
  }, [isMuted, hasUserInteracted, getAudioContext]);

  const playComboSound = useCallback((comboCount: number) => {
    if (isMuted || !hasUserInteracted) return;
    
    try {
      const ctx = getAudioContext();
      if (ctx.state === 'suspended') {
        ctx.resume();
      }
      soundEffects.combo(ctx, comboCount);
    } catch (error) {
      console.warn('Failed to play combo sound:', error);
    }
  }, [isMuted, hasUserInteracted, getAudioContext]);

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
