// API client utilities
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5050';

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
  login,
  register,
  verifyEmail,
  AuthError,
  type OAuthProvider,
  type LoginResponse,
  type RegisterResponse,
  type VerifyEmailResponse,
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

export {
  updateProfile,
} from './profile-client';

export {
  getUsers,
  getUser,
  deleteUser,
  bulkDeleteGuests,
  getAuditLogs,
  AdminApiError,
} from './admin-client';

export {
  getGamificationStatus,
  getChallenges,
  getAchievements,
  getSeasonPass,
  getCosmetics,
  getPersonalGoals,
  useStreakFreeze,
  equipCosmetic,
  claimSeasonReward,
  setTimezone,
  getUserTimezone,
  initializeTimezone,
  GamificationApiError,
} from './gamification-client';

export {
  getAdminAchievements,
  getAdminAchievement,
  createAdminAchievement,
  updateAdminAchievement,
  deleteAdminAchievement,
  getAdminChallengeTemplates,
  getAdminChallengeTemplate,
  createAdminChallengeTemplate,
  updateAdminChallengeTemplate,
  deleteAdminChallengeTemplate,
  getAdminCosmetics,
  getAdminCosmetic,
  createAdminCosmetic,
  updateAdminCosmetic,
  deleteAdminCosmetic,
} from './admin-gamification-client';
