using System.Net;
using System.Net.Http.Json;
using GameStore.Api.Data;
using GameStore.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameStore.Api.Tests.Integration;

// One app and one in-memory database are shared by all tests in this class,
// so each test creates its own data and doesn't assert exact counts.
// Seeded by the migrations: genres 1-5 (4 = Racing), platforms 1-5 (1 = PC).
public class GamesEndpointsTests(GameStoreWebApplicationFactory factory)
	: IClassFixture<GameStoreWebApplicationFactory>
{
	private const int UnknownId = 999999;

	private readonly HttpClient client = factory.CreateAuthenticatedClient();

	private static CreateGameDto NewGame(string name = "Hades", int genreId = 2, List<int>? platformIds = null) =>
		new(name, genreId, platformIds ?? [1, 2], 24.99m, new DateOnly(2020, 9, 17));

	private static UpdateGameDto UpdatedGame(string name = "Hades II", int genreId = 3, List<int>? platformIds = null) =>
		new(name, genreId, platformIds ?? [3], 29.99m, new DateOnly(2025, 1, 1));

	private async Task<GameDetailsDto> CreateGameAsync(string name = "Hades")
	{
		var response = await client.PostAsJsonAsync("/games", NewGame(name));
		response.EnsureSuccessStatusCode();

		return (await response.Content.ReadFromJsonAsync<GameDetailsDto>())!;
	}

	private static async Task AssertProblemAsync(HttpResponseMessage response, HttpStatusCode status, string detail)
	{
		Assert.Equal(status, response.StatusCode);
		Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

		var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		Assert.Equal(detail, problem!.Detail);
	}

	// Guards against the tests ever writing to the real GameStore.db.
	[Fact]
	public void App_UsesInMemoryDatabase()
	{
		using var scope = factory.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<GameStoreContext>();

		Assert.Contains(":memory:", dbContext.Database.GetConnectionString());
	}

	// GET /games

	[Fact]
	public async Task GetGames_ReturnsPagedResultContainingCreatedGame()
	{
		var created = await CreateGameAsync("Listed Game");

		var result = await client.GetFromJsonAsync<PagedResult<GameSummaryDto>>("/games?search=Listed&pageSize=100");

		var game = Assert.Single(result!.Items, game => game.Id == created.Id);
		Assert.Equal("Roleplaying", game.Genre);
		Assert.Equal(["PC", "PlayStation 5"], game.Platforms.Order());
	}

	[Fact]
	public async Task GetGames_OutOfRangePageSize_IsClamped()
	{
		var result = await client.GetFromJsonAsync<PagedResult<GameSummaryDto>>("/games?page=0&pageSize=100000");

		Assert.Equal(1, result!.Page);
		Assert.Equal(100, result.PageSize);
	}

	// GET /games/1

	[Fact]
	public async Task GetGame_ExistingId_Returns200WithDetails()
	{
		var created = await CreateGameAsync();

		var response = await client.GetAsync($"/games/{created.Id}");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var game = await response.Content.ReadFromJsonAsync<GameDetailsDto>();
		Assert.Equal(created.Id, game!.Id);
		Assert.Equal("Hades", game.Name);
		Assert.Equal(2, game.GenreId);
		Assert.Equal([1, 2], game.PlatformIds.Order());
	}

	[Fact]
	public async Task GetGame_UnknownId_Returns404()
	{
		var response = await client.GetAsync($"/games/{UnknownId}");

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	// POST /games

	[Fact]
	public async Task PostGame_ValidInput_Returns201WithLocationAndBody()
	{
		var response = await client.PostAsJsonAsync("/games", NewGame());

		Assert.Equal(HttpStatusCode.Created, response.StatusCode);
		var game = await response.Content.ReadFromJsonAsync<GameDetailsDto>();
		Assert.Equal($"/games/{game!.Id}", response.Headers.Location!.AbsolutePath);
		Assert.Equal([1, 2], game.PlatformIds.Order());
	}

	[Fact]
	public async Task PostGame_UnknownGenre_Returns400Problem()
	{
		var response = await client.PostAsJsonAsync("/games", NewGame(genreId: UnknownId));

		await AssertProblemAsync(response, HttpStatusCode.BadRequest, "Invalid genre ID.");
	}

	[Fact]
	public async Task PostGame_UnknownPlatform_Returns400Problem()
	{
		var response = await client.PostAsJsonAsync("/games", NewGame(platformIds: [1, UnknownId]));

		await AssertProblemAsync(response, HttpStatusCode.BadRequest, "Invalid platform IDs.");
	}

	[Fact]
	public async Task PostGame_EmptyName_Returns400()
	{
		var response = await client.PostAsJsonAsync("/games", NewGame(name: ""));

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task PostGame_PriceOutOfRange_Returns400()
	{
		var game = NewGame() with { Price = 1000m };

		var response = await client.PostAsJsonAsync("/games", game);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	// PUT /games/1

	[Fact]
	public async Task PutGame_ValidInput_Returns204AndSavesChanges()
	{
		var created = await CreateGameAsync();

		var response = await client.PutAsJsonAsync($"/games/{created.Id}", UpdatedGame());

		Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
		var game = await client.GetFromJsonAsync<GameDetailsDto>($"/games/{created.Id}");
		Assert.Equal("Hades II", game!.Name);
		Assert.Equal(3, game.GenreId);
		Assert.Equal(29.99m, game.Price);
		Assert.Equal([3], game.PlatformIds);
	}

	[Fact]
	public async Task PutGame_UnknownId_Returns404()
	{
		var response = await client.PutAsJsonAsync($"/games/{UnknownId}", UpdatedGame());

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task PutGame_UnknownGenre_Returns400ProblemAndChangesNothing()
	{
		var created = await CreateGameAsync();

		var response = await client.PutAsJsonAsync($"/games/{created.Id}", UpdatedGame(genreId: UnknownId));

		await AssertProblemAsync(response, HttpStatusCode.BadRequest, "Invalid genre ID.");
		var game = await client.GetFromJsonAsync<GameDetailsDto>($"/games/{created.Id}");
		Assert.Equal("Hades", game!.Name);
	}

	[Fact]
	public async Task PutGame_UnknownPlatform_Returns400ProblemAndChangesNothing()
	{
		var created = await CreateGameAsync();

		var response = await client.PutAsJsonAsync($"/games/{created.Id}", UpdatedGame(platformIds: [3, UnknownId]));

		await AssertProblemAsync(response, HttpStatusCode.BadRequest, "Invalid platform IDs.");
		var game = await client.GetFromJsonAsync<GameDetailsDto>($"/games/{created.Id}");
		Assert.Equal([1, 2], game!.PlatformIds.Order());
	}

	// DELETE /games/1

	[Fact]
	public async Task DeleteGame_ExistingGame_Returns204ThenGetReturns404()
	{
		var created = await CreateGameAsync();

		var deleteResponse = await client.DeleteAsync($"/games/{created.Id}");
		var getResponse = await client.GetAsync($"/games/{created.Id}");

		Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
		Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
	}

	[Fact]
	public async Task DeleteGame_UnknownId_Returns404()
	{
		var response = await client.DeleteAsync($"/games/{UnknownId}");

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}
}
