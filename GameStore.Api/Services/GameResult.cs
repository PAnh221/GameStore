using GameStore.Api.Dtos;

namespace GameStore.Api.Services;

public enum GameError
{
	None,
	NotFound,
	InvalidGenre,
	InvalidPlatforms
}

public record GameResult(GameDetailsDto? Game, GameError Error);
