using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using IF.Api.Http.Abstractions;
using IF.APM.App.Http.Api.Client;
using Microsoft.IdentityModel.Tokens;

namespace IF.APM.App.MCP.Server;

public class BootstrapResult
{
    public required Guid GridSecondaryId { get; init; }
    public required string AccessToken { get; init; }
    public required string BaseUrl { get; init; }
}

public class Bootstrapper
{
    /// <summary>The scope a token needs to read a grid's telemetry.</summary>
    public const string MemberScope = "member";

    private const string ScopeClaim = "scp";
    private const string GridClaim = "gsid";

    private const string HowToGetAPat =
        "Create a personal access token for the grid in DeepCube and set it in the IF_Token environment variable.";

    private readonly ISecurityClient _securityClient;

    public Bootstrapper(ISecurityClient securityClient)
    {
        _securityClient = securityClient;
    }

    /// <summary>
    /// Resolves the credential this server reads with: a personal access token (<c>IF_Token</c>), used as it is, or
    /// a grid API key (<c>IF_ApiKey</c>), exchanged for its token.
    /// </summary>
    /// <remarks>
    /// A grid API key created or rotated after CR-1 may only send telemetry (scope <c>otlp</c>), so every read this
    /// server makes would be refused. Such a key is refused here, at startup, with what to use instead, rather than
    /// failing every tool call later. Keys created before CR-1 carry <c>member</c> and keep working.
    /// </remarks>
    public async Task<BootstrapResult> InitializeAsync(string? token, string? apiKey, string? baseUrl)
    {
        var hasToken = !string.IsNullOrWhiteSpace(token);
        var hasApiKey = !string.IsNullOrWhiteSpace(apiKey);

        if (hasToken && hasApiKey)
        {
            throw new AuthenticationException("Set IF_Token or IF_ApiKey, not both.");
        }

        if (!hasToken && !hasApiKey)
        {
            throw new AuthenticationException(
                $"No credential was provided. Set IF_Token to a personal access token for the grid. {HowToGetAPat}");
        }

        var resolvedBaseUrl = baseUrl ?? "https://api-azure.deepcube.ai";

        string accessToken;
        string source;
        if (hasToken)
        {
            accessToken = token!.Trim();
            source = "The token in IF_Token";
            if (!new JwtSecurityTokenHandler().CanReadToken(accessToken))
            {
                throw new AuthenticationException(
                    $"The value of IF_Token is not a personal access token. {HowToGetAPat}");
            }
        }
        else
        {
            var wrapViewModel = await _securityClient.UnwrapApiKeyAsync(new UnwrapApiKeyModel(apiKey!));
            accessToken = wrapViewModel.Token;
            source = "This grid API key";
        }

        var jwtToken = ValidateToken(accessToken);
        EnsureCanRead(jwtToken, source);
        var gridSecondaryId = ExtractGridSecondaryId(jwtToken);

        return new BootstrapResult
        {
            GridSecondaryId = gridSecondaryId,
            AccessToken = accessToken,
            BaseUrl = resolvedBaseUrl
        };
    }

    /// <summary>
    /// Refuses a token that names its scopes and does not include <see cref="MemberScope"/>: it cannot read. A token
    /// that names no scopes is left to the API to decide.
    /// </summary>
    public static void EnsureCanRead(JwtSecurityToken jwtToken, string source)
    {
        var scopes = jwtToken.Claims.Where(c => c.Type == ScopeClaim).Select(c => c.Value).ToList();
        if (scopes.Count > 0 && !scopes.Contains(MemberScope, StringComparer.OrdinalIgnoreCase))
        {
            throw new AuthenticationException(
                $"{source} can only send telemetry, so it cannot read traces, logs or dashboards. Grid API keys are for " +
                $"sending telemetry; this server needs a personal access token. {HowToGetAPat}");
        }

        if (!jwtToken.Claims.Any(c => c.Type == GridClaim))
        {
            throw new AuthenticationException(
                $"{source} is not bound to a grid. Create the personal access token for one grid. {HowToGetAPat}");
        }
    }

    public static JwtSecurityToken ValidateToken(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = false, // API-issued token — signing key not available locally
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            SignatureValidator = (token, _) => handler.ReadJwtToken(token) // Trust API-issued token signature
        };

        handler.ValidateToken(accessToken, validationParameters, out var validatedToken);
        return (JwtSecurityToken)validatedToken;
    }

    public static Guid ExtractGridSecondaryId(JwtSecurityToken jwtToken)
    {
        var gridSecondaryIdClaim = jwtToken.Claims.SingleOrDefault(c => c.Type.Equals(GridClaim));

        if (gridSecondaryIdClaim == null)
        {
            throw new AuthenticationException("Grid secondary ID not found in token");
        }

        if (!Guid.TryParse(gridSecondaryIdClaim.Value, out var gridSecondaryId))
        {
            throw new AuthenticationException("Invalid grid secondary ID format in token");
        }

        return gridSecondaryId;
    }
}
