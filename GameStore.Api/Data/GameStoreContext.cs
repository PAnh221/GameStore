using GameStore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data;

public class GameStoreContext(DbContextOptions<GameStoreContext> options)
	: DbContext(options)
{
	public DbSet<Game> Games => Set<Game>();

	public DbSet<Genre> Genres => Set<Genre>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameStoreContext).Assembly);
	}
}
