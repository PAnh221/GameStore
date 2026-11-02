using GameStore.Api.Dtos;
using GameStore.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GameStore.Api.Endpoints;

public static class GenresEndpoints
{
	const string GetGenreEndpointName = "GetGenre";

	public static RouteGroupBuilder MapGenresEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("genres").WithParameterValidation();

		group.MapGet("/", async Task<Ok<List<GenreDto>>> (IGenreService genreService) =>
		{
			return TypedResults.Ok(await genreService.GetGenresAsync());
		});

		group.MapGet("/{id}", async Task<Results<Ok<GenreDto>, NotFound>> (int id, IGenreService genreService) =>
		{
			GenreDto? genre = await genreService.GetGenreAsync(id);

			return genre is null ?
				TypedResults.NotFound() : TypedResults.Ok(genre);
		}).WithName(GetGenreEndpointName);

		group.MapPost("", async Task<Results<CreatedAtRoute<GenreDto>, ProblemHttpResult>> (CreateGenreDto newGenre, IGenreService genreService) =>
		{
			var result = await genreService.CreateGenreAsync(newGenre);

			return result.Error switch
			{
				GenreError.DuplicateName => TypedResults.Problem(detail: "Genre already exists.", statusCode: StatusCodes.Status400BadRequest),
				_ => TypedResults.CreatedAtRoute(result.Genre!, GetGenreEndpointName, new { id = result.Genre!.Id })
			};
		});

		group.MapPut("/{id}", async Task<Results<NoContent, ProblemHttpResult, NotFound>> (int id, UpdateGenreDto updatedGenre, IGenreService genreService) =>
		{
			var result = await genreService.UpdateGenreAsync(id, updatedGenre);

			return result.Error switch
			{
				GenreError.NotFound => TypedResults.NotFound(),
				GenreError.DuplicateName => TypedResults.Problem(detail: "Genre name already exists.", statusCode: StatusCodes.Status400BadRequest),
				_ => TypedResults.NoContent()
			};
		});

		group.MapDelete("/{id}", async Task<Results<NoContent, ProblemHttpResult, NotFound>> (int id, IGenreService genreService) =>
		{
			var error = await genreService.DeleteGenreAsync(id);

			return error switch
			{
				GenreError.HasGames => TypedResults.Problem(detail: "Cannot delete genre because it has associated games.", statusCode: StatusCodes.Status409Conflict),
				GenreError.NotFound => TypedResults.NotFound(),
				_ => TypedResults.NoContent()
			};
		}).ProducesProblem(StatusCodes.Status409Conflict);

		return group;
	}
}
