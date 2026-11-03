using GameStore.Api.Dtos;
using GameStore.Api.Services;
using GameStore.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameStore.Api.Tests.Services;

// xUnit creates a new instance of this class for every test,
// so each test gets its own fresh database.
// Seeded data: genres 1-5 (4 = Racing), platforms 1-5 (1 = PC), no games.
public class GameServiceTests : IDisposable
{
	private readonly TestDatabase database = new();
	private readonly GameService service;

	public GameServiceTests()
	{
		service = new GameService(database.Context, NullLogger<GameService>.Instance);
	}

	public void Dispose() => database.Dispose();

	// Creates a game, then clears the change tracker so the next call
	// starts like a new request, reading from the database.
	private async Task<GameDetailsDto> AddGameAsync(string name = "Hades", int genreId = 2, List<int>? platformIds = null)
	{
		var result = await service.CreateGameAsync(
			new CreateGameDto(name, genreId, platformIds ?? [1, 2], 24.99m, new DateOnly(2020, 9, 17)));

		database.Context.ChangeTracker.Clear();

		return result.Game!;
	}

	// GetGameAsync

	[Fact]
	public async Task GetGameAsync_ExistingGame_ReturnsDetailsWithPlatformIds()
	{
		var created = await AddGameAsync(platformIds: [1, 2]);

		var game = await service.GetGameAsync(created.Id);

		Assert.NotNull(game);
		Assert.Equal("Hades", game.Name);
		Assert.Equal([1, 2], game.PlatformIds.Order());
	}

	[Fact]
	public async Task GetGameAsync_UnknownId_ReturnsNull()
	{
		var game = await service.GetGameAsync(999);

		Assert.Null(game);
	}

	// CreateGameAsync

	[Fact]
	public async Task CreateGameAsync_ValidInput_ReturnsGameWithNewId()
	{
		var result = await service.CreateGameAsync(
			new CreateGameDto("Hades", 2, [1, 2], 24.99m, new DateOnly(2020, 9, 17)));

		Assert.Equal(GameError.None, result.Error);
		Assert.NotNull(result.Game);
		Assert.True(result.Game.Id > 0);
		Assert.Equal([1, 2], result.Game.PlatformIds.Order());
	}

	[Fact]
	public async Task CreateGameAsync_UnknownGenre_ReturnsInvalidGenreAndSavesNothing()
	{
		var result = await service.CreateGameAsync(
			new CreateGameDto("Hades", 999, [1], 24.99m, new DateOnly(2020, 9, 17)));

		Assert.Equal(GameError.InvalidGenre, result.Error);
		Assert.Null(result.Game);
		Assert.Equal(0, await database.Context.Games.CountAsync());
	}

	[Fact]
	public async Task CreateGameAsync_UnknownPlatform_ReturnsInvalidPlatformsAndSavesNothing()
	{
		var result = await service.CreateGameAsync(
			new CreateGameDto("Hades", 2, [1, 999], 24.99m, new DateOnly(2020, 9, 17)));

		Assert.Equal(GameError.InvalidPlatforms, result.Error);
		Assert.Null(result.Game);
		Assert.Equal(0, await database.Context.Games.CountAsync());
	}

	[Fact]
	public async Task CreateGameAsync_DuplicatePlatformIds_LinksEachPlatformOnce()
	{
		var created = await AddGameAsync(platformIds: [1, 1, 2]);

		var game = await service.GetGameAsync(created.Id);

		Assert.Equal([1, 2], game!.PlatformIds.Order());
	}

	[Fact]
	public async Task CreateGameAsync_NullPlatformIds_SavesGameWithNoPlatforms()
	{
		var result = await service.CreateGameAsync(
			new CreateGameDto("Hades", 2, null!, 24.99m, new DateOnly(2020, 9, 17)));

		Assert.Equal(GameError.None, result.Error);
		Assert.Empty(result.Game!.PlatformIds);
	}

	// UpdateGameAsync

	[Fact]
	public async Task UpdateGameAsync_ValidInput_UpdatesFieldsAndReplacesPlatforms()
	{
		var created = await AddGameAsync(platformIds: [1, 2]);

		var result = await service.UpdateGameAsync(created.Id,
			new UpdateGameDto("Hades II", 3, [3], 29.99m, new DateOnly(2025, 1, 1)));
		database.Context.ChangeTracker.Clear();

		Assert.Equal(GameError.None, result.Error);
		var game = await service.GetGameAsync(created.Id);
		Assert.Equal("Hades II", game!.Name);
		Assert.Equal(3, game.GenreId);
		Assert.Equal(29.99m, game.Price);
		Assert.Equal([3], game.PlatformIds);
	}

	[Fact]
	public async Task UpdateGameAsync_EmptyPlatformIds_RemovesAllPlatforms()
	{
		var created = await AddGameAsync(platformIds: [1, 2]);

		await service.UpdateGameAsync(created.Id,
			new UpdateGameDto("Hades", 2, [], 24.99m, new DateOnly(2020, 9, 17)));
		database.Context.ChangeTracker.Clear();

		var game = await service.GetGameAsync(created.Id);
		Assert.Empty(game!.PlatformIds);
	}

	[Fact]
	public async Task UpdateGameAsync_UnknownId_ReturnsNotFound()
	{
		var result = await service.UpdateGameAsync(999,
			new UpdateGameDto("Hades", 2, [1], 24.99m, new DateOnly(2020, 9, 17)));

		Assert.Equal(GameError.NotFound, result.Error);
	}

