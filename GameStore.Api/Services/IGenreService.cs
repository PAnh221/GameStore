using GameStore.Api.Dtos;

namespace GameStore.Api.Services;

public interface IGenreService
{
	Task<List<GenreDto>> GetGenresAsync();
	Task<GenreDto?> GetGenreAsync(int id);
	Task<GenreResult> CreateGenreAsync(CreateGenreDto newGenre);
	Task<GenreResult> UpdateGenreAsync(int id, UpdateGenreDto updatedGenre);
	Task<GenreError> DeleteGenreAsync(int id);
}
