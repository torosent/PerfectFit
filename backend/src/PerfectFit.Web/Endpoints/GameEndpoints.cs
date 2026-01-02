using MediatR;
using PerfectFit.UseCases.Games.Commands;
using PerfectFit.UseCases.Games.DTOs;
using PerfectFit.UseCases.Games.Queries;

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
            .Produces<PlacePieceResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/end", EndGame)
            .WithName("EndGame")
            .WithSummary("End an active game")
            .Produces<GameStateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateGame(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        // TODO: Extract userId from JWT when authentication is implemented
        var command = new CreateGameCommand(UserId: null);
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
        PlacePieceRequestDto request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new PlacePieceCommand(
            GameId: id,
            PieceIndex: request.PieceIndex,
            Row: request.Position.Row,
            Col: request.Position.Col
        );
        
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Found)
        {
            return Results.NotFound();
        }

        if (!result.GameActive)
        {
            return Results.BadRequest(new { error = "Game is not active" });
        }

        if (result.Response is not null && !result.Response.Success)
        {
            return Results.BadRequest(new { error = "Invalid piece placement" });
        }

        return Results.Ok(result.Response);
    }

    private static async Task<IResult> EndGame(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new EndGameCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        
        return !result.Found 
            ? Results.NotFound() 
            : Results.Ok(result.GameState);
    }
}
