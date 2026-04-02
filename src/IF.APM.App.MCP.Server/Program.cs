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

var apiKey = builder.Configuration["ApiKey"];
var baseUrl = builder.Configuration["BaseUrl"];

var securityClient = new SecurityClient(new ApiClientConfiguration
{
    BaseUrl = baseUrl ?? "https://api-azure.iapm.app"
}, new HttpClient());

var bootstrapper = new Bootstrapper(securityClient);
var result = await bootstrapper.InitializeAsync(apiKey, baseUrl);

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

public class GridAnchor
{
    public Guid GridSecondaryId { get; set; }
}
