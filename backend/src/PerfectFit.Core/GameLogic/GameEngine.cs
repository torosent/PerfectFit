using PerfectFit.Core.GameLogic.Board;
using PerfectFit.Core.GameLogic.Generation;
using PerfectFit.Core.GameLogic.PieceBag;
using PerfectFit.Core.GameLogic.Pieces;
using PerfectFit.Core.GameLogic.Scoring;

namespace PerfectFit.Core.GameLogic;

/// <summary>
/// Main game engine that orchestrates all game logic.
/// Uses board-aware weighted piece generation for fair gameplay.
/// </summary>
public sealed class GameEngine
{
    private const int PieceHandSize = 3;

    private readonly GameBoard _board;
    private readonly PieceBagGenerator _pieceBag;
    private readonly List<PieceType> _currentPieces;

    /// <summary>
    /// Gets the current score.
    /// </summary>
    public int Score { get; private set; }

    /// <summary>
    /// Gets the current combo count.
    /// </summary>
    public int Combo { get; private set; }

    /// <summary>
    /// Gets the total number of lines cleared in this game.
    /// </summary>
    public int TotalLinesCleared { get; private set; }

    /// <summary>
    /// Gets the maximum combo achieved in this game.
    /// </summary>
    public int MaxCombo { get; private set; }

    /// <summary>
    /// Gets whether the game is over (no valid moves remaining).
    /// </summary>
    public bool IsGameOver { get; private set; }

    /// <summary>
    /// Gets the current pieces available to place.
    /// </summary>
    public IReadOnlyList<PieceType> CurrentPieces => _currentPieces.AsReadOnly();

    /// <summary>
    /// Gets the current board analysis for UI display (danger level, etc).
    /// </summary>
    public BoardAnalyzer.BoardAnalysis GetBoardAnalysis() => BoardAnalyzer.Analyze(_board);

    /// <summary>
    /// Creates a new game engine with an optional seed for deterministic behavior.
    /// </summary>
    /// <param name="seed">Optional seed for reproducible games.</param>
    /// <param name="useWeightedGeneration">If true, uses board-aware weighted piece generation. Default is true.</param>
    public GameEngine(int? seed = null, bool useWeightedGeneration = true)
    {
        _board = new GameBoard();
        _pieceBag = new PieceBagGenerator(seed, useWeightedGeneration);
        _currentPieces = [];

        Score = 0;
        Combo = 0;
        TotalLinesCleared = 0;
        MaxCombo = 0;
        IsGameOver = false;

        // Draw initial pieces (board-aware)
        DrawPieces(PieceHandSize);
    }

    private GameEngine(
        GameBoard board,
        PieceBagGenerator pieceBag,
        List<PieceType> currentPieces,
        int score,
        int combo,
        int totalLinesCleared,
        int maxCombo)
    {
        _board = board;
        _pieceBag = pieceBag;
        _currentPieces = currentPieces;
        Score = score;
        Combo = combo;
        TotalLinesCleared = totalLinesCleared;
        MaxCombo = maxCombo;
        IsGameOver = CheckGameOver();
    }

    /// <summary>
    /// Checks if a piece can be placed at the specified position.
    /// </summary>
    /// <param name="pieceIndex">Index of the piece in current pieces (0-2).</param>
    /// <param name="row">Target row on the board.</param>
    /// <param name="col">Target column on the board.</param>
    /// <returns>True if the piece can be placed.</returns>
    public bool CanPlacePiece(int pieceIndex, int row, int col)
    {
        if (pieceIndex < 0 || pieceIndex >= _currentPieces.Count)
            return false;

        var piece = Piece.Create(_currentPieces[pieceIndex]);
        return _board.CanPlacePiece(piece, row, col);
    }

