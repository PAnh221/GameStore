using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Services;

public class GameService(GameStoreContext dbContext, ILogger<GameService> logger) : IGameService
{
	public async Task<PagedResult<GameSummaryDto>> GetGamesAsync(int page, int pageSize, int? genreId, string? search)
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

		return new PagedResult<GameSummaryDto>(page, pageSize, totalPages, totalGames, games);
	}

	public async Task<GameDetailsDto?> GetGameAsync(int id)
	{
		Game? game = await dbContext.Games
			.Include(game => game.Platforms)
			.AsNoTracking()
			.FirstOrDefaultAsync(game => game.Id == id);

		return game?.ToGameDetailsDto();
	}

	public async Task<GameResult> CreateGameAsync(CreateGameDto newGame)
	{
		var genreExists = await dbContext.Genres.AnyAsync(genre => genre.Id == newGame.GenreId);

		if (!genreExists)
		{
			logger.LogWarning("Create game rejected: genre {GenreId} does not exist", newGame.GenreId);
			return new GameResult(null, GameError.InvalidGenre);
		}

		var platforms = await LoadPlatformsAsync(newGame.PlatformIds);

		if (platforms is null)
		{
			logger.LogWarning("Create game rejected: one or more platforms in {PlatformIds} do not exist", newGame.PlatformIds);
			return new GameResult(null, GameError.InvalidPlatforms);
		}

		Game game = newGame.ToEntity();
		game.Platforms = platforms;

		dbContext.Games.Add(game);
		await dbContext.SaveChangesAsync();

		logger.LogInformation("Created game {GameId} '{GameName}'", game.Id, game.Name);

		return new GameResult(game.ToGameDetailsDto(), GameError.None);
	}

	public async Task<bool> DeleteGameAsync(int id)
	{
		var deleteCount = await dbContext.Games
			.Where(game => game.Id == id)
			.ExecuteDeleteAsync();

		if (deleteCount == 0)
		{
			return false;
		}

		logger.LogInformation("Deleted game {GameId}", id);
		return true;
	}

	public async Task<GameResult> UpdateGameAsync(int id, UpdateGameDto updatedGame)
	{
		var existingGame = await dbContext.Games
			.Include(game => game.Platforms)
			.FirstOrDefaultAsync(game => game.Id == id);

		if (existingGame is null)
		{
			return new GameResult(null, GameError.NotFound);
		}

		var genreExists = await dbContext.Genres.AnyAsync(genre => genre.Id == updatedGame.GenreId);

		if (!genreExists)
		{
			logger.LogWarning("Update game {GameId} rejected: genre {GenreId} does not exist", id, updatedGame.GenreId);
			return new GameResult(null, GameError.InvalidGenre);
		}

		var platforms = await LoadPlatformsAsync(updatedGame.PlatformIds);

		if (platforms is null)
		{
			logger.LogWarning("Update game {GameId} rejected: one or more platforms in {PlatformIds} do not exist", id, updatedGame.PlatformIds);
			return new GameResult(null, GameError.InvalidPlatforms);
		}

		dbContext.Entry(existingGame)
			.CurrentValues
			.SetValues(updatedGame.ToEntity(id));

		existingGame.Platforms = platforms;

		await dbContext.SaveChangesAsync();

		logger.LogInformation("Updated game {GameId}", id);

		return new GameResult(null, GameError.None);
	}

	private async Task<List<Platform>?> LoadPlatformsAsync(List<int>? platformIds)
	{
		var ids = platformIds ?? [];

		var platforms = await dbContext.Platforms
			.Where(platform => ids.Contains(platform.Id))
			.ToListAsync();

		return platforms.Count == ids.Distinct().Count() ? platforms : null;
	}
}