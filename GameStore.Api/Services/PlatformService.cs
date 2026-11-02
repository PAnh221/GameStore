using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Services;

public class PlatformService(GameStoreContext dbContext, ILogger<PlatformService> logger) : IPlatformService
{
	public async Task<List<PlatformDto>> GetPlatformsAsync()
	{
		return await dbContext.Platforms
			.Select(platform => platform.ToDto())
			.AsNoTracking()
			.ToListAsync();
	}

	public async Task<PlatformDto?> GetPlatformAsync(int id)
	{
		Platform? platform = await dbContext.Platforms.FindAsync(id);

		return platform?.ToDto();
	}

	public async Task<PlatformResult> CreatePlatformAsync(CreatePlatformDto newPlatform)
	{
		var platformExists = await dbContext.Platforms.AnyAsync(platform => platform.Name == newPlatform.Name);

		if (platformExists)
		{
			logger.LogWarning("Create platform rejected: name '{PlatformName}' already exists", newPlatform.Name);
			return new PlatformResult(null, PlatformError.DuplicateName);
		}

		Platform platform = newPlatform.ToEntity();

		dbContext.Platforms.Add(platform);
		await dbContext.SaveChangesAsync();

		logger.LogInformation("Created platform {PlatformId} '{PlatformName}'", platform.Id, platform.Name);

		return new PlatformResult(platform.ToDto(), PlatformError.None);
	}

	public async Task<PlatformResult> UpdatePlatformAsync(int id, UpdatePlatformDto updatedPlatform)
	{
		var existingPlatform = await dbContext.Platforms.FindAsync(id);

		if (existingPlatform is null)
		{
			return new PlatformResult(null, PlatformError.NotFound);
		}

		var platformExists = await dbContext.Platforms.AnyAsync(platform => platform.Name == updatedPlatform.Name && platform.Id != id);

		if (platformExists)
		{
			logger.LogWarning("Update platform {PlatformId} rejected: name '{PlatformName}' already exists", id, updatedPlatform.Name);
			return new PlatformResult(null, PlatformError.DuplicateName);
		}

		dbContext.Entry(existingPlatform)
			.CurrentValues
			.SetValues(updatedPlatform.ToEntity(id));

		await dbContext.SaveChangesAsync();

		logger.LogInformation("Updated platform {PlatformId}", id);

		return new PlatformResult(null, PlatformError.None);
	}

	public async Task<PlatformError> DeletePlatformAsync(int id)
	{
		var hasGames = await dbContext.Games.AnyAsync(game => game.Platforms.Any(platform => platform.Id == id));

		if (hasGames)
		{
			logger.LogWarning("Delete platform {PlatformId} rejected: it has associated games", id);
			return PlatformError.HasGames;
		}

		var deleteCount = await dbContext.Platforms
			.Where(platform => platform.Id == id)
			.ExecuteDeleteAsync();

		if (deleteCount == 0)
		{
			return PlatformError.NotFound;
		}

		logger.LogInformation("Deleted platform {PlatformId}", id);

		return PlatformError.None;
	}
}