	[Fact]
	public async Task UpdateGameAsync_UnknownGenre_ReturnsInvalidGenreAndChangesNothing()
	{
		var created = await AddGameAsync(platformIds: [1, 2]);

		var result = await service.UpdateGameAsync(created.Id,
			new UpdateGameDto("Changed", 999, [3], 29.99m, new DateOnly(2025, 1, 1)));
		database.Context.ChangeTracker.Clear();

		Assert.Equal(GameError.InvalidGenre, result.Error);
		var game = await service.GetGameAsync(created.Id);
		Assert.Equal("Hades", game!.Name);
		Assert.Equal([1, 2], game.PlatformIds.Order());
	}

	[Fact]
	public async Task UpdateGameAsync_UnknownPlatform_ReturnsInvalidPlatformsAndChangesNothing()
	{
		var created = await AddGameAsync(platformIds: [1, 2]);

		var result = await service.UpdateGameAsync(created.Id,
			new UpdateGameDto("Changed", 2, [3, 999], 29.99m, new DateOnly(2025, 1, 1)));
		database.Context.ChangeTracker.Clear();

		Assert.Equal(GameError.InvalidPlatforms, result.Error);
		var game = await service.GetGameAsync(created.Id);
		Assert.Equal("Hades", game!.Name);
		Assert.Equal([1, 2], game.PlatformIds.Order());
	}

	[Fact]
	public async Task UpdateGameAsync_MissingGameAndBadGenre_ReturnsNotFoundFirst()
	{
		var result = await service.UpdateGameAsync(999,
			new UpdateGameDto("Hades", 999, [999], 24.99m, new DateOnly(2020, 9, 17)));

		Assert.Equal(GameError.NotFound, result.Error);
	}

	// DeleteGameAsync

	[Fact]
	public async Task DeleteGameAsync_ExistingGame_ReturnsTrueAndRemovesIt()
	{
		var created = await AddGameAsync();

		var isDeleted = await service.DeleteGameAsync(created.Id);

		Assert.True(isDeleted);
		Assert.Null(await service.GetGameAsync(created.Id));
	}

	[Fact]
	public async Task DeleteGameAsync_UnknownId_ReturnsFalse()
	{
		var isDeleted = await service.DeleteGameAsync(999);

		Assert.False(isDeleted);
	}

	// GetGamesAsync

	[Fact]
	public async Task GetGamesAsync_NoFilters_ReturnsAllGamesWithGenreAndPlatformNames()
	{
		await AddGameAsync("Hades", genreId: 2, platformIds: [1]);
		await AddGameAsync("Forza", genreId: 4, platformIds: [1, 3]);

		var result = await service.GetGamesAsync(page: 1, pageSize: 10, genreId: null, search: null);

		Assert.Equal(2, result.TotalCount);
		var forza = Assert.Single(result.Items, game => game.Name == "Forza");
		Assert.Equal("Racing", forza.Genre);
		Assert.Equal(["PC", "Xbox Series X|S"], forza.Platforms.Order());
	}

	[Fact]
	public async Task GetGamesAsync_GenreFilter_ReturnsOnlyThatGenre()
	{
		await AddGameAsync("Hades", genreId: 2);
		await AddGameAsync("Forza", genreId: 4);
		await AddGameAsync("Gran Turismo", genreId: 4);

		var result = await service.GetGamesAsync(page: 1, pageSize: 10, genreId: 4, search: null);

		Assert.Equal(2, result.TotalCount);
		Assert.All(result.Items, game => Assert.Equal("Racing", game.Genre));
	}

	[Fact]
	public async Task GetGamesAsync_Search_MatchesPartOfName()
	{
		await AddGameAsync("Hades");
		await AddGameAsync("Forza Horizon");

		var result = await service.GetGamesAsync(page: 1, pageSize: 10, genreId: null, search: "Horiz");

		var game = Assert.Single(result.Items);
		Assert.Equal("Forza Horizon", game.Name);
	}

	[Fact]
	public async Task GetGamesAsync_SecondPage_ReturnsNextGamesInIdOrder()
	{
		await AddGameAsync("A");
		await AddGameAsync("B");
		await AddGameAsync("C");

		var result = await service.GetGamesAsync(page: 2, pageSize: 2, genreId: null, search: null);

		Assert.Equal(3, result.TotalCount);
		Assert.Equal(2, result.TotalPages);
		var game = Assert.Single(result.Items);
		Assert.Equal("C", game.Name);
	}

	[Fact]
	public async Task GetGamesAsync_PagePastTheEnd_ReturnsNoItemsButCorrectTotal()
	{
		await AddGameAsync("A");

		var result = await service.GetGamesAsync(page: 99, pageSize: 10, genreId: null, search: null);

		Assert.Empty(result.Items);
		Assert.Equal(1, result.TotalCount);
	}

	[Theory]
	[InlineData(0, 10, 1, 10)]
	[InlineData(-5, 10, 1, 10)]
	[InlineData(1, 0, 1, 1)]
	[InlineData(1, -5, 1, 1)]
	[InlineData(1, 1000, 1, 100)]
	public async Task GetGamesAsync_OutOfRangePaging_IsClamped(int page, int pageSize, int expectedPage, int expectedPageSize)
	{
		var result = await service.GetGamesAsync(page, pageSize, genreId: null, search: null);

		Assert.Equal(expectedPage, result.Page);
		Assert.Equal(expectedPageSize, result.PageSize);
	}
}
