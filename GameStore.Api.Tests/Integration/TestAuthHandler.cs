using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameStore.Api.Tests.Integration;

// Replaces JWT validation in tests: any request with an Authorization header is signed in,
// and a request without one is anonymous, so protected routes return 401.
public class TestAuthHandler(
	IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder)
	: AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
	public const string SchemeName = "Test";

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (!Request.Headers.ContainsKey("Authorization"))
		{
			return Task.FromResult(AuthenticateResult.NoResult());
		}

		var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "test-user")], SchemeName);
		var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

		return Task.FromResult(AuthenticateResult.Success(ticket));
	}
}
