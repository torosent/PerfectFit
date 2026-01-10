using MediatR;
using PerfectFit.UseCases.Games.Commands;
using PerfectFit.UseCases.Games.DTOs;
using PerfectFit.UseCases.Games.Queries;
using PerfectFit.Web.DTOs;
using System.Security.Claims;

// Alias to disambiguate Web DTOs from UseCases DTOs
using WebPlacePieceRequestDto = PerfectFit.Web.DTOs.PlacePieceRequestDto;
using WebPlacePieceResponseDto = PerfectFit.Web.DTOs.PlacePieceResponseDto;

namespace PerfectFit.Web.Endpoints;

/// <summary>
/// Minimal API endpoints for game operations.
/// </summary>
public static class GameEndpoints
{
    /// <summary>
    /// Maps all game-related endpoints.
    /// </summary>
    public static void MapGameEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/games")
            .WithTags("Games");

        group.MapPost("/", CreateGame)
            .WithName("CreateGame")
            .WithSummary("Create a new game session")
            .Produces<GameStateDto>(StatusCodes.Status201Created);

        group.MapGet("/{id:guid}", GetGame)
            .WithName("GetGame")
            .WithSummary("Get the current state of a game")
            .Produces<GameStateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/place", PlacePiece)
            .WithName("PlacePiece")
            .WithSummary("Place a piece on the game board")
            .Produces<WebPlacePieceResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/end", EndGame)
            .WithName("EndGame")
            .WithSummary("End an active game")
            .Produces<GameEndResponseWebDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateGame(
        IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        // Extract userId from JWT if authenticated
        int? userId = null;
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
        {
            userId = parsedId;
        }

        var command = new CreateGameCommand(UserId: userId);
        var result = await mediator.Send(command, cancellationToken);

        return Results.Created($"/api/games/{result.Id}", result);
    }

    private static async Task<IResult> GetGame(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetGameQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        return result is null
            ? Results.NotFound()
            : Results.Ok(result);
    }

    private static async Task<IResult> PlacePiece(
        Guid id,
        WebPlacePieceRequestDto request,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        // Extract userId from JWT if authenticated
        int? userId = null;
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
        {
            userId = parsedId;
        }

        var command = new PlacePieceCommand(
            GameId: id,
            UserId: userId,
            PieceIndex: request.PieceIndex,
            Row: request.Position.Row,
            Col: request.Position.Col,
            ClientTimestamp: request.ClientTimestamp
        );

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Found)
        {
            return Results.NotFound();
        }

        if (!result.GameActive)
        {
            return Results.BadRequest(new { error = result.RejectionReason ?? "Game is not active" });
        }

        // Anti-cheat rejection
        if (result.RejectionReason is not null)
        {
            return Results.BadRequest(new { error = result.RejectionReason });
        }

        if (result.Response is not null && !result.Response.Success)
        {
            return Results.BadRequest(new { error = "Invalid piece placement" });
        }

        return Results.Ok(result.Response);
    }

    private static async Task<IResult> EndGame(
        Guid id,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        // Extract userId from JWT if authenticated
        int? userId = null;
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
        {
            userId = parsedId;
        }

        var command = new EndGameCommand(id, userId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Found || result.Response is null)
        {
            return Results.NotFound();
        }

        // Map to Web DTOs
        var response = new GameEndResponseWebDto(
            GameState: MapGameStateToWebDto(result.Response.GameState),
            Gamification: result.Response.Gamification is not null 
                ? MapGamificationToWebDto(result.Response.Gamification) 
                : null
        );

        return Results.Ok(response);
    }

    private static GameStateWebDto MapGameStateToWebDto(GameStateDto dto)
    {
        return new GameStateWebDto(
            Id: dto.Id,
            Grid: dto.Grid,
            CurrentPieces: dto.CurrentPieces.Select(p => new PieceWebDto(
                Type: MapPieceTypeToWebDto(p.Type),
                Shape: p.Shape,
                Color: p.Color
            )).ToArray(),
            Score: dto.Score,
            Combo: dto.Combo,
            Status: dto.Status == GameStatusDto.Playing ? GameStatusWebDto.Playing : GameStatusWebDto.Ended,
            LinesCleared: dto.LinesCleared
        );
    }

    private static PieceTypeWebDto MapPieceTypeToWebDto(PieceTypeDto type) => type switch
    {
        PieceTypeDto.I => PieceTypeWebDto.I,
        PieceTypeDto.O => PieceTypeWebDto.O,
        PieceTypeDto.T => PieceTypeWebDto.T,
        PieceTypeDto.S => PieceTypeWebDto.S,
        PieceTypeDto.Z => PieceTypeWebDto.Z,
        PieceTypeDto.J => PieceTypeWebDto.J,
        PieceTypeDto.L => PieceTypeWebDto.L,
        PieceTypeDto.DOT => PieceTypeWebDto.DOT,
        PieceTypeDto.LINE2 => PieceTypeWebDto.LINE2,
        PieceTypeDto.LINE3 => PieceTypeWebDto.LINE3,
        PieceTypeDto.LINE5 => PieceTypeWebDto.LINE5,
        PieceTypeDto.CORNER => PieceTypeWebDto.CORNER,
        PieceTypeDto.BIG_CORNER => PieceTypeWebDto.BIG_CORNER,
        PieceTypeDto.SQUARE_2X2 => PieceTypeWebDto.SQUARE_2X2,
        PieceTypeDto.SQUARE_3X3 => PieceTypeWebDto.SQUARE_3X3,
        PieceTypeDto.RECT_2X3 => PieceTypeWebDto.RECT_2X3,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    private static GameEndGamificationDto MapGamificationToWebDto(GameEndGamificationResponseDto dto)
    {
        return new GameEndGamificationDto(
            Streak: new StreakDto(
                CurrentStreak: dto.Streak.CurrentStreak,
                LongestStreak: dto.Streak.LongestStreak,
                FreezeTokens: dto.Streak.FreezeTokens,
                IsAtRisk: dto.Streak.IsAtRisk,
                ResetTime: dto.Streak.ResetTime
            ),
            ChallengeUpdates: dto.ChallengeUpdates.Select(c => new ChallengeProgressDto(
                ChallengeId: c.ChallengeId,
                ChallengeName: c.ChallengeName,
                NewProgress: c.NewProgress,
                JustCompleted: c.JustCompleted,
                XPEarned: c.XPEarned
            )).ToList(),
            NewAchievements: dto.NewAchievements.Select(a => new AchievementUnlockDto(
                AchievementId: a.AchievementId,
                Name: a.Name,
                Description: a.Description,
                IconUrl: a.IconUrl,
                RewardType: a.RewardType,
                RewardValue: a.RewardValue
            )).ToList(),
            SeasonProgress: new SeasonXPDto(
                XPEarned: dto.SeasonProgress.XPEarned,
                TotalXP: dto.SeasonProgress.TotalXP,
                NewTier: dto.SeasonProgress.NewTier,
                TierUp: dto.SeasonProgress.TierUp,
                NewRewardsCount: dto.SeasonProgress.NewRewardsCount
            ),
            GoalUpdates: dto.GoalUpdates.Select(g => new GoalProgressDto(
                GoalId: g.GoalId,
                Description: g.Description,
                NewProgress: g.NewProgress,
                JustCompleted: g.JustCompleted
            )).ToList()
        );
    }
}
