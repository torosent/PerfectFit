'use client';

import confetti from 'canvas-confetti';

/**
 * Confetti burst effects for the game
 */

interface ConfettiOptions {
  /** Number of particles (default: 100) */
  particleCount?: number;
  /** Spread angle in degrees (default: 70) */
  spread?: number;
  /** Starting x position (0-1, default: 0.5) */
  x?: number;
  /** Starting y position (0-1, default: 0.5) */
  y?: number;
  /** Colors array */
  colors?: string[];
}

/**
 * Fire a confetti burst
 */
export function fireConfetti(options: ConfettiOptions = {}): void {
  const {
    particleCount = 100,
    spread = 70,
    x = 0.5,
    y = 0.5,
    colors = ['#14b8a6', '#22d3ee', '#fbbf24', '#f472b6', '#a855f7'],
  } = options;

  confetti({
    particleCount,
    spread,
    origin: { x, y },
    colors,
    disableForReducedMotion: true,
  });
}

/**
 * Fire confetti for a new high score
 */
export function fireHighScoreConfetti(): void {
  const duration = 2000;
  const end = Date.now() + duration;

  const frame = () => {
    confetti({
      particleCount: 3,
      angle: 60,
      spread: 55,
      origin: { x: 0 },
      colors: ['#fbbf24', '#f59e0b', '#eab308'],
      disableForReducedMotion: true,
    });
    confetti({
      particleCount: 3,
      angle: 120,
      spread: 55,
      origin: { x: 1 },
      colors: ['#fbbf24', '#f59e0b', '#eab308'],
      disableForReducedMotion: true,
    });

    if (Date.now() < end) {
      requestAnimationFrame(frame);
    }
  };

  frame();
}

/**
 * Fire confetti for clearing 4+ lines
 */
export function fireBigClearConfetti(): void {
  confetti({
    particleCount: 150,
    spread: 100,
    origin: { x: 0.5, y: 0.6 },
    colors: ['#14b8a6', '#22d3ee', '#0ea5e9', '#6366f1'],
    disableForReducedMotion: true,
  });
}

/**
 * Fire confetti for combo 5+
 */
export function fireComboConfetti(comboCount: number): void {
  const particleCount = Math.min(50 + comboCount * 20, 200);
  
  confetti({
    particleCount,
    spread: 60 + comboCount * 5,
    origin: { x: 0.5, y: 0.3 },
    colors: ['#f472b6', '#a855f7', '#ec4899', '#d946ef'],
    disableForReducedMotion: true,
  });
}

/**
 * Fire epic confetti for perfect clear (board completely empty)
 */
export function firePerfectClearConfetti(): void {
  const duration = 3000;
  const animationEnd = Date.now() + duration;
  const defaults = { 
    startVelocity: 30, 
    spread: 360, 
    ticks: 60, 
    zIndex: 1000,
    disableForReducedMotion: true,
  };

  function randomInRange(min: number, max: number): number {
    return Math.random() * (max - min) + min;
  }

  const interval: ReturnType<typeof setInterval> = setInterval(function() {
    const timeLeft = animationEnd - Date.now();

    if (timeLeft <= 0) {
      return clearInterval(interval);
    }

    const particleCount = 50 * (timeLeft / duration);
    
    // Fire from two points for more impact
    confetti({
      ...defaults,
      particleCount,
      origin: { x: randomInRange(0.1, 0.3), y: Math.random() - 0.2 },
      colors: ['#fbbf24', '#f59e0b', '#fcd34d', '#ffffff'],
    });
    confetti({
      ...defaults,
      particleCount,
      origin: { x: randomInRange(0.7, 0.9), y: Math.random() - 0.2 },
      colors: ['#fbbf24', '#f59e0b', '#fcd34d', '#ffffff'],
    });
  }, 250);
}

/**
 * Fire a starburst effect (for perfect clear)
 */
export function fireStarburst(): void {
  const defaults = {
    origin: { y: 0.5 },
    disableForReducedMotion: true,
  };

  function fire(particleRatio: number, opts: confetti.Options): void {
    confetti({
      ...defaults,
      ...opts,
      particleCount: Math.floor(200 * particleRatio),
    });
  }

  fire(0.25, {
    spread: 26,
    startVelocity: 55,
    colors: ['#fbbf24'],
  });
  fire(0.2, {
    spread: 60,
    colors: ['#f59e0b'],
  });
  fire(0.35, {
    spread: 100,
    decay: 0.91,
    scalar: 0.8,
    colors: ['#fcd34d'],
  });
  fire(0.1, {
    spread: 120,
    startVelocity: 25,
    decay: 0.92,
    scalar: 1.2,
    colors: ['#fef3c7'],
  });
  fire(0.1, {
    spread: 120,
    startVelocity: 45,
    colors: ['#ffffff'],
  });
}
