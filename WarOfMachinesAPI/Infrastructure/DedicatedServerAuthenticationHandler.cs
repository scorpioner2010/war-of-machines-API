using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace WarOfMachines.Infrastructure;

public sealed class DedicatedServerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "DedicatedServer";
    public const string ServerIdClaim = "server_id";

    private readonly DedicatedServerCredentialValidator _validator;

    public DedicatedServerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        DedicatedServerCredentialValidator validator)
        : base(options, logger, encoder)
    {
        _validator = validator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Server-Id", out var serverIdValues)
            || !Request.Headers.TryGetValue("X-Server-Credential", out var credentialValues))
        {
            return AuthenticateResult.NoResult();
        }

        string serverId = serverIdValues.ToString().Trim();
        string credential = credentialValues.ToString();
        if (serverId.Length == 0 || credential.Length == 0)
        {
            return AuthenticateResult.Fail("Dedicated server credentials are invalid.");
        }

        bool valid = await _validator.ValidateAsync(serverId, credential, Context.RequestAborted);
        if (!valid)
        {
            return AuthenticateResult.Fail("Dedicated server credentials are invalid.");
        }

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ServerIdClaim, serverId),
                new Claim(ClaimTypes.Role, "dedicated_server")
            },
            SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return AuthenticateResult.Success(ticket);
    }
}
