using MediatR;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;
using PerfectFit.UseCases.Leaderboard.Queries;

namespace PerfectFit.UseCases.Leaderboard.Commands;

/// <summary>
/// Command to submit a score to the leaderboard.
/// </summary>
/// <param name="GameSessionId">The game session ID to submit.</param>
/// <param name="UserId">The user submitting the score.</param>
public record SubmitScoreCommand(
    Guid GameSessionId,
    int UserId
) : IRequest<SubmitScoreResult>;

/// <summary>
/// Result of score submission.
/// </summary>
public record SubmitScoreResult(
    bool Success,
    string? ErrorMessage,
    LeaderboardEntryResult? Entry,
    bool IsNewHighScore,
    int? NewRank
);

/// <summary>
/// Handler for SubmitScoreCommand.
/// </summary>
public class SubmitScoreCommandHandler : IRequestHandler<SubmitScoreCommand, SubmitScoreResult>
{
    private readonly IScoreValidationService _scoreValidationService;
    private readonly ILeaderboardRepository _leaderboardRepository;
    private readonly IUserRepository _userRepository;

    public SubmitScoreCommandHandler(
        IScoreValidationService scoreValidationService,
        ILeaderboardRepository leaderboardRepository,
        IUserRepository userRepository)
    {
        _scoreValidationService = scoreValidationService;
        _leaderboardRepository = leaderboardRepository;
        _userRepository = userRepository;
    }

    public async Task<SubmitScoreResult> Handle(SubmitScoreCommand request, CancellationToken cancellationToken)
    {
        // Validate the score submission
        var validationResult = await _scoreValidationService.ValidateScoreAsync(
            request.GameSessionId,
            request.UserId,
            claimedScore: null,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return new SubmitScoreResult(
                Success: false,
                ErrorMessage: validationResult.ErrorMessage,
                Entry: null,
                IsNewHighScore: false,
                NewRank: null);
        }

        var session = validationResult.GameSession!;
        
        // Don't submit zero scores
        if (session.Score <= 0)
        {
            return new SubmitScoreResult(
                Success: false,
                ErrorMessage: "Cannot submit a score of zero.",
                Entry: null,
                IsNewHighScore: false,
                NewRank: null);
        }

        // Get user for high score check
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return new SubmitScoreResult(
                Success: false,
                ErrorMessage: "User not found.",
                Entry: null,
                IsNewHighScore: false,
                NewRank: null);
        }

        var isNewHighScore = session.Score > user.HighScore;

        // Create leaderboard entry
        var entry = LeaderboardEntry.Create(
            userId: request.UserId,
            score: session.Score,
            linesCleared: session.LinesCleared,
            maxCombo: session.MaxCombo,
            gameSessionId: session.Id
        );

        await _leaderboardRepository.AddAsync(entry, cancellationToken);

        // Update user high score if this is a new record
        if (isNewHighScore)
        {
            user.UpdateHighScore(session.Score);
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        // Get the new rank
        var newRank = await _leaderboardRepository.GetUserRankAsync(request.UserId, cancellationToken);

        var entryResult = new LeaderboardEntryResult(
            Rank: newRank,
            DisplayName: user.DisplayName,
            Score: entry.Score,
            LinesCleared: entry.LinesCleared,
            MaxCombo: entry.MaxCombo,
            AchievedAt: entry.AchievedAt
        );

        return new SubmitScoreResult(
            Success: true,
            ErrorMessage: null,
            Entry: entryResult,
            IsNewHighScore: isNewHighScore,
            NewRank: newRank);
    }
}
