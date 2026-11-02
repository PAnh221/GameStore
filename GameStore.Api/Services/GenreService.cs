using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Services;

public class GenreService(GameStoreContext dbContext, ILogger<GenreService> logger) : IGenreService
{
	public async Task<List<GenreDto>> GetGenresAsync()
	{
		return await dbContext.Genres
			.Select(genre => genre.ToDto())
			.AsNoTracking()
			.ToListAsync();
	}

	public async Task<GenreDto?> GetGenreAsync(int id)
	{
		Genre? genre = await dbContext.Genres.FindAsync(id);

		return genre?.ToDto();
	}

	public async Task<GenreResult> CreateGenreAsync(CreateGenreDto newGenre)
	{
		var genreExists = await dbContext.Genres.AnyAsync(genre => genre.Name == newGenre.Name);

		if (genreExists)
		{
			logger.LogWarning("Create genre rejected: name '{GenreName}' already exists", newGenre.Name);
			return new GenreResult(null, GenreError.DuplicateName);
		}

		Genre genre = newGenre.ToEntity();

		dbContext.Genres.Add(genre);
		await dbContext.SaveChangesAsync();

		logger.LogInformation("Created genre {GenreId} '{GenreName}'", genre.Id, genre.Name);

		return new GenreResult(genre.ToDto(), GenreError.None);
	}

	public async Task<GenreResult> UpdateGenreAsync(int id, UpdateGenreDto updatedGenre)
	{
		var existingGenre = await dbContext.Genres.FindAsync(id);

		if (existingGenre is null)
		{
			return new GenreResult(null, GenreError.NotFound);
		}

		var genreExists = await dbContext.Genres.AnyAsync(genre => genre.Name == updatedGenre.Name && genre.Id != id);

		if (genreExists)
		{
			logger.LogWarning("Update genre {GenreId} rejected: name '{GenreName}' already exists", id, updatedGenre.Name);
			return new GenreResult(null, GenreError.DuplicateName);
		}

		dbContext.Entry(existingGenre)
			.CurrentValues
			.SetValues(updatedGenre.ToEntity(id));

		await dbContext.SaveChangesAsync();

		logger.LogInformation("Updated genre {GenreId}", id);

		return new GenreResult(null, GenreError.None);
	}

	public async Task<GenreError> DeleteGenreAsync(int id)
	{
		var hasGames = await dbContext.Games.AnyAsync(game => game.GenreId == id);

		if (hasGames)
		{
			logger.LogWarning("Delete genre {GenreId} rejected: it has associated games", id);
			return GenreError.HasGames;
		}

		var deleteCount = await dbContext.Genres
			.Where(genre => genre.Id == id)
			.ExecuteDeleteAsync();

		if (deleteCount == 0)
		{
			return GenreError.NotFound;
		}

		logger.LogInformation("Deleted genre {GenreId}", id);

		return GenreError.None;
	}
}
