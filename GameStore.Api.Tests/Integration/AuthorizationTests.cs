using System.Net;

namespace GameStore.Api.Tests.Integration;

public class AuthorizationTests(GameStoreWebApplicationFactory factory)
	: IClassFixture<GameStoreWebApplicationFactory>
{
	private readonly HttpClient anonymousClient = factory.CreateClient();
	private readonly HttpClient userClient = factory.CreateAuthenticatedClient();
	private readonly HttpClient adminClient = factory.CreateAdminClient();

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
	[InlineData("/games/999999")]
	[InlineData("/genres/999999")]
	[InlineData("/platforms/999999")]
	public async Task Delete_UserWithoutAdminRole_Returns403(string url)
	{
		var response = await userClient.DeleteAsync(url);

		Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
		Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
	}

	// 404 proves the request got past authorization to the endpoint.
	[Theory]
	[InlineData("/games/999999")]
	[InlineData("/genres/999999")]
	[InlineData("/platforms/999999")]
	public async Task Delete_Admin_ReachesEndpoint(string url)
	{
		var response = await adminClient.DeleteAsync(url);

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
