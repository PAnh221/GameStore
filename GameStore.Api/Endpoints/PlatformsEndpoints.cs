using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GameStore.Api.Endpoints;

public static class PlatformsEndpoints
{
	const string GetPlatformEndpointName = "GetPlatform";

	public static RouteGroupBuilder MapPlatformsEndpoints(this WebApplication app)
	{
		var logger = app.Services
			.GetRequiredService<ILoggerFactory>()
			.CreateLogger("PlatformsEndpoints");

		var group = app.MapGroup("platforms")
						.WithParameterValidation();

		// GET /platforms
		group.MapGet("/", async Task<Ok<List<PlatformDto>>> (GameStoreContext dbContext) =>
		{
			var platforms = await dbContext.Platforms
							.Select(platform => platform.ToDto())
							.AsNoTracking()
							.ToListAsync();

			return TypedResults.Ok(platforms);
		});

		// GET /platforms/1
		group.MapGet("/{id}", async Task<Results<Ok<PlatformDto>, NotFound>> (int id, GameStoreContext dbContext) =>
		{
			Platform? platform = await dbContext.Platforms.FindAsync(id);

			return platform is null ?
				TypedResults.NotFound() : TypedResults.Ok(platform.ToDto());
		})
		.WithName(GetPlatformEndpointName);

		return group;
	}
}
