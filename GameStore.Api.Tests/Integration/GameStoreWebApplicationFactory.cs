using Microsoft.AspNetCore.Mvc.Testing;
using GameStore.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GameStore.Api.Tests.Integration;

public class GameStoreWebApplicationFactory : WebApplicationFactory<Program>
{
	private readonly SqliteConnection connection = new("DataSource=:memory:");

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		connection.Open();

		builder.ConfigureTestServices(services =>
		{
			services.RemoveAll<DbContextOptions<GameStoreContext>>();
			services.AddDbContext<GameStoreContext>(options => options.UseSqlite(connection));
		});
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		connection.Dispose();
	}
}
