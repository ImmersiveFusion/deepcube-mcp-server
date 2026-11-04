using System.Security.Authentication;
using IF.Api.Http.Abstractions;
using IF.APM.App.Http.Api.Client;
using IF.APM.App.MCP.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

// Create a generic host builder for
// dependency injection, logging, and configuration.
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables("IF_");

// Configure logging for better integration with MCP clients.
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// IF_Token: a personal access token for one grid. IF_ApiKey: a grid API key (see Bootstrapper).
// IF_BaseUrl: overrides the API host; unset, the server uses Bootstrapper.DefaultBaseUrl.
var token = builder.Configuration["Token"];
var apiKey = builder.Configuration["ApiKey"];
var baseUrl = Bootstrapper.ResolveBaseUrl(builder.Configuration["BaseUrl"]);

var securityClient = new SecurityClient(new ApiClientConfiguration
{
    BaseUrl = baseUrl
}, new HttpClient());

var bootstrapper = new Bootstrapper(securityClient);
BootstrapResult result;
try
{
    result = await bootstrapper.InitializeAsync(token, apiKey, baseUrl);
}
catch (AuthenticationException ex)
{
    // A plain line on stderr, not a stack trace: the MCP client only says "connection closed".
    await Console.Error.WriteLineAsync($"dcp: {ex.Message}");
    return 1;
}
catch (HttpRequestException ex)
{
    await Console.Error.WriteLineAsync(
        $"dcp: could not reach the DeepCube API at {baseUrl} ({ex.Message}). " +
        "Check the network, or set IF_BaseUrl to the API you use.");
    return 1;
}

var config = new ApiClientConfiguration
{
    AccessToken = result.AccessToken,
    BaseUrl = result.BaseUrl
};

builder.Services.AddHttpClient();
builder.Services.AddSingleton<GridAnchor>(_ => new GridAnchor { GridSecondaryId = result.GridSecondaryId });
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
return 0;

public class GridAnchor
{
    public Guid GridSecondaryId { get; set; }
}
