"use client";

import React, { useState, useEffect } from 'react';
import { theme } from '@/lib/landing-theme';

export function GameBoardPreview() {
  // Pre-computed animation frames for smooth, reliable playback
  // Each frame is a complete board state with optional clearing info
  const animationFrames: { 
    board: (string | null)[][]; 
    clearing: { rows: number[]; cols: number[] }; 
    score: number 
  }[] = [
    // Frame 0: Empty board
    { board: Array(8).fill(null).map(() => Array(8).fill(null)), clearing: { rows: [], cols: [] }, score: 0 },
  ];
  
  // Generate frames programmatically
  (function() {
    // Helper to create board copy
    const copyBoard = (b: (string | null)[][]) => b.map(r => [...r]);
    
    // Frame 1: O-piece at bottom-left (rows 6-7, cols 0-1)
    let b = copyBoard(animationFrames[0].board);
    b[6][0] = "#fbbf24"; b[6][1] = "#fbbf24";
    b[7][0] = "#fbbf24"; b[7][1] = "#fbbf24";
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 0 });
    
    // Frame 2: Horizontal domino (row 7, cols 2-3)
    b = copyBoard(b);
    b[7][2] = "#34d399"; b[7][3] = "#34d399";
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 0 });
    
    // Frame 3: O-piece middle (rows 6-7, cols 4-5)
    b = copyBoard(b);
    b[6][4] = "#fbbf24"; b[6][5] = "#fbbf24";
    b[7][4] = "#fbbf24"; b[7][5] = "#fbbf24";
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 0 });
    
    // Frame 4: Domino completes row 7 (cols 6-7) - CLEARING state
    b = copyBoard(b);
    b[7][6] = "#34d399"; b[7][7] = "#34d399";
    animationFrames.push({ board: b, clearing: { rows: [7], cols: [] }, score: 0 });
    
    // Frame 5: After clear - row 7 empty
    b = copyBoard(b);
    for (let c = 0; c < 8; c++) b[7][c] = null;
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 100 });
    
    // Frame 6: L-shape piece (rows 4-6, cols 2-3)
    b = copyBoard(b);
    b[4][2] = "#fb923c"; b[5][2] = "#fb923c"; b[6][2] = "#fb923c"; b[6][3] = "#fb923c";
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 100 });
    
    // Frame 7: J-shape piece (rows 4-6, cols 5-6)
    b = copyBoard(b);
    b[4][6] = "#38bdf8"; b[5][6] = "#38bdf8"; b[6][5] = "#38bdf8"; b[6][6] = "#38bdf8";
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 100 });
    
    // Frame 8: Horizontal 3-block (row 7, cols 0-2)
    b = copyBoard(b);
    b[7][0] = "#a855f7"; b[7][1] = "#a855f7"; b[7][2] = "#a855f7";
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 100 });
    
    // Frame 9: Horizontal 3-block (row 7, cols 3-5)
    b = copyBoard(b);
    b[7][3] = "#a855f7"; b[7][4] = "#a855f7"; b[7][5] = "#a855f7";
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 100 });
    
    // Frame 10: Domino completes row 7 again - CLEARING
    b = copyBoard(b);
    b[7][6] = "#34d399"; b[7][7] = "#34d399";
    animationFrames.push({ board: b, clearing: { rows: [7], cols: [] }, score: 100 });
    
    // Frame 11: After second clear
    b = copyBoard(b);
    for (let c = 0; c < 8; c++) b[7][c] = null;
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 200 });
    
    // Frame 12: T-piece (rows 3-4, cols 3-5)
    b = copyBoard(b);
    b[3][3] = "#2dd4bf"; b[3][4] = "#2dd4bf"; b[3][5] = "#2dd4bf"; b[4][4] = "#2dd4bf";
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 200 });
    
    // Frame 13: S-piece (rows 2-3, cols 6-7)
    b = copyBoard(b);
    b[2][7] = "#f472b6"; b[3][6] = "#f472b6"; b[3][7] = "#f472b6";
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 200 });
    
    // Frame 14: Vertical 3-block on col 7 (rows 4-6)
    b = copyBoard(b);
    b[4][7] = "#22d3ee"; b[5][7] = "#22d3ee"; b[6][7] = "#22d3ee";
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 200 });
    
    // Frame 15: Single blocks scattered
    b = copyBoard(b);
    b[1][2] = "#ef4444"; b[2][4] = "#ef4444"; b[5][3] = "#ef4444";
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 200 });
    
    // Frame 16: More decoration - domino
    b = copyBoard(b);
    b[7][0] = "#fbbf24"; b[7][1] = "#fbbf24";
    animationFrames.push({ board: b, clearing: { rows: [], cols: [] }, score: 200 });
  })();

  const [frameIndex, setFrameIndex] = useState(0);

  // Smooth animation loop using requestAnimationFrame timing
  useEffect(() => {
    const interval = setInterval(() => {
      setFrameIndex(prev => (prev + 1) % animationFrames.length);
    }, 900);

    return () => clearInterval(interval);
  }, [animationFrames.length]);

  const currentFrame = animationFrames[frameIndex];
  const { board, clearing, score } = currentFrame;

  // Check if a cell is being cleared
  const isCellClearing = (row: number, col: number) => {
    return clearing.rows.includes(row) || clearing.cols.includes(col);
  };

  return (
    <div className="relative">
      {/* Glow effect behind board */}
      <div className="absolute inset-0 blur-3xl scale-110" style={{ background: `linear-gradient(to right, ${theme.glowTeal}, ${theme.glowBlue}, ${theme.glowCyan})` }} />
      
      {/* Score display */}
      <div className="relative mb-3 flex items-center justify-center">
        <div 
          className="px-4 py-2 rounded-xl text-sm font-bold"
          style={{ background: theme.bgCard, border: `1px solid ${theme.borderLight}` }}
        >
          <span style={{ color: theme.textSecondary }}>Score: </span>
          <span style={{ color: '#fbbf24', transition: 'all 0.3s ease' }}>
            {score.toLocaleString()}
          </span>
        </div>
      </div>
      
      {/* The game board */}
      <div className="relative p-3 rounded-2xl shadow-2xl" style={{ background: theme.bgCard, border: `1px solid ${theme.borderMedium}` }}>
        <div className="grid grid-cols-8 gap-0.5">
          {board.flat().map((cell, i) => {
            const row = Math.floor(i / 8);
            const col = i % 8;
            const cellClearing = cell && isCellClearing(row, col);
            
            return (
              <div
                key={i}
                className="w-5 h-5 md:w-6 md:h-6 rounded-sm"
                style={{ 
                  background: cell 
                    ? cellClearing 
                      ? `linear-gradient(45deg, ${cell}, #ffffff, ${cell})`
                      : cell
                    : 'rgba(30, 58, 95, 0.6)',
                  border: cell ? 'none' : '1px solid rgba(56, 97, 140, 0.4)',
                  boxShadow: cell 
                    ? cellClearing 
                      ? `0 0 12px ${cell}, 0 0 24px ${cell}` 
                      : '0 2px 4px rgba(0,0,0,0.3)'
                    : 'none',
                  transform: cellClearing ? 'scale(1.1)' : 'scale(1)',
                  transition: 'all 0.25s ease-out',
                }}
              />
            );
          })}
        </div>
      </div>
    </div>
  );
}
