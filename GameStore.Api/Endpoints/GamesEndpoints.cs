using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
	const string GetGameEndpointName = "GetGame";

	public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app)
	{
		var logger = app.Services
			.GetRequiredService<ILoggerFactory>()
			.CreateLogger("GamesEndpoints");

		var group = app.MapGroup("games")
						.WithParameterValidation();

		// GET /games
		group.MapGet("/", async Task<Ok<PagedResult<GameSummaryDto>>> (
			GameStoreContext dbContext,
			int? genreId,
			string? search,
			int page = 1,
			int pageSize = 10) =>
		{
			page = Math.Max(page, 1);
			pageSize = Math.Clamp(pageSize, 1, 100);

			var query = dbContext.Games
				.Include(game => game.Genre)
				.Include(game => game.Platforms).AsNoTracking();

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

			return TypedResults.Ok(new PagedResult<GameSummaryDto>(page, pageSize, totalPages, totalGames, games));
		});

		// GET /games/1
		group.MapGet("/{id}", async Task<Results<Ok<GameDetailsDto>, NotFound>> (int id, GameStoreContext dbContext) =>
		{
			Game? game = await dbContext.Games
				.Include(game => game.Platforms)
				.AsNoTracking()
				.FirstOrDefaultAsync(game => game.Id == id);

			return game is null ?
				TypedResults.NotFound() : TypedResults.Ok(game.ToGameDetailsDto());
		})
		.WithName(GetGameEndpointName);

		// POST /games
		group.MapPost("", async Task<Results<CreatedAtRoute<GameDetailsDto>, ProblemHttpResult>> (CreateGameDto newGame, GameStoreContext dbContext) =>
		{
			var genreExists = await dbContext.Genres.AnyAsync(genre => genre.Id == newGame.GenreId);

			if (!genreExists)
			{
				logger.LogWarning("Create game rejected: genre {GenreId} does not exist", newGame.GenreId);
				return TypedResults.Problem(detail: "Invalid genre ID.", statusCode: StatusCodes.Status400BadRequest);
			}

			var platformIds = newGame.PlatformIds ?? [];
			var platforms = await dbContext
				.Platforms
				.Where(platform => platformIds.Contains(platform.Id))
				.ToListAsync();

			if (platforms.Count != platformIds.Distinct().Count())
			{
				logger.LogWarning("Create game rejected: one or more platforms in {PlatformIds} do not exist", platformIds);
				return TypedResults.Problem(detail: "Invalid platform IDs.", statusCode: StatusCodes.Status400BadRequest);
			}

			Game game = newGame.ToEntity();
			game.Platforms = platforms;

			dbContext.Games.Add(game);
			await dbContext.SaveChangesAsync();

			logger.LogInformation("Created game {GameId} '{GameName}'", game.Id, game.Name);

			return TypedResults.CreatedAtRoute(
				game.ToGameDetailsDto(),
				GetGameEndpointName,
				new { id = game.Id });
		});

		// PUT /games/1
		group.MapPut("/{id}", async Task<Results<NoContent, ProblemHttpResult, NotFound>> (int id, UpdateGameDto updatedGame, GameStoreContext dbContext) =>
		{
			var existingGame = await dbContext.Games
				.Include(g => g.Platforms)
				.FirstOrDefaultAsync(g => g.Id == id);

			if (existingGame is null)
			{
				return TypedResults.NotFound();
			}

			var genreExists = await dbContext.Genres.AnyAsync(genre => genre.Id == updatedGame.GenreId);

			if (!genreExists)
			{
				logger.LogWarning("Update game {GameId} rejected: genre {GenreId} does not exist", id, updatedGame.GenreId);
				return TypedResults.Problem(detail: "Invalid genre ID.", statusCode: StatusCodes.Status400BadRequest);
			}

			var platformIds = updatedGame.PlatformIds ?? [];
			var platforms = await dbContext
				.Platforms
				.Where(platform => platformIds.Contains(platform.Id))
				.ToListAsync();

			if (platforms.Count != platformIds.Distinct().Count())
			{
				logger.LogWarning("Update game {GameId} rejected: one or more platforms in {PlatformIds} do not exist", id, platformIds);
				return TypedResults.Problem(detail: "Invalid platform IDs.", statusCode: StatusCodes.Status400BadRequest);
			}

			dbContext.Entry(existingGame)
				.CurrentValues
				.SetValues(updatedGame.ToEntity(id));

			existingGame.Platforms = platforms;

			await dbContext.SaveChangesAsync();

			logger.LogInformation("Updated game {GameId}", id);

			return TypedResults.NoContent();
		});

		// DELETE /games/1
		group.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> (int id, GameStoreContext dbContext) =>
		{
			var deleteCount = await dbContext.Games
					.Where(game => game.Id == id)
					.ExecuteDeleteAsync();

			if (deleteCount == 0)
			{
				return TypedResults.NotFound();
			}

			logger.LogInformation("Deleted game {GameId}", id);

			return TypedResults.NoContent();
		});

		return group;
	}
}
