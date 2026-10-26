using GameStore.Api.Data;
using GameStore.Api.Mapping;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GameStore.Api.Endpoints;

public static class GenresEndpoints
{
	const string GetGenreEndpointName = "GetGenre";

	public static RouteGroupBuilder MapGenresEndpoints(this WebApplication app)
	{
		var logger = app.Services
			.GetRequiredService<ILoggerFactory>()
			.CreateLogger("GenresEndpoints");

		var group = app.MapGroup("genres").WithParameterValidation();

		group.MapGet("/", async Task<Ok<List<GenreDto>>> (GameStoreContext dbContext) =>
		{
			var genres = await dbContext.Genres
							.Select(genre => genre.ToDto())
							.AsNoTracking()
							.ToListAsync();
			return TypedResults.Ok(genres);
		});

		group.MapGet("/{id}", async Task<Results<Ok<GenreDto>, NotFound>> (int id, GameStoreContext dbContext) =>
		{
			var genre = await dbContext.Genres.FindAsync(id);

			if (genre is null)
			{
				return TypedResults.NotFound();
			}

			return TypedResults.Ok(genre.ToDto());
		}).WithName(GetGenreEndpointName);

		group.MapPost("", async Task<Results<CreatedAtRoute<GenreDto>, ProblemHttpResult>> (CreateGenreDto newGenre, GameStoreContext dbContext) =>
		{
			var genreExists = await dbContext.Genres.AnyAsync(genre => genre.Name == newGenre.Name);

			if (genreExists)
			{
				logger.LogWarning("Create genre rejected: name '{GenreName}' already exists", newGenre.Name);
				return TypedResults.Problem(detail: "Genre already exists.", statusCode: StatusCodes.Status400BadRequest);
			}

			Genre genre = newGenre.ToEntity();

			dbContext.Genres.Add(genre);
			await dbContext.SaveChangesAsync();

			logger.LogInformation("Created genre {GenreId} '{GenreName}'", genre.Id, genre.Name);

			return TypedResults.CreatedAtRoute(
				genre.ToDto(),
				GetGenreEndpointName,
				new { id = genre.Id }
			);
		});

		group.MapPut("/{id}", async Task<Results<NoContent, ProblemHttpResult, NotFound>> (int id, UpdateGenreDto updatedGenre, GameStoreContext dbContext) =>
		{
			var existingGenre = await dbContext.Genres.FindAsync(id);

			if (existingGenre is null)
			{
				return TypedResults.NotFound();
			}

			var genreExists = await dbContext.Genres.AnyAsync(genre => genre.Name == updatedGenre.Name && genre.Id != id);

			if (genreExists)
			{
				logger.LogWarning("Update genre {GenreId} rejected: name '{GenreName}' already exists", id, updatedGenre.Name);
				return TypedResults.Problem(detail: "Genre name already exists.", statusCode: StatusCodes.Status400BadRequest);
			}

			dbContext.Entry(existingGenre)
					 .CurrentValues
					 .SetValues(updatedGenre.ToEntity(id));

			await dbContext.SaveChangesAsync();

			logger.LogInformation("Updated genre {GenreId}", id);

			return TypedResults.NoContent();
		});

		group.MapDelete("/{id}", async Task<Results<NoContent, ProblemHttpResult, NotFound>> (int id, GameStoreContext dbContext) =>
		{
			var hasGames = await dbContext.Games.AnyAsync(game => game.GenreId == id);
			if (hasGames)
			{
				logger.LogWarning("Delete genre {GenreId} rejected: it has associated games", id);
				return TypedResults.Problem(detail: "Cannot delete genre because it has associated games.", statusCode: StatusCodes.Status409Conflict);
			}

			var deleteCount = await dbContext.Genres
					.Where(genre => genre.Id == id)
					.ExecuteDeleteAsync();

			if (deleteCount == 0)
			{
				return TypedResults.NotFound();
			}

			logger.LogInformation("Deleted genre {GenreId}", id);

			return TypedResults.NoContent();
		}).ProducesProblem(StatusCodes.Status409Conflict);

		return group;
	}
}
