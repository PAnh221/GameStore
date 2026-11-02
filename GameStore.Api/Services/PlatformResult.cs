using GameStore.Api.Dtos;

namespace GameStore.Api.Services;

public enum PlatformError
{
	None,
	NotFound,
	DuplicateName,
	HasGames
}

public record PlatformResult(PlatformDto? Platform, PlatformError Error);
