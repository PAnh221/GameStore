using GameStore.Api.Dtos;

namespace GameStore.Api.Services;

public enum GenreError
{
	None,
	NotFound,
	DuplicateName,
	HasGames
}

public record GenreResult(GenreDto? Genre, GenreError Error);
