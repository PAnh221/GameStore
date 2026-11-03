using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;

namespace GameStore.Api.Tests.Mapping;

public class PlatformMappingTests
{
	[Fact]
	public void ToEntity_FromCreateDto_CopiesName()
	{
		var platform = new CreatePlatformDto("Steam Deck").ToEntity();

		Assert.Equal(0, platform.Id);
		Assert.Equal("Steam Deck", platform.Name);
	}

	[Fact]
	public void ToEntity_FromUpdateDto_SetsId()
	{
		var platform = new UpdatePlatformDto("Steam Deck").ToEntity(6);

		Assert.Equal(6, platform.Id);
		Assert.Equal("Steam Deck", platform.Name);
	}

	[Fact]
	public void ToDto_CopiesIdAndName()
	{
		var dto = new Platform { Id = 1, Name = "PC" }.ToDto();

		Assert.Equal(new PlatformDto(1, "PC"), dto);
	}
}
