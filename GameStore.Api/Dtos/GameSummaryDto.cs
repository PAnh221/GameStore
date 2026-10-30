namespace GameStore.Api.Dtos;

public record class GameSummaryDto(
	int Id,
	string Name,
	string Genre,
	List<string> Platforms,
	decimal Price,
	DateOnly ReleaseDate);
