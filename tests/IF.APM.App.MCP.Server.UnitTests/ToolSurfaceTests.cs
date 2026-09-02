using ComponentModelDescription = System.ComponentModel.DescriptionAttribute;
using System.Reflection;
using FluentAssertions;
using IF.APM.App.MCP.Server.Tools;
using ModelContextProtocol.Server;

namespace IF.APM.App.MCP.Server.UnitTests;

/// <summary>
/// Locks the MCP tool surface to the Unity client's assistant tool set.
/// The Unity client (ApmTools.cs + DiagnosticsTools.cs) and this server must
/// advertise the same tool names, because a model trained on one surface is
/// told to call the tools by name on the other. Adding a tool to one client
/// without the other is drift, and this test fails on it.
/// </summary>
[TestFixture]
public class ToolSurfaceTests
{
    /// <summary>
    /// Tool names as declared by the Unity client's Name overrides:
    /// 15 from ApmTools.cs, 11 from DiagnosticsTools.cs.
    /// </summary>
    internal static readonly string[] UnityToolNames =
    [
        // ApmTools.cs
        "GetAvailableCharts",
        "GetRangeData",
        "GetDashboard",
        "GetTraceFilterFacets",
        "GetLogFilterFacets",
        "GetTraces",
        "GetTrace",
        "GetTraceSpans",
        "GetTraceSpanTags",
        "GetTraceSpanEvents",
        "GetTraceErrors",
        "GetTraceLogs",
        "GetLogs",
        "GetLog",
        "GetGridInfo",
        // DiagnosticsTools.cs
        "GetSystemHealth",
        "GetDiagnosis",
        "GetServiceMap",
        "GetPressurePoints",
        "GetTrendAnalysis",
        "GetAlertSummary",
        "GetIncidentTimeline",
        "GetServiceDetail",
        "GetSlowestEndpoints",
        "GetDeploymentCorrelation",
        "GetComparison"
    ];

    private static IEnumerable<MethodInfo> ToolMethods() =>
        typeof(ApmTool).Assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<McpServerToolTypeAttribute>() != null)
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.GetCustomAttribute<McpServerToolAttribute>() != null);

    [Test]
    public void Server_Advertises_Exactly_The_Unity_Tool_Set()
    {
        var advertised = ToolMethods().Select(m => m.Name).ToArray();

        advertised.Should().BeEquivalentTo(UnityToolNames);
    }

    [Test]
    public void Every_Tool_Has_A_Description_For_Model_Tool_Selection()
    {
        foreach (var method in ToolMethods())
        {
            var description = method.GetCustomAttribute<ComponentModelDescription>();
            description.Should().NotBeNull($"{method.Name} needs a description");
            description!.Description.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Test]
    public void Every_Tool_Parameter_Is_Described()
    {
        // The grid anchor and the API clients are injected, not model-supplied,
        // so only the remaining parameters need descriptions.
        var injected = new[] { "client", "anchor" };

        foreach (var method in ToolMethods())
        {
            foreach (var parameter in method.GetParameters().Where(p => !injected.Contains(p.Name)))
            {
                parameter.GetCustomAttribute<ComponentModelDescription>()
                    .Should().NotBeNull($"{method.Name}.{parameter.Name} needs a description");
            }
        }
    }
}
