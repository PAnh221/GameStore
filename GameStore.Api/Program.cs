using GameStore.Api.Dtos;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string GetGameEndpointName = "GetGame";

List<GameDto> games = [
    new GameDto(1, "The Legend of Zelda: Breath of the Wild", "Action-Adventure", 59.99m, new DateOnly(2017, 3, 3)),
    new GameDto(2, "Super Mario Odyssey", "Platformer", 59.99m, new DateOnly(2017, 10, 27)),
    new GameDto(3, "Animal Crossing: New Horizons", "Simulation", 59.99m, new DateOnly(2020, 3, 20))
];

// GET /games
app.MapGet("/games", () => games);

// GET /games/1
app.MapGet("games/{id}", (int id) => games.Find(game => game.Id == id))
    .WithName(GetGameEndpointName);

// POST /games
app.MapPost("games", (CreateGameDto newGame) =>
{
    GameDto game = new(
            games.Count + 1,
            newGame.Name,
            newGame.Genre,
            newGame.Price,
            newGame.ReleaseDate
        );
    games.Add(game);

    return Results.CreatedAtRoute(GetGameEndpointName, new { id = game.Id }, games);
});

// PUT 

app.Run();
