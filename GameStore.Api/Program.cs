using GameStore.Api.Data;
using GameStore.Api.Endpoints;
using GameStore.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connString = builder.Configuration.GetConnectionString("GameStore");
builder.Services.AddSqlite<GameStoreContext>(connString);
builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IPlatformService, PlatformService>();
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.MapGamesEndpoints();
app.MapGenresEndpoints();
app.MapPlatformsEndpoints();

await app.MigrateDbAsync();

app.Run();

public partial class Program { }
