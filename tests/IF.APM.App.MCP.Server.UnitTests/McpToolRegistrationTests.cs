using FluentAssertions;
using IF.APM.App.Http.Api.Client;
using IF.APM.App.MCP.Server.Tools;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using NSubstitute;

namespace IF.APM.App.MCP.Server.UnitTests;

/// <summary>
/// Exercises the real MCP registration path (the same AddMcpServer +
/// WithToolsFromAssembly call Program.cs makes) and inspects the tool
/// descriptors an MCP client receives from tools/list. Reflection over
/// attributes proves the code is annotated; this proves the server actually
/// advertises the tools.
/// </summary>
[TestFixture]
public class McpToolRegistrationTests
{
    private static IReadOnlyList<McpServerTool> RegisteredTools()
    {
        var services = new ServiceCollection();

        // Same registrations Program.cs makes, so the SDK resolves these from
        // DI instead of exposing them as model-supplied arguments.
        services.AddSingleton(new GridAnchor { GridSecondaryId = Guid.NewGuid() });
        services.AddSingleton(Substitute.For<IAPMClient>());
        services.AddSingleton(Substitute.For<IGeneralClient>());

        services.AddMcpServer().WithToolsFromAssembly(typeof(ApmTool).Assembly);
        return services.BuildServiceProvider().GetServices<McpServerTool>().ToList();
    }

    [Test]
    public void Assembly_Scan_Registers_All_Twenty_Six_Tools()
    {
        RegisteredTools().Should().HaveCount(ToolSurfaceTests.UnityToolNames.Length);
    }

    [Test]
    public void Advertised_Tool_Names_Match_The_Unity_Client()
    {
        var names = RegisteredTools().Select(t => t.ProtocolTool.Name);

        names.Should().BeEquivalentTo(ToolSurfaceTests.UnityToolNames);
    }

    [Test]
    public void Every_Advertised_Tool_Carries_A_Description_And_Input_Schema()
    {
        foreach (var tool in RegisteredTools())
        {
            tool.ProtocolTool.Description
                .Should().NotBeNullOrWhiteSpace($"{tool.ProtocolTool.Name} is chosen by an LLM from its description");
            tool.ProtocolTool.InputSchema.ValueKind
                .Should().NotBe(System.Text.Json.JsonValueKind.Undefined);
        }
    }

    [Test]
    public void Injected_Dependencies_Are_Not_Exposed_As_Model_Supplied_Arguments()
    {
        // client and anchor come from DI. If they leaked into the input schema
        // an LLM would be asked to invent an API client and a grid ID.
        foreach (var tool in RegisteredTools())
        {
            var schema = tool.ProtocolTool.InputSchema.GetRawText();
            schema.Should().NotContain("\"client\"", $"{tool.ProtocolTool.Name} must not ask the model for the API client");
            schema.Should().NotContain("\"anchor\"", $"{tool.ProtocolTool.Name} must not ask the model for the grid anchor");
        }
    }
}
