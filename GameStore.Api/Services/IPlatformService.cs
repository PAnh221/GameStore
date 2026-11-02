using GameStore.Api.Dtos;

namespace GameStore.Api.Services;

public interface IPlatformService
{
	Task<List<PlatformDto>> GetPlatformsAsync();
	Task<PlatformDto?> GetPlatformAsync(int id);
	Task<PlatformResult> CreatePlatformAsync(CreatePlatformDto newPlatform);
	Task<PlatformResult> UpdatePlatformAsync(int id, UpdatePlatformDto updatedPlatform);
	Task<PlatformError> DeletePlatformAsync(int id);
}
