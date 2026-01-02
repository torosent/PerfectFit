import type { Variants, Transition } from 'motion/react';

/**
 * Animation variants for game board cells
 */
export const cellVariants: Variants = {
  empty: { 
    scale: 1, 
    opacity: 0.3,
  },
  filled: { 
    scale: 1, 
    opacity: 1,
  },
  clearing: {
    scale: [1, 1.3, 1.1, 0],
    opacity: [1, 1, 1, 0],
    rotate: [0, 0, 180, 360],
    filter: [
      'brightness(1) hue-rotate(0deg)',
      'brightness(2) hue-rotate(90deg)',
      'brightness(2.5) hue-rotate(180deg)',
      'brightness(0) hue-rotate(360deg)',
    ],
    boxShadow: [
      '0 0 0px rgba(255,255,255,0)',
      '0 0 30px rgba(255,255,255,1), 0 0 60px currentColor',
      '0 0 50px rgba(255,255,255,1), 0 0 100px currentColor',
      '0 0 0px rgba(255,255,255,0)',
    ],
    transition: { 
      duration: 0.6,
      ease: [0.4, 0, 0.2, 1],
      times: [0, 0.3, 0.6, 1],
    },
  },
  placed: {
    scale: [0, 1.15, 1],
    opacity: [0, 1, 1],
    transition: { 
      duration: 0.3,
      ease: [0.34, 1.56, 0.64, 1], // spring-like easing
    },
  },
};

/**
 * Animation variants for draggable pieces
 */
export const pieceVariants: Variants = {
  idle: { 
    scale: 1,
    rotate: 0,
  },
  hover: { 
    scale: 1.05,
    transition: { duration: 0.2 },
  },
  dragging: { 
    scale: 1.1, 
    opacity: 0.8,
    rotate: 2,
    transition: { duration: 0.15 },
  },
  selected: { 
    scale: 1.05,
    boxShadow: '0 0 20px rgba(59, 130, 246, 0.5)',
    transition: { duration: 0.2 },
  },
  tap: {
    scale: 0.95,
    transition: { duration: 0.1 },
  },
};

/**
 * Animation variants for score display
 */
export const scoreVariants: Variants = {
  initial: { scale: 1 },
  update: { 
    scale: [1, 1.15, 1],
    transition: { 
      duration: 0.3,
      ease: 'easeOut',
    },
  },
  highlight: {
    color: ['#ffffff', '#fbbf24', '#ffffff'],
    transition: { duration: 0.5 },
  },
};

/**
 * Animation variants for combo indicator
 */
export const comboVariants: Variants = {
  initial: {
    y: 20,
    opacity: 0,
    scale: 0.5,
  },
  show: {
    y: 0,
    opacity: 1,
    scale: 1,
    transition: {
      type: 'spring',
      stiffness: 400,
      damping: 15,
    },
  },
  exit: {
    y: -20,
    opacity: 0,
    scale: 0.8,
    transition: {
      duration: 0.2,
      ease: 'easeIn',
    },
  },
  pulse: {
    scale: [1, 1.2, 1],
    transition: {
      duration: 0.3,
      ease: 'easeInOut',
    },
  },
};

/**
 * Animation variants for points popup
 */
export const pointsPopupVariants: Variants = {
  initial: {
    y: 0,
    opacity: 0,
    scale: 0.5,
  },
  animate: {
    y: -60,
    opacity: [0, 1, 1, 0],
    scale: [0.5, 1.2, 1, 0.8],
    transition: {
      duration: 1.2,
      ease: 'easeOut',
      times: [0, 0.2, 0.8, 1],
    },
  },
};

/**
 * Animation variants for modal
 */
export const modalVariants: Variants = {
  hidden: {
    opacity: 0,
    scale: 0.8,
    y: 20,
  },
  visible: {
    opacity: 1,
    scale: 1,
    y: 0,
    transition: {
      type: 'spring',
      stiffness: 300,
      damping: 25,
    },
  },
  exit: {
    opacity: 0,
    scale: 0.9,
    y: -10,
    transition: {
      duration: 0.2,
      ease: 'easeIn',
    },
  },
};

/**
 * Animation variants for modal backdrop
 */
export const backdropVariants: Variants = {
  hidden: { opacity: 0 },
  visible: { 
    opacity: 1,
    transition: { duration: 0.2 },
  },
  exit: { 
    opacity: 0,
    transition: { duration: 0.2 },
  },
};

/**
 * Animation variants for staggered children
 */
export const staggerContainerVariants: Variants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.05,
      delayChildren: 0.1,
    },
  },
};

/**
 * Animation variants for staggered items
 */
export const staggerItemVariants: Variants = {
  hidden: { 
    opacity: 0, 
    y: 20,
  },
  visible: {
    opacity: 1,
    y: 0,
    transition: {
      type: 'spring',
      stiffness: 300,
      damping: 24,
    },
  },
};

/**
 * Animation variants for button interactions
 */
export const buttonVariants: Variants = {
  idle: { scale: 1 },
  hover: { 
    scale: 1.02,
    transition: { duration: 0.2 },
  },
  tap: { 
    scale: 0.98,
    transition: { duration: 0.1 },
  },
};

/**
 * Transition presets
 */
export const springTransition: Transition = {
  type: 'spring',
  stiffness: 300,
  damping: 20,
};

export const smoothTransition: Transition = {
  duration: 0.3,
  ease: 'easeInOut',
};

export const quickTransition: Transition = {
  duration: 0.15,
  ease: 'easeOut',
};

/**
 * Animation variants for the clearing shockwave overlay
 */
export const clearingShockwaveVariants: Variants = {
  initial: {
    scaleX: 0,
    opacity: 0,
  },
  animate: {
    scaleX: [0, 1.2, 1],
    opacity: [0, 1, 0],
    transition: {
      duration: 0.5,
      ease: 'easeOut',
      times: [0, 0.4, 1],
    },
  },
};

/**
 * Animation variants for the clearing sparkle particles
 */
export const clearingSparkleVariants: Variants = {
  initial: {
    scale: 0,
    opacity: 0,
  },
  animate: {
    scale: [0, 1.5, 0],
    opacity: [0, 1, 0],
    y: [0, -20, -40],
    transition: {
      duration: 0.8,
      ease: 'easeOut',
    },
  },
};

/**
 * Get stagger delay for clearing animation based on cell position
 * Creates a wave effect sweeping across the line
 */
export function getClearingDelay(row: number, col: number): number {
  // Wave sweeps from left to right for rows, top to bottom for columns
  return col * 0.04;
}

/**
 * Get placed animation delay for cells in a piece
 * Creates a cascade effect
 */
export function getPlacedDelay(index: number): number {
  return index * 0.05;
}
