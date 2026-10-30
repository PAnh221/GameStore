using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos;

public record class CreatePlatformDto(
	[Required][StringLength(50)] string Name
);
