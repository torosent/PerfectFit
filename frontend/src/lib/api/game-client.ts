import { API_BASE_URL } from './index';
import type { GameState, PlacePieceRequest, PlacePieceResponse } from '@/types';

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
 * Generic fetch wrapper with error handling
 */
async function apiFetch<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const url = `${API_BASE_URL}${endpoint}`;
  
  const defaultHeaders: HeadersInit = {
    'Content-Type': 'application/json',
  };

  const response = await fetch(url, {
    ...options,
    headers: {
      ...defaultHeaders,
      ...options.headers,
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
 * @returns The initial game state
 */
export async function createGame(): Promise<GameState> {
  return apiFetch<GameState>('/api/games', {
    method: 'POST',
  });
}

/**
 * Get an existing game by ID
 * @param id - The game session ID
 * @returns The current game state
 */
export async function getGame(id: string): Promise<GameState> {
  return apiFetch<GameState>(`/api/games/${id}`);
}

/**
 * Place a piece on the game board
 * @param gameId - The game session ID
 * @param request - The piece placement request
 * @returns The result of the placement including updated game state
 */
export async function placePiece(
  gameId: string,
  request: PlacePieceRequest
): Promise<PlacePieceResponse> {
  return apiFetch<PlacePieceResponse>(`/api/games/${gameId}/place`, {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

/**
 * End the current game
 * @param gameId - The game session ID
 * @returns The final game state
 */
export async function endGame(gameId: string): Promise<GameState> {
  return apiFetch<GameState>(`/api/games/${gameId}/end`, {
    method: 'POST',
  });
}

/**
 * Get current game state (alias for getGame for semantic clarity)
 */
export const fetchGameState = getGame;
