namespace GameStore.Api.Dtos;

public record class GameDetailsDto(
	int Id,
	string Name,
	int GenreId,
	List<int> PlatformIds,
	decimal Price,
	DateOnly ReleaseDate);
