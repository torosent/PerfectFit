/**
 * Anti-cheat utilities for game integrity
 * 
 * This module provides client-side validation and integrity checks.
 * Note: Client-side anti-cheat is easily bypassed and should be considered
 * a first line of defense only. All critical validation happens server-side.
 */

// Minimum time between moves in milliseconds (matches backend)
const MIN_TIME_BETWEEN_MOVES_MS = 50;

// Track last move timestamp
let lastMoveTimestamp: number | null = null;

// Track move count for rate limiting
let sessionMoveCount = 0;

// Session start time
let sessionStartTime: number | null = null;

/**
 * Records the start of a new game session
 */
export function startNewSession(): void {
  lastMoveTimestamp = null;
  sessionMoveCount = 0;
  sessionStartTime = Date.now();
}

/**
 * Validates and records a move attempt
 * @returns Object with validation result and client timestamp
 */
export function validateMoveAttempt(): {
  isValid: boolean;
  reason?: string;
  clientTimestamp: number;
  timeSinceLastMove: number | null;
} {
  const now = Date.now();
  const timeSinceLastMove = lastMoveTimestamp ? now - lastMoveTimestamp : null;

  // Check rate limiting
  if (timeSinceLastMove !== null && timeSinceLastMove < MIN_TIME_BETWEEN_MOVES_MS) {
    return {
      isValid: false,
      reason: 'Too fast! Please slow down.',
      clientTimestamp: now,
      timeSinceLastMove,
    };
  }

  // Update tracking
  lastMoveTimestamp = now;
  sessionMoveCount++;

  return {
    isValid: true,
    clientTimestamp: now,
    timeSinceLastMove,
  };
}

/**
 * Gets current session statistics
 */
export function getSessionStats(): {
  moveCount: number;
  sessionDuration: number;
  averageMoveTime: number | null;
} {
  const duration = sessionStartTime ? Date.now() - sessionStartTime : 0;
  return {
    moveCount: sessionMoveCount,
    sessionDuration: duration,
    averageMoveTime: sessionMoveCount > 0 ? duration / sessionMoveCount : null,
  };
}

/**
 * Validates that a piece placement is within bounds
 */
export function validatePlacementBounds(
  row: number,
  col: number,
  gridSize: number = 8
): boolean {
  return row >= 0 && row < gridSize && col >= 0 && col < gridSize;
}

/**
 * Validates that a piece index is valid
 */
export function validatePieceIndex(
  pieceIndex: number,
  availablePieces: number
): boolean {
  return pieceIndex >= 0 && pieceIndex < availablePieces;
}

/**
 * Detects common cheating patterns
 * Note: These checks are easily bypassed and are mainly for deterrence
 */
export function detectSuspiciousBehavior(): {
  suspicious: boolean;
  reasons: string[];
} {
  const reasons: string[] = [];

  // Check if DevTools might be open (rough heuristic)
  // Note: This is not reliable and can have false positives
  const devToolsOpen = checkDevToolsOpen();
  if (devToolsOpen) {
    reasons.push('Developer tools detected');
  }

  // Check for suspicious timing patterns
  if (sessionMoveCount > 10 && sessionStartTime) {
    const avgTimePerMove = (Date.now() - sessionStartTime) / sessionMoveCount;
    // Less than 100ms average per move is suspicious
    if (avgTimePerMove < 100) {
      reasons.push('Suspiciously fast move rate');
    }
  }

  return {
    suspicious: reasons.length > 0,
    reasons,
  };
}

/**
 * Rough check for DevTools (not reliable, just a deterrent)
 */
function checkDevToolsOpen(): boolean {
  // This is a common but unreliable check
  // It can have false positives and is easily bypassed
  const threshold = 160;
  const widthThreshold = window.outerWidth - window.innerWidth > threshold;
  const heightThreshold = window.outerHeight - window.innerHeight > threshold;
  
  return widthThreshold || heightThreshold;
}

/**
 * Generates a simple client fingerprint for session tracking
 * This is NOT for security - just for logging/debugging
 */
export function generateClientFingerprint(): string {
  const components: string[] = [
    navigator.userAgent,
    navigator.language,
    screen.width.toString(),
    screen.height.toString(),
    new Date().getTimezoneOffset().toString(),
  ];
  
  // Simple hash of components
  const str = components.join('|');
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    const char = str.charCodeAt(i);
    hash = ((hash << 5) - hash) + char;
    hash = hash & hash; // Convert to 32bit integer
  }
  
  return Math.abs(hash).toString(16);
}

/**
 * Logs suspicious activity (in production, this could report to analytics)
 */
export function logSuspiciousActivity(
  activity: string,
  details?: Record<string, unknown>
): void {
  // In development, just log to console
  if (process.env.NODE_ENV === 'development') {
    console.warn('[Anti-Cheat]', activity, details);
  }
  
  // In production, you might want to:
  // - Send to analytics
  // - Log to error tracking service
  // - Flag the session for review
}
