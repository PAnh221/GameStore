using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Services;
using GameStore.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameStore.Api.Tests.Services;

// Seeded data: genres 1 Fighting, 2 Roleplaying, 3 Sports, 4 Racing, 5 Kids and Family.
public class GenreServiceTests : IDisposable
{
	private readonly TestDatabase database = new();
	private readonly GenreService service;

	public GenreServiceTests()
	{
		service = new GenreService(database.Context, NullLogger<GenreService>.Instance);
	}

	public void Dispose() => database.Dispose();

	private async Task<bool> GenreExistsAsync(int id) =>
		await database.Context.Genres.AnyAsync(genre => genre.Id == id);

	// GetGenresAsync / GetGenreAsync

	[Fact]
	public async Task GetGenresAsync_ReturnsSeededGenres()
	{
		var genres = await service.GetGenresAsync();

		Assert.Equal(5, genres.Count);
		Assert.Contains(new GenreDto(4, "Racing"), genres);
	}

	[Fact]
	public async Task GetGenreAsync_ExistingId_ReturnsGenre()
	{
		var genre = await service.GetGenreAsync(1);

		Assert.Equal(new GenreDto(1, "Fighting"), genre);
	}

	[Fact]
	public async Task GetGenreAsync_UnknownId_ReturnsNull()
	{
		var genre = await service.GetGenreAsync(999);

		Assert.Null(genre);
	}

	// CreateGenreAsync

	[Fact]
	public async Task CreateGenreAsync_NewName_ReturnsGenreWithNewId()
	{
		var result = await service.CreateGenreAsync(new CreateGenreDto("Puzzle"));

		Assert.Equal(GenreError.None, result.Error);
		Assert.Equal("Puzzle", result.Genre!.Name);
		Assert.True(await GenreExistsAsync(result.Genre.Id));
	}

	[Fact]
	public async Task CreateGenreAsync_DuplicateName_ReturnsDuplicateName()
	{
		var result = await service.CreateGenreAsync(new CreateGenreDto("Racing"));

		Assert.Equal(GenreError.DuplicateName, result.Error);
		Assert.Null(result.Genre);
		Assert.Equal(5, await database.Context.Genres.CountAsync());
	}

	// UpdateGenreAsync

	[Fact]
	public async Task UpdateGenreAsync_NewName_RenamesGenre()
	{
		var result = await service.UpdateGenreAsync(1, new UpdateGenreDto("Beat 'em up"));

		Assert.Equal(GenreError.None, result.Error);
		var name = await database.Context.Genres.Where(genre => genre.Id == 1).Select(genre => genre.Name).SingleAsync();
		Assert.Equal("Beat 'em up", name);
	}

	[Fact]
	public async Task UpdateGenreAsync_SameNameAsItself_IsAllowed()
	{
		var result = await service.UpdateGenreAsync(1, new UpdateGenreDto("Fighting"));

		Assert.Equal(GenreError.None, result.Error);
	}

	[Fact]
	public async Task UpdateGenreAsync_NameOfAnotherGenre_ReturnsDuplicateName()
	{
		var result = await service.UpdateGenreAsync(1, new UpdateGenreDto("Racing"));

		Assert.Equal(GenreError.DuplicateName, result.Error);
	}

	[Fact]
	public async Task UpdateGenreAsync_UnknownId_ReturnsNotFound()
	{
		var result = await service.UpdateGenreAsync(999, new UpdateGenreDto("Puzzle"));

		Assert.Equal(GenreError.NotFound, result.Error);
	}

	// DeleteGenreAsync

	[Fact]
	public async Task DeleteGenreAsync_UnusedGenre_DeletesIt()
	{
		var error = await service.DeleteGenreAsync(5);

		Assert.Equal(GenreError.None, error);
		Assert.False(await GenreExistsAsync(5));
	}

	[Fact]
	public async Task DeleteGenreAsync_UnknownId_ReturnsNotFound()
	{
		var error = await service.DeleteGenreAsync(999);

		Assert.Equal(GenreError.NotFound, error);
	}

	[Fact]
	public async Task DeleteGenreAsync_GenreWithGames_ReturnsHasGamesAndKeepsIt()
	{
		database.Context.Games.Add(new Game { Name = "Forza", GenreId = 4, Price = 49.99m, ReleaseDate = new DateOnly(2021, 11, 9) });
		await database.Context.SaveChangesAsync();

		var error = await service.DeleteGenreAsync(4);

		Assert.Equal(GenreError.HasGames, error);
		Assert.True(await GenreExistsAsync(4));
		Assert.Equal(1, await database.Context.Games.CountAsync());
	}
}
