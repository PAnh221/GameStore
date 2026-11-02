using GameStore.Api.Dtos;
using GameStore.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GameStore.Api.Endpoints;

public static class PlatformsEndpoints
{
	const string GetPlatformEndpointName = "GetPlatform";

	public static RouteGroupBuilder MapPlatformsEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("platforms")
						.WithParameterValidation();

		// GET /platforms
		group.MapGet("/", async Task<Ok<List<PlatformDto>>> (IPlatformService platformService) =>
		{
			return TypedResults.Ok(await platformService.GetPlatformsAsync());
		});

		// GET /platforms/1
		group.MapGet("/{id}", async Task<Results<Ok<PlatformDto>, NotFound>> (int id, IPlatformService platformService) =>
		{
			PlatformDto? platform = await platformService.GetPlatformAsync(id);

			return platform is null ?
				TypedResults.NotFound() : TypedResults.Ok(platform);
		})
		.WithName(GetPlatformEndpointName);

		// POST /platforms
		group.MapPost("", async Task<Results<CreatedAtRoute<PlatformDto>, ProblemHttpResult>> (CreatePlatformDto newPlatform, IPlatformService platformService) =>
		{
			var result = await platformService.CreatePlatformAsync(newPlatform);

			return result.Error switch
			{
				PlatformError.DuplicateName => TypedResults.Problem(detail: "Platform already exists.", statusCode: StatusCodes.Status400BadRequest),
				_ => TypedResults.CreatedAtRoute(result.Platform!, GetPlatformEndpointName, new { id = result.Platform!.Id })
			};
		});

		// PUT /platforms/1
		group.MapPut("/{id}", async Task<Results<NoContent, ProblemHttpResult, NotFound>> (int id, UpdatePlatformDto updatedPlatform, IPlatformService platformService) =>
		{
			var result = await platformService.UpdatePlatformAsync(id, updatedPlatform);

			return result.Error switch
			{
				PlatformError.NotFound => TypedResults.NotFound(),
				PlatformError.DuplicateName => TypedResults.Problem(detail: "Platform name already exists.", statusCode: StatusCodes.Status400BadRequest),
				_ => TypedResults.NoContent()
			};
		});

		// DELETE /platforms/1
		group.MapDelete("/{id}", async Task<Results<NoContent, ProblemHttpResult, NotFound>> (int id, IPlatformService platformService) =>
		{
			var error = await platformService.DeletePlatformAsync(id);

			return error switch
			{
				PlatformError.HasGames => TypedResults.Problem(detail: "Cannot delete platform because it has associated games.", statusCode: StatusCodes.Status409Conflict),
				PlatformError.NotFound => TypedResults.NotFound(),
				_ => TypedResults.NoContent()
			};
		}).ProducesProblem(StatusCodes.Status409Conflict);

		return group;
	}
}
