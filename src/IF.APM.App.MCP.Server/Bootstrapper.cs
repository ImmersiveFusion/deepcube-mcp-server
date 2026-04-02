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
    private readonly ISecurityClient _securityClient;

    public Bootstrapper(ISecurityClient securityClient)
    {
        _securityClient = securityClient;
    }

    public async Task<BootstrapResult> InitializeAsync(string? apiKey, string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new AuthenticationException(
                "API Key was not provided. Please set the IF_ApiKey environment variable.");
        }

        var resolvedBaseUrl = baseUrl ?? "https://api-azure.iapm.app";

        var wrapViewModel = await _securityClient.UnwrapApiKeyAsync(new UnwrapApiKeyModel(apiKey));
        var accessToken = wrapViewModel.Token;

        var jwtToken = ValidateToken(accessToken);
        var gridSecondaryId = ExtractGridSecondaryId(jwtToken);

        return new BootstrapResult
        {
            GridSecondaryId = gridSecondaryId,
            AccessToken = accessToken,
            BaseUrl = resolvedBaseUrl
        };
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
        var gridSecondaryIdClaim = jwtToken.Claims.SingleOrDefault(c => c.Type.Equals("gsid"));

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
