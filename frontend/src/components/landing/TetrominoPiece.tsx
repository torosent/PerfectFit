import React from 'react';

export function TetrominoPiece({ 
  shape, 
  color, 
  className = "" 
}: { 
  shape: number[][]; 
  color: string; 
  className?: string;
}) {
  return (
    <div className={`grid gap-0.5 ${className}`} style={{ 
      gridTemplateColumns: `repeat(${shape[0].length}, 1fr)` 
    }}>
      {shape.flat().map((cell, i) => (
        <div
          key={i}
          className="w-4 h-4 md:w-5 md:h-5 rounded-sm transition-all duration-300"
          style={cell ? { background: color, boxShadow: '0 4px 6px rgba(0,0,0,0.3)' } : undefined}
        />
      ))}
    </div>
  );
}
