import React from 'react';
import { TetrominoPiece } from './TetrominoPiece';

export function FloatingPieces() {
  const pieces = [
    { shape: [[1, 1], [1, 1]], color: "#fbbf24", delay: "0s", x: "10%", y: "20%" },
    { shape: [[1, 1, 1, 1]], color: "#2dd4bf", delay: "2s", x: "80%", y: "15%" },
    { shape: [[1, 0], [1, 0], [1, 1]], color: "#fb923c", delay: "4s", x: "15%", y: "70%" },
    { shape: [[0, 1], [1, 1], [1, 0]], color: "#34d399", delay: "1s", x: "85%", y: "60%" },
    { shape: [[1, 1, 1], [0, 1, 0]], color: "#38bdf8", delay: "3s", x: "5%", y: "45%" },
    { shape: [[1, 1, 0], [0, 1, 1]], color: "#f97316", delay: "5s", x: "90%", y: "35%" },
    { shape: [[0, 1, 1], [1, 1, 0]], color: "#22d3ee", delay: "2.5s", x: "75%", y: "80%" },
  ];

  return (
    <div className="absolute inset-0 overflow-hidden pointer-events-none">
      {pieces.map((piece, i) => (
        <div
          key={i}
          className="absolute animate-float opacity-20"
          style={{
            left: piece.x,
            top: piece.y,
            animationDelay: piece.delay,
          }}
        >
          <TetrominoPiece shape={piece.shape} color={piece.color} />
        </div>
      ))}
    </div>
  );
}
