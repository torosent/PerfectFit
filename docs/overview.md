# Project Overview

## What is PerfectFit?

PerfectFit is a full-stack grid-based block placement puzzle game inspired by classic block puzzle games. Players strategically place tetromino-like pieces on a 10x10 grid to clear lines and achieve high scores.

## Core Features

### Gameplay
- **10x10 Game Grid**: Strategic placement area
- **Multiple Piece Types**: 15+ unique piece shapes including:
  - Tetrominoes (I, O, T, S, Z, J, L)
  - Lines (DOT, LINE2, LINE3, LINE5)
  - Corners (CORNER, BIG_CORNER)
  - Squares (SQUARE_2X2, SQUARE_3X3)
- **Combo System**: Chain line clears for bonus points
- **Drag-and-Drop**: Intuitive piece placement with @dnd-kit
- **Real-time Animations**: Smooth visual feedback using Motion

### User Management
- **Local Authentication**: Email/password registration with email verification
- **Microsoft OAuth**: Sign in with Microsoft account
- **Guest Play**: Instant play without registration
- **User Profiles**: Track high scores and games played
- **Session Management**: JWT-based authentication
- **Security**: Account lockout, rate limiting, BCrypt password hashing

### Leaderboard
- **Global Rankings**: Compete with other players
- **Personal Stats**: Track your progress
- **Score Validation**: Server-side verification prevents cheating

### Anti-Cheat System
- **Rate Limiting**: Minimum 50ms between moves to prevent automation
- **Score Plausibility**: Mathematical validation of score/lines/combo relationships
- **Move History**: Server records all moves for integrity verification
- **Game Duration**: Validates minimum playtime before score submission
- **Input Validation**: Bounds checking on all client inputs
- **Client Fingerprinting**: Optional client identification for anomaly detection

## Game Rules

1. **Turns**: Each turn, you receive 3 random pieces
2. **Placement**: Place all 3 pieces before receiving new ones
3. **Line Clearing**: Complete rows or columns to clear them
4. **Scoring**: Points awarded based on lines cleared and combo multiplier
5. **Game Over**: When no pieces can be placed

## Project Goals

- **Performance**: Sub-100ms API response times
- **Accessibility**: Keyboard navigation and screen reader support
- **Scalability**: Stateless backend design for horizontal scaling
- **Security**: Server-side game state validation with multi-layer anti-cheat
- **Cross-Platform**: Web-based, works on desktop and mobile browsers
