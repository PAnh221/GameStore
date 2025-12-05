using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

public class GenresModel : PageModel
{
    public List<GenreDto>? Genres { get; set; }

    public record GenreDto(int Id, string Name);

    public async Task OnGetAsync([FromServices] IHttpClientFactory httpFactory)
    {
        var client = httpFactory.CreateClient("GameApi");
        try
        {
            Genres = await client.GetFromJsonAsync<List<GenreDto>>("genres");
        }
        catch
        {
            Genres = new List<GenreDto>();
        }
    }
}