using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using IF.APM.App.Http.Api.Client;
using IF.Http.Api.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

// Create a generic host builder for
// dependency injection, logging, and configuration.
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables("IF");

// Configure logging for better integration with MCP clients.
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

var apiKey = builder.Configuration["IF_ApiKey"];

if (apiKey == null)
{
    throw new AuthenticationException("API Key was not provided. Please set the IF_ApiKey environment variable.");
}

var baseUrl = "https://api-azure.iapm.app";

var securityClient = new SecurityClient(new ApiClientConfiguration()
{
    BaseUrl = baseUrl

}, new HttpClient());

var wrapViewModel =  await securityClient.UnwrapApiKeyAsync(new UnwrapApiKeyModel(apiKey));

var accessToken = wrapViewModel.Token;

var handler = new JwtSecurityTokenHandler();
var jwtToken = handler.ReadJwtToken(accessToken);

if (jwtToken.ValidTo < DateTime.UtcNow)
{
    throw new AuthenticationException("Access token is expired");
}

var gridSecondaryIdClaim = jwtToken.Claims.SingleOrDefault(c => c.Type.Equals("gsid"));

if (gridSecondaryIdClaim == null)
{
    throw new AuthenticationException("Grid secondary ID not found in token");
}

Guid gridSecondaryId;
try
{
    gridSecondaryId = new Guid(gridSecondaryIdClaim.Value);
}
catch (Exception ex)
{
    throw new Exception("Invalid hub secondary ID specified", ex);
}

var config = new ApiClientConfiguration
{
    AccessToken = accessToken,
    BaseUrl = baseUrl
};

builder.Services.AddHttpClient();
builder.Services.AddSingleton<GridAnchor>(_ = new GridAnchor() { GridSecondaryId = gridSecondaryId});
builder.Services.AddSingleton<IGeneralClient, GeneralClient>(sp => new GeneralClient(config, sp.GetService<IHttpClientFactory>()!.CreateClient(nameof(GeneralClient))));
builder.Services.AddSingleton<IAPMClient, APMClient>(sp => new APMClient(config, sp.GetService<IHttpClientFactory>()!.CreateClient(nameof(APMClient))));

// Register the MCP server and configure it to use stdio transport.
// Scan the assembly for tool definitions.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Build and run the host. This starts the MCP server.
await builder.Build().RunAsync();

public class GridAnchor
{
    public Guid GridSecondaryId { get; set; }
}

