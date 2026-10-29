namespace GameStore.Api.Entities;

public class Platform
{
	public int Id { get; set; }

	public required string Name { get; set; }

	public List<Game> Games { get; set; } = [];
}
