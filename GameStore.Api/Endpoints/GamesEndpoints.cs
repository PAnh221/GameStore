using GameStore.Api.Dtos;
using GameStore.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
	const string GetGameEndpointName = "GetGame";

	public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("games")
						.WithParameterValidation();

		// GET /games
		group.MapGet("/", async Task<Ok<PagedResult<GameSummaryDto>>> (
			IGameService gameService,
			int? genreId,
			string? search,
			int page = 1,
			int pageSize = 10) =>
		{
			return TypedResults.Ok(await gameService.GetGamesAsync(page, pageSize, genreId, search));
		});

		// GET /games/1
		group.MapGet("/{id}", async Task<Results<Ok<GameDetailsDto>, NotFound>> (int id, IGameService gameService) =>
		{
			GameDetailsDto? game = await gameService.GetGameAsync(id);

			return game is null ?
				TypedResults.NotFound() : TypedResults.Ok(game);
		})
		.WithName(GetGameEndpointName);

		// POST /games
		group.MapPost("", async Task<Results<CreatedAtRoute<GameDetailsDto>, ProblemHttpResult>> (CreateGameDto newGame, IGameService gameService) =>
		{
			var result = await gameService.CreateGameAsync(newGame);

			return result.Error switch
			{
				GameError.InvalidGenre => TypedResults.Problem(detail: "Invalid genre ID.", statusCode: StatusCodes.Status400BadRequest),
				GameError.InvalidPlatforms => TypedResults.Problem(detail: "Invalid platform IDs.", statusCode: StatusCodes.Status400BadRequest),
				_ => TypedResults.CreatedAtRoute(result.Game!, GetGameEndpointName, new { id = result.Game!.Id })
			};
		});

		// PUT /games/1
		group.MapPut("/{id}", async Task<Results<NoContent, ProblemHttpResult, NotFound>> (int id, UpdateGameDto updatedGame, IGameService gameService) =>
		{
			var result = await gameService.UpdateGameAsync(id, updatedGame);

			return result.Error switch
			{
				GameError.InvalidGenre => TypedResults.Problem(detail: "Invalid genre ID.", statusCode: StatusCodes.Status400BadRequest),
				GameError.InvalidPlatforms => TypedResults.Problem(detail: "Invalid platform IDs.", statusCode: StatusCodes.Status400BadRequest),
				GameError.NotFound => TypedResults.NotFound(),
				_ => TypedResults.NoContent(),
			};
		});

		// DELETE /games/1
		group.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> (int id, IGameService gameService) =>
		{
			var isDeleted = await gameService.DeleteGameAsync(id);

			return isDeleted ? TypedResults.NoContent() : TypedResults.NotFound();
		});

		return group;
	}
}
