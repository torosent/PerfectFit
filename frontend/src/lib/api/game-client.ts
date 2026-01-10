import { API_BASE_URL } from './index';
import type { GameState, PlacePieceRequest, PlacePieceResponse, GameEndResponse } from '@/types';

/**
 * Custom error class for API errors
 */
export class ApiError extends Error {
  constructor(
    message: string,
    public statusCode: number,
    public details?: unknown
  ) {
    super(message);
    this.name = 'ApiError';
  }
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

    throw new ApiError(errorMessage, response.status, details);
  }

  // Handle 204 No Content
  if (response.status === 204) {
    return {} as T;
  }

  return response.json();
}

/**
 * Create a new game session
 * @param token - Optional JWT token for authenticated users
 * @returns The initial game state
 */
export async function createGame(token?: string | null): Promise<GameState> {
  return apiFetch<GameState>('/api/games', {
    method: 'POST',
    token,
  });
}

/**
 * Get an existing game by ID
 * @param id - The game session ID
 * @param token - Optional JWT token for authenticated users
 * @returns The current game state
 */
export async function getGame(id: string, token?: string | null): Promise<GameState> {
  return apiFetch<GameState>(`/api/games/${id}`, { token });
}

/**
 * Place a piece on the game board
 * @param gameId - The game session ID
 * @param request - The piece placement request
 * @param token - Optional JWT token for authenticated users
 * @returns The result of the placement including updated game state
 */
export async function placePiece(
  gameId: string,
  request: PlacePieceRequest,
  token?: string | null
): Promise<PlacePieceResponse> {
  // Add client timestamp for anti-cheat validation
  const requestWithTimestamp = {
    ...request,
    clientTimestamp: Date.now(),
  };

  return apiFetch<PlacePieceResponse>(`/api/games/${gameId}/place`, {
    method: 'POST',
    body: JSON.stringify(requestWithTimestamp),
    token,
  });
}

/**
 * End the current game
 * @param gameId - The game session ID
 * @param token - Optional JWT token for authenticated users
 * @returns The final game state with gamification updates
 */
export async function endGame(gameId: string, token?: string | null): Promise<GameEndResponse> {
  return apiFetch<GameEndResponse>(`/api/games/${gameId}/end`, {
    method: 'POST',
    token,
  });
}

/**
 * Get current game state (alias for getGame for semantic clarity)
 */
export const fetchGameState = getGame;
