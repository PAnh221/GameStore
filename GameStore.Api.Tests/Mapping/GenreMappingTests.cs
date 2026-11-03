using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;

namespace GameStore.Api.Tests.Mapping;

public class GenreMappingTests
{
	[Fact]
	public void ToEntity_FromCreateDto_CopiesName()
	{
		var genre = new CreateGenreDto("Puzzle").ToEntity();

		Assert.Equal(0, genre.Id);
		Assert.Equal("Puzzle", genre.Name);
	}

	[Fact]
	public void ToEntity_FromUpdateDto_SetsId()
	{
		var genre = new UpdateGenreDto("Puzzle").ToEntity(6);

		Assert.Equal(6, genre.Id);
		Assert.Equal("Puzzle", genre.Name);
	}

	[Fact]
	public void ToDto_CopiesIdAndName()
	{
		var dto = new Genre { Id = 3, Name = "Sports" }.ToDto();

		Assert.Equal(new GenreDto(3, "Sports"), dto);
	}
}
