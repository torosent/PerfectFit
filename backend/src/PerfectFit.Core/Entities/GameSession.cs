using PerfectFit.Core.Enums;

namespace PerfectFit.Core.Entities;

public class GameSession
{
    private const string EmptyBoardState = """{"grid":[[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0]]}""";
    private const string InitialPieces = """[]""";
    private const string InitialPieceBag = """{"index":0}""";

    public Guid Id { get; private set; }
    public int? UserId { get; private set; }
    public string BoardState { get; private set; } = string.Empty;
    public string CurrentPieces { get; private set; } = string.Empty;
    public string PieceBagState { get; private set; } = string.Empty;
    public int Score { get; private set; }
    public int Combo { get; private set; }
    public int LinesCleared { get; private set; }
    public int MaxCombo { get; private set; }
    public GameStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }

    // Anti-cheat tracking
    public int MoveCount { get; private set; }
    public DateTime? LastMoveAt { get; private set; }
    public string MoveHistory { get; private set; } = "[]";
    public string ClientFingerprint { get; private set; } = string.Empty;

    // Navigation
    public User? User { get; private set; }

    // Private constructor for EF Core
    private GameSession() { }

    public static GameSession Create(int? userId, string? clientFingerprint = null)
    {
        var now = DateTime.UtcNow;
        return new GameSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BoardState = EmptyBoardState,
            CurrentPieces = InitialPieces,
            PieceBagState = InitialPieceBag,
            Score = 0,
            Combo = 0,
            LinesCleared = 0,
            MaxCombo = 0,
            Status = GameStatus.Playing,
            StartedAt = now,
            EndedAt = null,
            LastActivityAt = now,
            MoveCount = 0,
            LastMoveAt = null,
            MoveHistory = "[]",
            ClientFingerprint = clientFingerprint ?? string.Empty
        };
    }

    public void UpdateBoard(string boardState, string currentPieces, string pieceBagState)
    {
        EnsureGameIsActive();

        BoardState = boardState;
        CurrentPieces = currentPieces;
        PieceBagState = pieceBagState;
        LastActivityAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a move for anti-cheat tracking.
    /// </summary>
    /// <param name="pieceIndex">Index of the piece placed.</param>
    /// <param name="row">Row position.</param>
    /// <param name="col">Column position.</param>
    /// <param name="pointsEarned">Points earned from this move.</param>
    /// <param name="linesCleared">Lines cleared from this move.</param>
    public void RecordMove(int pieceIndex, int row, int col, int pointsEarned, int linesCleared)
    {
        EnsureGameIsActive();

        var now = DateTime.UtcNow;
        MoveCount++;
        LastMoveAt = now;

        // Append to move history (compact format)
        var moveEntry = $"{{\"i\":{pieceIndex},\"r\":{row},\"c\":{col},\"p\":{pointsEarned},\"l\":{linesCleared},\"t\":\"{now:O}\"}}";
        if (MoveHistory == "[]")
        {
            MoveHistory = $"[{moveEntry}]";
        }
        else
        {
            MoveHistory = MoveHistory.Insert(MoveHistory.Length - 1, $",{moveEntry}");
        }
    }

    /// <summary>
    /// Gets the time since the last move in milliseconds.
    /// Returns null if no moves have been made.
    /// </summary>
    public double? GetTimeSinceLastMove()
    {
        if (LastMoveAt == null)
            return null;

        return (DateTime.UtcNow - LastMoveAt.Value).TotalMilliseconds;
    }

    /// <summary>
    /// Gets the total game duration in seconds.
    /// </summary>
    public double GetGameDuration()
    {
        var endTime = EndedAt ?? DateTime.UtcNow;
        return (endTime - StartedAt).TotalSeconds;
    }

    public void AddScore(int points, int linesCleared)
    {
        EnsureGameIsActive();

        Score += points;
        LinesCleared += linesCleared;
        LastActivityAt = DateTime.UtcNow;
    }

    public void UpdateCombo(int combo)
    {
        EnsureGameIsActive();

        Combo = combo;
        if (combo > MaxCombo)
        {
            MaxCombo = combo;
        }
        LastActivityAt = DateTime.UtcNow;
    }

    public void EndGame()
    {
        if (Status != GameStatus.Playing)
        {
            return;
        }

        Status = GameStatus.Ended;
        EndedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
    }

    public void AbandonGame()
    {
        if (Status != GameStatus.Playing)
        {
            return;
        }

        Status = GameStatus.Abandoned;
        EndedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
    }

    private void EnsureGameIsActive()
    {
        if (Status != GameStatus.Playing)
        {
            throw new InvalidOperationException("Game session is not active.");
        }
    }
}
