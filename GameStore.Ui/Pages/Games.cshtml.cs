using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

public class GamesModel : PageModel
{
    public List<GameSummary>? Games { get; set; }

    public record GameSummary(int Id, string Name, string Genre, decimal Price, DateOnly ReleaseDate);

    public record PagedResult<T>(int Page, int PageSize, int TotalPages, int TotalCount, List<T> Items);

    public async Task OnGetAsync([FromServices] IHttpClientFactory httpFactory)
    {
        var client = httpFactory.CreateClient("GameApi");
        try
        {
            var result = await client.GetFromJsonAsync<PagedResult<GameSummary>>("games");
            Games = result?.Items;
        }
        catch
        {
            Games = new List<GameSummary>();
        }
    }
}