    /// <summary>
    /// Attempts to place a piece at the specified position.
    /// </summary>
    /// <param name="pieceIndex">Index of the piece in current pieces (0-2).</param>
    /// <param name="row">Target row on the board.</param>
    /// <param name="col">Target column on the board.</param>
    /// <returns>Result of the placement attempt.</returns>
    public PlacementResult PlacePiece(int pieceIndex, int row, int col)
    {
        // Validate piece index
        if (pieceIndex < 0 || pieceIndex >= _currentPieces.Count)
        {
            return new PlacementResult(
                Success: false,
                PointsEarned: 0,
                LinesCleared: 0,
                NewCombo: Combo,
                IsGameOver: IsGameOver,
                ClearResult: null);
        }

        var pieceType = _currentPieces[pieceIndex];
        var piece = Piece.Create(pieceType);

        // Try to place the piece
        if (!_board.TryPlacePiece(piece, row, col))
        {
            return new PlacementResult(
                Success: false,
                PointsEarned: 0,
                LinesCleared: 0,
                NewCombo: Combo,
                IsGameOver: IsGameOver,
                ClearResult: null);
        }

        // Piece placed successfully - remove from hand
        _currentPieces.RemoveAt(pieceIndex);

        // Check for line clears
        var clearResult = LineClearer.ClearLines(_board);
        var linesCleared = clearResult.TotalLinesCleared;

        // Calculate score
        int pointsEarned = 0;
        if (linesCleared > 0)
        {
            pointsEarned = ScoreCalculator.CalculatePoints(linesCleared, Combo);
            Score += pointsEarned;
            TotalLinesCleared += linesCleared;
            Combo++;

            if (Combo > MaxCombo)
            {
                MaxCombo = Combo;
            }
        }
        else
        {
            // No lines cleared - reset combo
            Combo = 0;
        }

        // Only draw new pieces when all 3 pieces have been placed (turn complete)
        bool newTurnStarted = false;
        if (_currentPieces.Count == 0)
        {
            DrawPieces(PieceHandSize);
            newTurnStarted = true;
        }

        // Check if game is over
        IsGameOver = CheckGameOver();

        return new PlacementResult(
            Success: true,
            PointsEarned: pointsEarned,
            LinesCleared: linesCleared,
            NewCombo: Combo,
            IsGameOver: IsGameOver,
            ClearResult: linesCleared > 0 ? clearResult : null,
            PiecesRemainingInTurn: _currentPieces.Count,
            NewTurnStarted: newTurnStarted);
    }

    /// <summary>
    /// Gets the current game state for persistence.
    /// </summary>
    /// <returns>A GameState record with all game data.</returns>
    public GameState GetState()
    {
        return new GameState(
            BoardGrid: _board.ToArray(),
            CurrentPieceTypes: new List<PieceType>(_currentPieces),
            PieceBagState: _pieceBag.SerializeState(),
            Score: Score,
            Combo: Combo,
            TotalLinesCleared: TotalLinesCleared,
            MaxCombo: MaxCombo);
    }

    /// <summary>
    /// Creates a GameEngine from a saved state.
    /// </summary>
    /// <param name="state">The saved game state.</param>
    /// <returns>A new GameEngine instance.</returns>
    public static GameEngine FromState(GameState state)
    {
        var board = GameBoard.FromArray(state.BoardGrid);
        var pieceBag = PieceBagGenerator.FromState(state.PieceBagState);
        var currentPieces = new List<PieceType>(state.CurrentPieceTypes);

        return new GameEngine(
            board,
            pieceBag,
            currentPieces,
            state.Score,
            state.Combo,
            state.TotalLinesCleared,
            state.MaxCombo);
    }

    private void DrawPieces(int count)
    {
        // Use board-aware generation
        var newPieces = _pieceBag.GetNextPieces(count, _board, TotalLinesCleared);
        _currentPieces.AddRange(newPieces);
    }

    private bool CheckGameOver()
    {
        // Game is over if none of the current pieces can be placed anywhere
        foreach (var pieceType in _currentPieces)
        {
            var piece = Piece.Create(pieceType);
            if (_board.CanPlacePieceAnywhere(piece))
            {
                return false;
            }
        }

        return true;
    }
}
