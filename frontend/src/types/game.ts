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

export interface GameState {
  id: string;
  grid: Grid;
  currentPieces: Piece[];
  score: number;
  combo: number;
  status: 'playing' | 'ended';
  linesCleared: number;
}

export interface PlacePieceRequest {
  pieceIndex: number;
  position: Position;
}

export interface PlacePieceResponse {
  success: boolean;
  gameState: GameState;
  linesCleared: number;
  pointsEarned: number;
  isGameOver: boolean;
}

export interface LeaderboardEntry {
  rank: number;
  userId: string;
  displayName: string;
  score: number;
  achievedAt: string;
}

export interface UserProfile {
  id: string;
  displayName: string;
  email?: string;
  provider: 'google' | 'apple' | 'microsoft' | 'guest';
  highScore: number;
  gamesPlayed: number;
}
