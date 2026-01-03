using MediatR;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Leaderboard.Queries;

/// <summary>
/// Query to get top scores from the leaderboard.
/// </summary>
/// <param name="Count">Number of entries to retrieve (default: 100).</param>
public record GetTopScoresQuery(int Count = 100) : IRequest<List<LeaderboardEntryResult>>;

/// <summary>
/// Result item for leaderboard entries.
/// </summary>
public record LeaderboardEntryResult(
    int Rank,
    string DisplayName,
    string? Avatar,
    int Score,
    int LinesCleared,
    int MaxCombo,
    DateTime AchievedAt
);

/// <summary>
/// Handler for GetTopScoresQuery.
/// </summary>
public class GetTopScoresQueryHandler : IRequestHandler<GetTopScoresQuery, List<LeaderboardEntryResult>>
{
    private readonly ILeaderboardRepository _leaderboardRepository;

    public GetTopScoresQueryHandler(ILeaderboardRepository leaderboardRepository)
    {
        _leaderboardRepository = leaderboardRepository;
    }

    public async Task<List<LeaderboardEntryResult>> Handle(GetTopScoresQuery request, CancellationToken cancellationToken)
    {
        var count = Math.Clamp(request.Count, 1, 100);
        var entries = await _leaderboardRepository.GetTopScoresAsync(count, cancellationToken);

        var results = new List<LeaderboardEntryResult>();
        var rank = 1;

        foreach (var entry in entries)
        {
            results.Add(new LeaderboardEntryResult(
                Rank: rank++,
                DisplayName: entry.User?.DisplayName ?? "Unknown",
                Avatar: entry.User?.Avatar,
                Score: entry.Score,
                LinesCleared: entry.LinesCleared,
                MaxCombo: entry.MaxCombo,
                AchievedAt: entry.AchievedAt
            ));
        }

        return results;
    }
}
