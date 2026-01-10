# Project Overview

## What is PerfectFit?

PerfectFit is a full-stack grid-based block placement puzzle game inspired by classic block puzzle games. Players strategically place tetromino-like pieces on a 8x8 grid to clear lines and achieve high scores.

## Core Features

### Gameplay
- **8x8 Game Grid**: Strategic placement area
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

### Gamification System

A comprehensive engagement system designed to keep players motivated and rewarded.

#### Daily & Weekly Challenges
- **Daily Challenges**: 3 new challenges every day (reset at midnight UTC)
- **Weekly Challenges**: 3 larger challenges every Monday
- **Challenge Types**: Score targets, line clears, combo achievements, accuracy goals
- **XP Rewards**: Earn experience points for completing challenges

#### Streak System
- **Daily Streaks**: Maintain consecutive days of play
- **Streak Freeze Tokens**: Protect your streak when you can't play
- **Streak Rewards**: Bonus XP multipliers for longer streaks
- **Timezone-Aware**: Streaks reset based on local timezone preference

#### Season Pass
- **7-Day Seasons**: Weekly themed seasons with fresh rewards
- **50 Tiers**: Progress through tiers by earning XP
- **Tier Rewards**: Unlock cosmetics, streak freezes, and XP bonuses
- **Season Archive**: Track your progress across past seasons

#### Achievements & Badges
- **18+ Achievements**: Across 5 categories (Progression, Skill, Social, Collection, Special)
- **Rarity Tiers**: Common, Uncommon, Rare, Epic, Legendary
- **Cosmetic Rewards**: Unlock board themes, avatar frames, and badges
- **Automatic Detection**: Achievements unlock as you play

#### Cosmetics
- **Board Themes**: 8 unique visual styles (Classic, Ocean, Forest, Sunset, Night Sky, Galaxy, etc.)
- **Avatar Frames**: 7 frames with rarity-based designs (Bronze, Silver, Gold, Diamond, Champion)
- **Profile Badges**: 8 badges to showcase your achievements
- **Equipment System**: Mix and match your favorite cosmetics

#### Personal Goals
- **Beat Your Average**: Surpass your historical performance
- **Improve Accuracy**: Achieve higher precision in placements
- **New Personal Best**: Set new records
- **Motivational Prompts**: Goal notifications at game start

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

## Game Algorithm

### Piece Generation
The game uses a sophisticated "Weighted Piece Generator" to ensure fairness and solvability.

*   **Board Analysis**: Before generating pieces, the system analyzes the board to calculate a "Danger Level" (0.0 to 1.0) based on:
    *   Occupancy (percentage of filled cells).
    *   Legal Moves (number of possible placements for common pieces).
    *   Fragmentation (number of near-complete lines).
*   **Dynamic Weights**: Piece probabilities are adjusted based on the Danger Level.
    *   **Safe Board**: Higher chance of large/complex pieces.
    *   **Dangerous Board**: Higher chance of small/simple pieces (1x1 Dot, 1x2 Line).
*   **Solvability Guarantee**: The generator ensures that at least one of the three generated pieces can be placed on the current board. If a generated set is unplayable, it is discarded and regenerated.
*   **Rescue Mechanism**: In critical situations (Danger > 0.7), the system forces the inclusion of a "rescue piece" (small piece) to prevent unfair game-overs.

### Scoring System
*   **Line Clears**: Points are awarded for clearing rows or columns.
    *   1 Line: 10 points
    *   2 Lines: 30 points
    *   3 Lines: 60 points
    *   4 Lines: 100 points
    *   5+ Lines: 150 + (n-5)*50 points
*   **Combo System**: Clearing lines in consecutive turns builds a Combo Multiplier.
    *   Multiplier = 1.0 + (ComboCount * 0.5)
    *   Example: 3rd consecutive clear (Combo 2) gives 2x points.
*   **Total Score**: `(LineBonus * ComboMultiplier)`

### Board Mechanics
*   **Grid**: 8x8 grid.
*   **No Gravity**: Unlike Tetris, blocks do not fall when lines are cleared. They remain in their placed positions.
*   **Clear Logic**: A line is cleared when all 8 cells in a row or column are filled. Multiple lines can be cleared simultaneously.

## Project Goals

- **Performance**: Sub-100ms API response times
- **Accessibility**: Keyboard navigation and screen reader support
- **Scalability**: Stateless backend design for horizontal scaling
- **Security**: Server-side game state validation with multi-layer anti-cheat
- **Cross-Platform**: Web-based, works on desktop and mobile browsers
- **Engagement**: Gamification system with challenges, streaks, and rewards
- **Retention**: Season pass and achievement systems to encourage daily play
