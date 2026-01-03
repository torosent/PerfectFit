// Cell can be empty (null) or contain a color
export type CellValue = string | null;

// 10x10 grid
export type Grid = CellValue[][];

// Piece shape as 2D matrix of 0s and 1s
export type PieceShape = number[][];

// Piece types
export type PieceType =
  | 'I' | 'O' | 'T' | 'S' | 'Z' | 'J' | 'L'  // Tetrominoes
  | 'DOT' | 'LINE2' | 'LINE3' | 'LINE5'       // Lines
  | 'CORNER' | 'BIG_CORNER'                    // Corners
  | 'SQUARE_2X2' | 'SQUARE_3X3';              // Squares

export interface Piece {
  type: PieceType;
  shape: PieceShape;
  color: string;
}

export interface Position {
  row: number;
  col: number;
}

export interface ClearingCell {
  row: number;
  col: number;
  color: string;
}

export interface GameState {
  id: string;
  grid: Grid;
  currentPieces: Piece[];
  score: number;
  combo: number;
  status: 'Playing' | 'Ended';
  linesCleared: number;
}

export interface PlacePieceRequest {
  pieceIndex: number;
  position: Position;
  clientTimestamp?: number;
}

export interface PlacePieceResponse {
  success: boolean;
  gameState: GameState;
  linesCleared: number;
  pointsEarned: number;
  isGameOver: boolean;
  piecesRemainingInTurn: number;
  newTurnStarted: boolean;
}

export interface LeaderboardEntry {
  rank: number;
  userId?: string;
  displayName: string;
  score: number;
  achievedAt: string;
  avatar?: string;
  linesCleared?: number;
  maxCombo?: number;
}

export interface UserProfile {
  id: string;
  displayName: string;
  email?: string;
  provider: 'google' | 'facebook' | 'microsoft' | 'guest' | 'local';
  highScore: number;
  gamesPlayed: number;
  avatar?: string;
  role?: string;
}

/**
 * Request payload for updating user profile
 */
export interface UpdateProfileRequest {
  displayName?: string;
  avatar?: string;
}

/**
 * Response from profile update endpoint
 */
export interface UpdateProfileResponse {
  success: boolean;
  errorMessage?: string;
  suggestedDisplayName?: string;
  profile?: {
    id: number;
    displayName: string;
    avatar?: string;
  };
}
