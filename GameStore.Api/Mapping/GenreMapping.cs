using GameStore.Api.Dtos;
using GameStore.Api.Entities;

namespace GameStore.Api.Mapping;

public static class GenreMapping
{
	public static Genre ToEntity(this CreateGenreDto createGenreDto)
	{
		return new Genre()
		{
			Name = createGenreDto.Name
		};
	}

	public static Genre ToEntity(this UpdateGenreDto updateGenreDto, int id)
	{
		return new Genre()
		{
			Id = id,
			Name = updateGenreDto.Name
		};
	}

	public static GenreDto ToDto(this Genre genre)
	{
		return new GenreDto(genre.Id, genre.Name);
	}
}
