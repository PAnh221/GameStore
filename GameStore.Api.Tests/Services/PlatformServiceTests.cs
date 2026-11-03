using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Services;
using GameStore.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameStore.Api.Tests.Services;

// Seeded data: platforms 1 PC, 2 PlayStation 5, 3 Xbox Series X|S, 4 Nintendo Switch, 5 Mobile.
public class PlatformServiceTests : IDisposable
{
	private readonly TestDatabase database = new();
	private readonly PlatformService service;

	public PlatformServiceTests()
	{
		service = new PlatformService(database.Context, NullLogger<PlatformService>.Instance);
	}

	public void Dispose() => database.Dispose();

	private async Task<bool> PlatformExistsAsync(int id) =>
		await database.Context.Platforms.AnyAsync(platform => platform.Id == id);

	// GetPlatformsAsync / GetPlatformAsync

	[Fact]
	public async Task GetPlatformsAsync_ReturnsSeededPlatforms()
	{
		var platforms = await service.GetPlatformsAsync();

		Assert.Equal(5, platforms.Count);
		Assert.Contains(new PlatformDto(1, "PC"), platforms);
	}

	[Fact]
	public async Task GetPlatformAsync_ExistingId_ReturnsPlatform()
	{
		var platform = await service.GetPlatformAsync(3);

		Assert.Equal(new PlatformDto(3, "Xbox Series X|S"), platform);
	}

	[Fact]
	public async Task GetPlatformAsync_UnknownId_ReturnsNull()
	{
		var platform = await service.GetPlatformAsync(999);

		Assert.Null(platform);
	}

	// CreatePlatformAsync

	[Fact]
	public async Task CreatePlatformAsync_NewName_ReturnsPlatformWithNewId()
	{
		var result = await service.CreatePlatformAsync(new CreatePlatformDto("Steam Deck"));

		Assert.Equal(PlatformError.None, result.Error);
		Assert.Equal("Steam Deck", result.Platform!.Name);
		Assert.True(await PlatformExistsAsync(result.Platform.Id));
	}

	[Fact]
	public async Task CreatePlatformAsync_DuplicateName_ReturnsDuplicateName()
	{
		var result = await service.CreatePlatformAsync(new CreatePlatformDto("PC"));

		Assert.Equal(PlatformError.DuplicateName, result.Error);
		Assert.Null(result.Platform);
		Assert.Equal(5, await database.Context.Platforms.CountAsync());
	}

	// UpdatePlatformAsync

	[Fact]
	public async Task UpdatePlatformAsync_NewName_RenamesPlatform()
	{
		var result = await service.UpdatePlatformAsync(5, new UpdatePlatformDto("iOS and Android"));

		Assert.Equal(PlatformError.None, result.Error);
		var name = await database.Context.Platforms.Where(platform => platform.Id == 5).Select(platform => platform.Name).SingleAsync();
		Assert.Equal("iOS and Android", name);
	}

	[Fact]
	public async Task UpdatePlatformAsync_SameNameAsItself_IsAllowed()
	{
		var result = await service.UpdatePlatformAsync(1, new UpdatePlatformDto("PC"));

		Assert.Equal(PlatformError.None, result.Error);
	}

	[Fact]
	public async Task UpdatePlatformAsync_NameOfAnotherPlatform_ReturnsDuplicateName()
	{
		var result = await service.UpdatePlatformAsync(5, new UpdatePlatformDto("PC"));

		Assert.Equal(PlatformError.DuplicateName, result.Error);
	}

	[Fact]
	public async Task UpdatePlatformAsync_UnknownId_ReturnsNotFound()
	{
		var result = await service.UpdatePlatformAsync(999, new UpdatePlatformDto("Steam Deck"));

		Assert.Equal(PlatformError.NotFound, result.Error);
	}

	// DeletePlatformAsync

	[Fact]
	public async Task DeletePlatformAsync_UnusedPlatform_DeletesIt()
	{
		var error = await service.DeletePlatformAsync(5);

		Assert.Equal(PlatformError.None, error);
		Assert.False(await PlatformExistsAsync(5));
	}

	[Fact]
	public async Task DeletePlatformAsync_UnknownId_ReturnsNotFound()
	{
		var error = await service.DeletePlatformAsync(999);

		Assert.Equal(PlatformError.NotFound, error);
	}

	[Fact]
	public async Task DeletePlatformAsync_PlatformWithGames_ReturnsHasGamesAndKeepsIt()
	{
		var pc = await database.Context.Platforms.SingleAsync(platform => platform.Id == 1);
		database.Context.Games.Add(new Game { Name = "Hades", GenreId = 2, Platforms = [pc], Price = 24.99m, ReleaseDate = new DateOnly(2020, 9, 17) });
		await database.Context.SaveChangesAsync();

		var error = await service.DeletePlatformAsync(1);

		Assert.Equal(PlatformError.HasGames, error);
		Assert.True(await PlatformExistsAsync(1));
	}
}
