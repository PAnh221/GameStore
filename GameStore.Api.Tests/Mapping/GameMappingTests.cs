using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;

namespace GameStore.Api.Tests.Mapping;

public class GameMappingTests
{
	private static Game CreateGame() => new()
	{
		Id = 7,
		Name = "Hades",
		GenreId = 2,
		Genre = new Genre { Id = 2, Name = "Roleplaying" },
		Platforms = [new Platform { Id = 1, Name = "PC" }, new Platform { Id = 4, Name = "Nintendo Switch" }],
		Price = 24.99m,
		ReleaseDate = new DateOnly(2020, 9, 17)
	};

	[Fact]
	public void ToEntity_FromCreateDto_CopiesFields()
	{
		var dto = new CreateGameDto("Hades", 2, [1, 4], 24.99m, new DateOnly(2020, 9, 17));

		var game = dto.ToEntity();

		Assert.Equal(0, game.Id);
		Assert.Equal("Hades", game.Name);
		Assert.Equal(2, game.GenreId);
		Assert.Equal(24.99m, game.Price);
		Assert.Equal(new DateOnly(2020, 9, 17), game.ReleaseDate);
	}

	[Fact]
	public void ToEntity_FromCreateDto_DoesNotSetPlatforms()
	{
		// Platforms must be loaded from the database by the service, not created here.
		var dto = new CreateGameDto("Hades", 2, [1, 4], 24.99m, new DateOnly(2020, 9, 17));

		var game = dto.ToEntity();

		Assert.Empty(game.Platforms);
	}

	[Fact]
	public void ToEntity_FromUpdateDto_SetsId()
	{
		var dto = new UpdateGameDto("Hades II", 3, [2], 29.99m, new DateOnly(2025, 1, 1));

		var game = dto.ToEntity(7);

		Assert.Equal(7, game.Id);
		Assert.Equal("Hades II", game.Name);
		Assert.Equal(3, game.GenreId);
		Assert.Equal(29.99m, game.Price);
		Assert.Equal(new DateOnly(2025, 1, 1), game.ReleaseDate);
	}

	[Fact]
	public void ToGameSummaryDto_MapsGenreAndPlatformNames()
	{
		var dto = CreateGame().ToGameSummaryDto();

		Assert.Equal(7, dto.Id);
		Assert.Equal("Hades", dto.Name);
		Assert.Equal("Roleplaying", dto.Genre);
		Assert.Equal(["PC", "Nintendo Switch"], dto.Platforms);
		Assert.Equal(24.99m, dto.Price);
		Assert.Equal(new DateOnly(2020, 9, 17), dto.ReleaseDate);
	}

	[Fact]
	public void ToGameDetailsDto_MapsGenreIdAndPlatformIds()
	{
		var dto = CreateGame().ToGameDetailsDto();

		Assert.Equal(8, dto.Id);
		Assert.Equal("Hades", dto.Name);
		Assert.Equal(2, dto.GenreId);
		Assert.Equal([1, 4], dto.PlatformIds);
		Assert.Equal(24.99m, dto.Price);
		Assert.Equal(new DateOnly(2020, 9, 17), dto.ReleaseDate);
	}

	[Fact]
	public void ToGameSummaryDto_NoPlatforms_ReturnsEmptyList()
	{
		var game = CreateGame();
		game.Platforms = [];

		var dto = game.ToGameSummaryDto();

		Assert.Empty(dto.Platforms);
	}
}
