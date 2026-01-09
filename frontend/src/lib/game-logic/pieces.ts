import type { Piece, PieceShape, PieceType, Grid } from '@/types';

/**
 * Piece shape definitions - mirrors backend piece definitions
 * Each shape is a 2D matrix where 1 = filled, 0 = empty
 */
export const PIECE_SHAPES: Record<PieceType, PieceShape> = {
  // Tetrominoes
  I: [[1, 1, 1, 1]],
  O: [
    [1, 1],
    [1, 1],
  ],
  T: [
    [1, 1, 1],
    [0, 1, 0],
  ],
  S: [
    [0, 1, 1],
    [1, 1, 0],
  ],
  Z: [
    [1, 1, 0],
    [0, 1, 1],
  ],
  J: [
    [1, 0],
    [1, 0],
    [1, 1],
  ],
  L: [
    [0, 1],
    [0, 1],
    [1, 1],
  ],
  // Lines
  DOT: [[1]],
  LINE2: [[1, 1]],
  LINE3: [[1, 1, 1]],
  LINE5: [[1, 1, 1, 1, 1]],
  // Corners
  CORNER: [
    [1, 1],
    [1, 0],
  ],
  BIG_CORNER: [
    [1, 1, 1],
    [1, 0, 0],
    [1, 0, 0],
  ],
  // Squares
  SQUARE_2X2: [
    [1, 1],
    [1, 1],
  ],
  SQUARE_3X3: [
    [1, 1, 1],
    [1, 1, 1],
    [1, 1, 1],
  ],
};

/**
 * Color mapping for each piece type
 */
export const PIECE_COLORS: Record<PieceType, string> = {
  // Tetrominoes - classic Tetris colors
  I: '#00FFFF', // Cyan
  O: '#FFFF00', // Yellow
  T: '#800080', // Purple
  S: '#00FF00', // Green
  Z: '#FF0000', // Red
  J: '#0000FF', // Blue
  L: '#FFA500', // Orange
  // Lines
  DOT: '#808080', // Gray
  LINE2: '#FFB6C1', // Light Pink
  LINE3: '#90EE90', // Light Green
  LINE5: '#87CEEB', // Sky Blue
  // Corners
  CORNER: '#DDA0DD', // Plum
  BIG_CORNER: '#F0E68C', // Khaki
  // Squares
  SQUARE_2X2: '#CD853F', // Peru
  SQUARE_3X3: '#8B4513', // Saddle Brown
};

/**
 * Create a piece object from a piece type
 */
export function createPiece(type: PieceType): Piece {
  return {
    type,
    shape: PIECE_SHAPES[type],
    color: PIECE_COLORS[type],
  };
}

/**
 * Get the dimensions of a piece shape
 */
export function getPieceDimensions(shape: PieceShape): { rows: number; cols: number } {
  return {
    rows: shape.length,
    cols: shape[0]?.length ?? 0,
  };
}

/**
 * Check if a piece can be placed at the given position on the grid
 * @param grid - The current game grid
 * @param piece - The piece to place
 * @param row - The top-left row position
 * @param col - The top-left column position
 * @returns true if the piece can be placed, false otherwise
 */
export function canPlacePiece(
  grid: Grid,
  piece: Piece,
  row: number,
  col: number
): boolean {
  const { rows: gridRows, cols: gridCols } = { rows: grid.length, cols: grid[0]?.length ?? 0 };
  const { rows: pieceRows, cols: pieceCols } = getPieceDimensions(piece.shape);

  // Check if piece is within grid bounds
  if (row < 0 || col < 0 || row + pieceRows > gridRows || col + pieceCols > gridCols) {
    return false;
  }

  // Check if all cells the piece would occupy are empty
  for (let r = 0; r < pieceRows; r++) {
    for (let c = 0; c < pieceCols; c++) {
      if (piece.shape[r][c] === 1) {
        if (grid[row + r][col + c] !== null) {
          return false;
        }
      }
    }
  }

  return true;
}

/**
 * Get all cells that a piece would occupy at a given position
 * @param piece - The piece
 * @param row - The top-left row position
 * @param col - The top-left column position
 * @returns Array of {row, col} positions
 */
export function getPieceCells(
  piece: Piece,
  row: number,
  col: number
): Array<{ row: number; col: number }> {
  const cells: Array<{ row: number; col: number }> = [];
  const { rows: pieceRows, cols: pieceCols } = getPieceDimensions(piece.shape);

  for (let r = 0; r < pieceRows; r++) {
    for (let c = 0; c < pieceCols; c++) {
      if (piece.shape[r][c] === 1) {
        cells.push({ row: row + r, col: col + c });
      }
    }
  }

  return cells;
}

/**
 * Create an empty 8x8 grid
 */
export function createEmptyGrid(): Grid {
  return Array.from({ length: 8 }, () => Array.from({ length: 8 }, () => null));
}

/**
 * Get all available piece types
 */
export function getAllPieceTypes(): PieceType[] {
  return Object.keys(PIECE_SHAPES) as PieceType[];
}

/**
 * Represents lines that would be cleared by placing a piece
 */
export interface PotentialLineClear {
  rows: number[];
  cols: number[];
}

/**
 * Calculate which rows and columns would be cleared if a piece is placed at the given position
 * @param grid - The current game grid
 * @param piece - The piece to place
 * @param row - The top-left row position
 * @param col - The top-left column position
 * @returns Object containing arrays of row and column indices that would be cleared
 */
export function getPotentialLineClear(
  grid: Grid,
  piece: Piece,
  row: number,
  col: number
): PotentialLineClear {
  // If the piece can't be placed, return empty
  if (!canPlacePiece(grid, piece, row, col)) {
    return { rows: [], cols: [] };
  }

  // Create a simulated grid with the piece placed
  const simulatedGrid: Grid = grid.map(r => [...r]);
  const pieceCells = getPieceCells(piece, row, col);
  
  // Place the piece on the simulated grid
  for (const cell of pieceCells) {
    if (cell.row >= 0 && cell.row < 8 && cell.col >= 0 && cell.col < 8) {
      simulatedGrid[cell.row][cell.col] = piece.color;
    }
  }

  // Check which rows would be cleared (all cells filled)
  const clearedRows: number[] = [];
  for (let r = 0; r < 8; r++) {
    const rowFilled = simulatedGrid[r].every(cell => cell !== null);
    if (rowFilled) {
      clearedRows.push(r);
    }
  }

  // Check which columns would be cleared (all cells filled)
  const clearedCols: number[] = [];
  for (let c = 0; c < 8; c++) {
    const colFilled = simulatedGrid.every(r => r[c] !== null);
    if (colFilled) {
      clearedCols.push(c);
    }
  }

  return { rows: clearedRows, cols: clearedCols };
}
