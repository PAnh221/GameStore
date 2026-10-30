using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos;

public record class UpdatePlatformDto(
	[Required][StringLength(50)] string Name
);
