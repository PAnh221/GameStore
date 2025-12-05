using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

public class GamesModel : PageModel
{
    public List<GameSummary>? Games { get; set; }

    public record GameSummary(int Id, string Name, string Genre, decimal Price, DateOnly ReleaseDate);

    public async Task OnGetAsync([FromServices] IHttpClientFactory httpFactory)
    {
        var client = httpFactory.CreateClient("GameApi");
        try
        {
            Games = await client.GetFromJsonAsync<List<GameSummary>>("games");
        }
        catch
        {
            Games = new List<GameSummary>();
        }
    }
}