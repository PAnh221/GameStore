using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameStore.Api.Tests.Integration;

// Replaces JWT validation in tests: any request with an Authorization header is signed in,
// and a request without one is anonymous, so protected routes return 401.
// "Authorization: Test Admin" also gives the user the Admin role.
public class TestAuthHandler(
	IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder)
	: AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
	public const string SchemeName = "Test";
	public const string AdminParameter = "Admin";

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (!Request.Headers.ContainsKey("Authorization"))
		{
			return Task.FromResult(AuthenticateResult.NoResult());
		}

		List<Claim> claims = [new Claim(ClaimTypes.Name, "test-user")];

		if (Request.Headers.Authorization.ToString() == $"{SchemeName} {AdminParameter}")
		{
			claims.Add(new Claim(ClaimTypes.Role, "Admin"));
		}

		var identity = new ClaimsIdentity(claims, SchemeName);
		var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

		return Task.FromResult(AuthenticateResult.Success(ticket));
	}
}
