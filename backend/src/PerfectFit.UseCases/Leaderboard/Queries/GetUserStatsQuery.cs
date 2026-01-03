using MediatR;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Leaderboard.Queries;

/// <summary>
/// Query to get user statistics for the leaderboard.
/// </summary>
/// <param name="UserId">The user ID to get stats for.</param>
public record GetUserStatsQuery(int UserId) : IRequest<UserStatsResult?>;

/// <summary>
/// Result containing user's leaderboard statistics.
/// </summary>
public record UserStatsResult(
    int HighScore,
    int GamesPlayed,
    int? GlobalRank,
    LeaderboardEntryResult? BestGame
);

/// <summary>
/// Handler for GetUserStatsQuery.
/// </summary>
public class GetUserStatsQueryHandler : IRequestHandler<GetUserStatsQuery, UserStatsResult?>
{
    private readonly IUserRepository _userRepository;
    private readonly ILeaderboardRepository _leaderboardRepository;

    public GetUserStatsQueryHandler(
        IUserRepository userRepository,
        ILeaderboardRepository leaderboardRepository)
    {
        _userRepository = userRepository;
        _leaderboardRepository = leaderboardRepository;
    }

    public async Task<UserStatsResult?> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        // Get user's best leaderboard entry
        var bestEntry = await _leaderboardRepository.GetUserBestScoreAsync(request.UserId, cancellationToken);

        // Get user's rank (0 if no scores)
        var rank = await _leaderboardRepository.GetUserRankAsync(request.UserId, cancellationToken);

        LeaderboardEntryResult? bestGame = null;
        if (bestEntry is not null)
        {
            bestGame = new LeaderboardEntryResult(
                Rank: rank,
                DisplayName: user.Username,
                Avatar: user.Avatar,
                Score: bestEntry.Score,
                LinesCleared: bestEntry.LinesCleared,
                MaxCombo: bestEntry.MaxCombo,
                AchievedAt: bestEntry.AchievedAt
            );
        }

        return new UserStatsResult(
            HighScore: user.HighScore,
            GamesPlayed: user.GamesPlayed,
            GlobalRank: rank > 0 ? rank : null,
            BestGame: bestGame
        );
    }
}
