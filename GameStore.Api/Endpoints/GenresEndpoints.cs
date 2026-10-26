using GameStore.Api.Data;
using GameStore.Api.Mapping;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GenresEndpoints
{
	const string GetGenreEndpointName = "GetGenre";

	public static RouteGroupBuilder MapGenresEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("genres").WithParameterValidation();

		group.MapGet("/", async (GameStoreContext dbContext) =>
			await dbContext.Genres
							.Select(genre => genre.ToDto())
							.AsNoTracking()
							.ToListAsync());

		group.MapGet("/{id}", async (int id, GameStoreContext dbContext) =>
		{
			var genre = await dbContext.Genres.FindAsync(id);

			if (genre is null)
			{
				return Results.NotFound();
			}

			return Results.Ok(genre.ToDto());
		}).WithName(GetGenreEndpointName);

		group.MapPost("", async (CreateGenreDto newGenre, GameStoreContext dbContext) =>
		{
			var genreExists = await dbContext.Genres.AnyAsync(genre => genre.Name == newGenre.Name);

			if (genreExists)
			{
				return Results.BadRequest("Genre already exists.");
			}

			Genre genre = newGenre.ToEntity();

			dbContext.Genres.Add(genre);
			await dbContext.SaveChangesAsync();

			return Results.CreatedAtRoute(GetGenreEndpointName,
				new { id = genre.Id },
				genre.ToDto());
		});

		group.MapPut("/{id}", async (int id, UpdateGenreDto updatedGenre, GameStoreContext dbContext) =>
		{
			var existingGenre = await dbContext.Genres.FindAsync(id);

			if (existingGenre is null)
			{
				return Results.NotFound();
			}

			var genreExists = await dbContext.Genres.AnyAsync(genre => genre.Name == updatedGenre.Name && genre.Id != id);

			if (genreExists)
			{
				return Results.BadRequest("Genre name already exists.");
			}

			dbContext.Entry(existingGenre)
					 .CurrentValues
					 .SetValues(updatedGenre.ToEntity(id));

			await dbContext.SaveChangesAsync();

			return Results.NoContent();
		});

		group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
		{
			var hasGames = await dbContext.Games.AnyAsync(game => game.GenreId == id);
			if (hasGames)
			{
				return Results.Conflict("Cannot delete genre because it has associated games.");
			}

			var deleteCount = await dbContext.Genres
					.Where(genre => genre.Id == id)
					.ExecuteDeleteAsync();

			if (deleteCount == 0)
			{
				return Results.NotFound();
			}

			return Results.NoContent();
		});

		return group;
	}
}
