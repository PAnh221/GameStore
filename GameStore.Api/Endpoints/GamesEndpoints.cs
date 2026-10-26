using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
	const string GetGameEndpointName = "GetGame";

	public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("games")
						.WithParameterValidation();

		// GET /games
		group.MapGet("/", async (
			GameStoreContext dbContext,
			int? genreId,
			string? search,
			int page = 1,
			int pageSize = 10) =>
		{
			page = Math.Max(page, 1);
			pageSize = Math.Clamp(pageSize, 1, 100);

			var query = dbContext.Games.Include(game => game.Genre).AsNoTracking().AsQueryable();

			if (genreId.HasValue)
			{
				query = query.Where(game => game.GenreId == genreId.Value);
			}

			if (!string.IsNullOrWhiteSpace(search))
			{
				query = query.Where(game => game.Name.Contains(search));
			}

			var totalGames = await query.CountAsync();
			var totalPages = (int)Math.Ceiling(totalGames / (double)pageSize);

			var games = await query
				.OrderBy(game => game.Id)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(game => game.ToGameSummaryDto())
				.ToListAsync();

			return Results.Ok(new PagedResult<GameSummaryDto>(page, pageSize, totalPages, totalGames, games));
		});

		// GET /games/1
		group.MapGet("/{id}", async (int id, GameStoreContext dbContext) =>
		{
			Game? game = await dbContext.Games.FindAsync(id);

			return game is null ?
				Results.NotFound() : Results.Ok(game.ToGameDetailsDto());
		})
		.WithName(GetGameEndpointName);

		// POST /games
		group.MapPost("", async (CreateGameDto newGame, GameStoreContext dbContext) =>
		{
			var genreExists = await dbContext.Genres.AnyAsync(genre => genre.Id == newGame.GenreId);

			if (!genreExists)
			{
				return Results.Problem(detail: "Invalid genre ID.", statusCode: StatusCodes.Status400BadRequest);
			}

			Game game = newGame.ToEntity();

			dbContext.Games.Add(game);
			await dbContext.SaveChangesAsync();

			return Results.CreatedAtRoute(GetGameEndpointName,
				new { id = game.Id },
				game.ToGameDetailsDto());
		});

		// PUT /games/1
		group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext dbContext) =>
		{
			var existingGame = await dbContext.Games.FindAsync(id);

			if (existingGame is null)
			{
				return Results.NotFound();
			}

			var genreExists = await dbContext.Genres.AnyAsync(genre => genre.Id == updatedGame.GenreId);

			if (!genreExists)
			{
				return Results.Problem(detail: "Invalid genre ID.", statusCode: StatusCodes.Status400BadRequest);
			}

			dbContext.Entry(existingGame)
				.CurrentValues
				.SetValues(updatedGame.ToEntity(id));

			await dbContext.SaveChangesAsync();

			return Results.NoContent();
		});

		// DELETE /games/1
		group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
		{
			var deleteCount = await dbContext.Games
					.Where(game => game.Id == id)
					.ExecuteDeleteAsync();

			if (deleteCount == 0)
			{
				return Results.NotFound();
			}

			return Results.NoContent();
		});

		return group;
	}
}
