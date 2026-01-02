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

export {
  getOAuthUrl,
  getCurrentUser,
  refreshToken,
  createGuestSession,
  logout,
  getAuthHeaders,
  AuthError,
  type OAuthProvider,
} from './auth-client';

export {
  getTopScores,
  getUserStats,
  submitScore,
  LeaderboardError,
  type LeaderboardEntry,
  type UserStats,
  type SubmitScoreResult,
} from './leaderboard-client';
