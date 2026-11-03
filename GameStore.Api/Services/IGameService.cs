using GameStore.Api.Dtos;

namespace GameStore.Api.Services;

public interface IGameService
{
	Task<PagedResult<GameSummaryDto>> GetGamesAsync(int page, int pageSize, int? genreId, string? search);
	Task<GameDetailsDto?> GetGameAsync(int id);
	Task<GameResult> CreateGameAsync(CreateGameDto newGame);
	Task<GameResult> UpdateGameAsync(int id, UpdateGameDto updatedGame);
	Task<bool> DeleteGameAsync(int id);
}
