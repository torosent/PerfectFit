using MediatR;
using PerfectFit.UseCases.Leaderboard.Commands;
using PerfectFit.UseCases.Leaderboard.Queries;
using PerfectFit.Web.DTOs;
using System.Security.Claims;

namespace PerfectFit.Web.Endpoints;

/// <summary>
/// Minimal API endpoints for leaderboard operations.
/// </summary>
public static class LeaderboardEndpoints
{
    /// <summary>
    /// Maps all leaderboard-related endpoints.
    /// </summary>
    public static void MapLeaderboardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/leaderboard")
            .WithTags("Leaderboard");

        // GET /api/leaderboard - Get top scores (public)
        group.MapGet("/", GetTopScores)
            .WithName("GetTopScores")
            .WithSummary("Get top scores from the leaderboard")
            .Produces<List<LeaderboardEntryDto>>(StatusCodes.Status200OK);

        // GET /api/leaderboard/me - Get current user's stats (auth required)
        group.MapGet("/me", GetMyStats)
            .RequireAuthorization()
            .WithName("GetMyStats")
            .WithSummary("Get current user's leaderboard statistics")
            .Produces<UserStatsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/leaderboard/submit - Submit score (auth required)
        group.MapPost("/submit", SubmitScore)
            .RequireAuthorization()
            .WithName("SubmitScore")
            .WithSummary("Submit a game score to the leaderboard")
            .Produces<SubmitScoreResponseDto>(StatusCodes.Status200OK)
            .Produces<SubmitScoreResponseDto>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> GetTopScores(
        IMediator mediator,
        int? count = null,
        CancellationToken cancellationToken = default)
    {
        var limit = count ?? 100;
        limit = Math.Clamp(limit, 1, 100);

        var query = new GetTopScoresQuery(limit);
        var results = await mediator.Send(query, cancellationToken);

        var dtos = results.Select(r => new LeaderboardEntryDto(
            Rank: r.Rank,
            DisplayName: r.DisplayName,
            Avatar: r.Avatar,
            Score: r.Score,
            LinesCleared: r.LinesCleared,
            MaxCombo: r.MaxCombo,
            AchievedAt: r.AchievedAt
        )).ToList();

        return Results.Ok(dtos);
    }

    private static async Task<IResult> GetMyStats(
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var query = new GetUserStatsQuery(userId);
        var result = await mediator.Send(query, cancellationToken);

        if (result is null)
        {
            return Results.NotFound();
        }

        var dto = new UserStatsDto(
            HighScore: result.HighScore,
            GamesPlayed: result.GamesPlayed,
            GlobalRank: result.GlobalRank,
            BestGame: result.BestGame is null ? null : new LeaderboardEntryDto(
                Rank: result.BestGame.Rank,
                DisplayName: result.BestGame.DisplayName,
                Avatar: result.BestGame.Avatar,
                Score: result.BestGame.Score,
                LinesCleared: result.BestGame.LinesCleared,
                MaxCombo: result.BestGame.MaxCombo,
                AchievedAt: result.BestGame.AchievedAt
            )
        );

        return Results.Ok(dto);
    }

    private static async Task<IResult> SubmitScore(
        SubmitScoreRequestDto request,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Check if user is a guest (guests cannot submit to leaderboard for persistence)
        var providerClaim = user.FindFirst("provider")?.Value;
        if (providerClaim?.Equals("guest", StringComparison.OrdinalIgnoreCase) == true)
        {
            return Results.Json(
                new SubmitScoreResponseDto(
                    Success: false,
                    ErrorMessage: "Guest users cannot submit scores to the leaderboard.",
                    Entry: null,
                    IsNewHighScore: false,
                    NewRank: null
                ),
                statusCode: StatusCodes.Status403Forbidden
            );
        }

        var command = new SubmitScoreCommand(request.GameSessionId, userId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return Results.BadRequest(new SubmitScoreResponseDto(
                Success: false,
                ErrorMessage: result.ErrorMessage,
                Entry: null,
                IsNewHighScore: false,
                NewRank: null
            ));
        }

        return Results.Ok(new SubmitScoreResponseDto(
            Success: true,
            ErrorMessage: null,
            Entry: result.Entry is null ? null : new LeaderboardEntryDto(
                Rank: result.Entry.Rank,
                DisplayName: result.Entry.DisplayName,
                Avatar: result.Entry.Avatar,
                Score: result.Entry.Score,
                LinesCleared: result.Entry.LinesCleared,
                MaxCombo: result.Entry.MaxCombo,
                AchievedAt: result.Entry.AchievedAt
            ),
            IsNewHighScore: result.IsNewHighScore,
            NewRank: result.NewRank
        ));
    }
}
