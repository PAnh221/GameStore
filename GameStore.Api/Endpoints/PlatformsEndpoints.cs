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

		// POST /platforms
		group.MapPost("", async Task<Results<CreatedAtRoute<PlatformDto>, ProblemHttpResult>> (CreatePlatformDto newPlatform, GameStoreContext dbContext) =>
		{
			var platformExists = await dbContext.Platforms.AnyAsync(platform => platform.Name == newPlatform.Name);

			if (platformExists)
			{
				logger.LogWarning("Create platform rejected: name '{PlatformName}' already exists", newPlatform.Name);
				return TypedResults.Problem(detail: "Platform already exists.", statusCode: StatusCodes.Status400BadRequest);
			}

			Platform platform = newPlatform.ToEntity();

			dbContext.Platforms.Add(platform);
			await dbContext.SaveChangesAsync();

			logger.LogInformation("Created platform {PlatformId} '{PlatformName}'", platform.Id, platform.Name);

			return TypedResults.CreatedAtRoute(
				platform.ToDto(),
				GetPlatformEndpointName,
				new { id = platform.Id });
		});

		// PUT /platforms/1
		group.MapPut("/{id}", async Task<Results<NoContent, ProblemHttpResult, NotFound>> (int id, UpdatePlatformDto updatePlatform, GameStoreContext dbContext) =>
		{
			var existingPlatform = await dbContext.Platforms.FindAsync(id);

			if (existingPlatform is null)
			{
				return TypedResults.NotFound();
			}

			var platformExists = await dbContext.Platforms.AnyAsync(platform => platform.Name == updatePlatform.Name && platform.Id != id);

			if (platformExists)
			{
				logger.LogWarning("Update platform {PlatformId} rejected: name '{PlatformName}' already exists", id, updatePlatform.Name);
				return TypedResults.Problem(detail: "Platform name already exists.", statusCode: StatusCodes.Status400BadRequest);
			}

			dbContext.Entry(existingPlatform)
				.CurrentValues
				.SetValues(updatePlatform.ToEntity(id));

			await dbContext.SaveChangesAsync();

			logger.LogInformation("Updated platform {PlatformId}", id);

			return TypedResults.NoContent();
		});

		// DELETE /platforms/1
		group.MapDelete("/{id}", async Task<Results<NoContent, ProblemHttpResult, NotFound>> (int id, GameStoreContext dbContext) =>
		{
			var hasGames = await dbContext.Games.AnyAsync(game => game.Platforms.Any(platform => platform.Id == id));
			if (hasGames)
			{
				logger.LogWarning("Delete platform {PlatformId} rejected: it has associated games", id);
				return TypedResults.Problem(detail: "Cannot delete platform because it has associated games.", statusCode: StatusCodes.Status409Conflict);
			}

			var deleteCount = await dbContext.Platforms
					.Where(platform => platform.Id == id)
					.ExecuteDeleteAsync();

			if (deleteCount == 0)
			{
				return TypedResults.NotFound();
			}

			logger.LogInformation("Deleted platform {PlatformId}", id);

			return TypedResults.NoContent();
		}).ProducesProblem(StatusCodes.Status409Conflict);

		return group;
	}
}
