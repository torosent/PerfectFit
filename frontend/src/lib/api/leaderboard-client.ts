import { API_BASE_URL } from './index';

/**
 * Custom error class for leaderboard API errors
 */
export class LeaderboardError extends Error {
  constructor(
    message: string,
    public statusCode: number,
    public details?: unknown
  ) {
    super(message);
    this.name = 'LeaderboardError';
  }
}

/**
 * Leaderboard entry from the API
 */
export interface LeaderboardEntry {
  rank: number;
  userId: string;
  displayName: string;
  score: number;
  linesCleared: number;
  maxCombo: number;
  achievedAt: string;
}

/**
 * User statistics from the API
 */
export interface UserStats {
  highScore: number;
  gamesPlayed: number;
  globalRank: number | null;
  bestGame: LeaderboardEntry | null;
}

/**
 * Score submission result
 */
export interface SubmitScoreResult {
  success: boolean;
  entry?: LeaderboardEntry;
  isNewHighScore: boolean;
  newRank?: number;
}

/**
 * Options for API requests
 */
interface ApiFetchOptions extends RequestInit {
  /** JWT token for authenticated requests */
  token?: string | null;
}

/**
 * Generic fetch wrapper with error handling
 */
async function apiFetch<T>(
  endpoint: string,
  options: ApiFetchOptions = {}
): Promise<T> {
  const { token, ...fetchOptions } = options;
  const url = `${API_BASE_URL}${endpoint}`;
  
  const defaultHeaders: HeadersInit = {
    'Content-Type': 'application/json',
  };

  // Add authorization header if token provided
  if (token) {
    defaultHeaders['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(url, {
    ...fetchOptions,
    headers: {
      ...defaultHeaders,
      ...fetchOptions.headers,
    },
  });

  if (!response.ok) {
    let errorMessage = `API error: ${response.status} ${response.statusText}`;
    let details: unknown;
    
    try {
      const errorBody = await response.json();
      errorMessage = errorBody.message || errorBody.error || errorMessage;
      details = errorBody;
    } catch {
      // Response body wasn't JSON, use default message
    }

    throw new LeaderboardError(errorMessage, response.status, details);
  }

  // Handle 204 No Content
  if (response.status === 204) {
    return {} as T;
  }

  return response.json();
}

/**
 * Get top scores from the leaderboard
 * @param count - Number of entries to retrieve (default: 100)
 * @returns Array of leaderboard entries
 */
export async function getTopScores(count: number = 100): Promise<LeaderboardEntry[]> {
  return apiFetch<LeaderboardEntry[]>(`/api/leaderboard?count=${count}`);
}

/**
 * Get user statistics
 * @param token - JWT token for authentication
 * @returns User stats including high score, games played, and global rank
 */
export async function getUserStats(token: string): Promise<UserStats> {
  return apiFetch<UserStats>('/api/leaderboard/me', { token });
}

/**
 * Submit a game score to the leaderboard
 * @param gameSessionId - The game session ID to submit
 * @param token - JWT token for authentication
 * @returns Submission result with rank info
 */
export async function submitScore(
  gameSessionId: string,
  token: string
): Promise<SubmitScoreResult> {
  return apiFetch<SubmitScoreResult>('/api/leaderboard/submit', {
    method: 'POST',
    body: JSON.stringify({ gameSessionId }),
    token,
  });
}
