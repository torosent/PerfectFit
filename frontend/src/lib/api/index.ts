// API client utilities
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

export {
  createGame,
  getGame,
  placePiece,
  endGame,
  fetchGameState,
  ApiError,
} from './game-client';
