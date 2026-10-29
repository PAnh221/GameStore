using GameStore.Api.Dtos;
using GameStore.Api.Entities;

namespace GameStore.Api.Mapping;

public static class PlatformMapping
{
	public static Platform ToEntity(this CreatePlatformDto createPlatformDto)
	{
		return new Platform()
		{
			Name = createPlatformDto.Name
		};
	}

	public static Platform ToEntity(this UpdatePlatformDto updatePlatformDto, int id)
	{
		return new Platform()
		{
			Id = id,
			Name = updatePlatformDto.Name
		};
	}

	public static PlatformDto ToDto(this Platform platform)
	{
		return new PlatformDto(platform.Id, platform.Name);
	}
}
