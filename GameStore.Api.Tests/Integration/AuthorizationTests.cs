using System.Net;

namespace GameStore.Api.Tests.Integration;

public class AuthorizationTests(GameStoreWebApplicationFactory factory)
	: IClassFixture<GameStoreWebApplicationFactory>
{
	private readonly HttpClient anonymousClient = factory.CreateClient();

	[Theory]
	[InlineData("POST", "/games")]
	[InlineData("PUT", "/games/1")]
	[InlineData("DELETE", "/games/1")]
	[InlineData("POST", "/genres")]
	[InlineData("PUT", "/genres/1")]
	[InlineData("DELETE", "/genres/1")]
	[InlineData("POST", "/platforms")]
	[InlineData("PUT", "/platforms/1")]
	[InlineData("DELETE", "/platforms/1")]
	public async Task WriteRoute_NoToken_Returns401(string method, string url)
	{
		var response = await anonymousClient.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
	}

	[Theory]
	[InlineData("/games")]
	[InlineData("/genres")]
	[InlineData("/genres/1")]
	[InlineData("/platforms")]
	[InlineData("/platforms/1")]
	public async Task ReadRoute_NoToken_Returns200(string url)
	{
		var response = await anonymousClient.GetAsync(url);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}
}
