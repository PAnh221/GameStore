using GameStore.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Tests.Helpers;

// A fresh in-memory SQLite database with the seeded genres and platforms.
// The database exists only while the connection is open, so dispose it after each test.
public sealed class TestDatabase : IDisposable
{
	private readonly SqliteConnection connection;

	public GameStoreContext Context { get; }

	public TestDatabase()
	{
		connection = new SqliteConnection("DataSource=:memory:");
		connection.Open();

		var options = new DbContextOptionsBuilder<GameStoreContext>()
			.UseSqlite(connection)
			.Options;

		Context = new GameStoreContext(options);
		Context.Database.EnsureCreated();
	}

	public void Dispose()
	{
		Context.Dispose();
		connection.Dispose();
	}
}
