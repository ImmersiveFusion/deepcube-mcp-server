using System.ComponentModel;
using System.Text.Json;
using IF.APM.App.Http.Api.Client;
using ModelContextProtocol.Server;

namespace IF.APM.App.MCP.Server.Tools;

/// <summary>
/// Composite diagnostics tools for intent-driven APM queries.
/// SP-053 Wave 3: MCP Server Parity.
///
/// NOTE: These tools require the IAPMClient to be regenerated via NSwag
/// after the DiagnosticsController endpoints are deployed (PR #2247).
/// Until then, these are stubbed to compile but will return "not yet available".
/// After NSwag regen, replace the stub bodies with the real client calls
/// (commented below each stub).
/// </summary>
[McpServerToolType]
public static class DiagnosticsTool
{
    // Stub response until SDK is regenerated with diagnostics endpoints
    private static string NotYetAvailable(string toolName) =>
        JsonSerializer.Serialize(new { error = $"{toolName} requires SDK regeneration after DiagnosticsController deployment (PR #2247). Use the granular trace/log tools in the meantime." });

    [McpServerTool, Description(
        "Gets overall system health status including per-service states, error rates, Apdex score, and top errors. " +
        "Use this FIRST to answer 'Is everything ok?', 'How is the system?', or check overall status.")]
    public static string GetSystemHealth(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional, defaults to 15 minutes ago)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional, defaults to now)")] DateTimeOffset? rangeEnd = null)
    {
        // TODO: After NSwag regen, replace with:
        // var result = client.GetSystemHealthAsync(anchor.GridSecondaryId, rangeStart, rangeEnd).GetAwaiter().GetResult();
        // return JsonSerializer.Serialize(result);
        return NotYetAvailable("GetSystemHealth");
    }

    [McpServerTool, Description(
        "Diagnoses current system issues by analyzing recent errors, grouping by root cause, and ranking by severity. " +
        "Use this AFTER GetSystemHealth shows 'critical' or 'degraded', OR directly for 'What's wrong?'.")]
    public static string GetDiagnosis(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional)")] DateTimeOffset? rangeEnd = null)
    {
        return NotYetAvailable("GetDiagnosis");
    }

    [McpServerTool, Description(
        "Gets the service dependency map with per-service health, latency, error rates, and inter-service dependencies.")]
    public static string GetServiceMap(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional)")] DateTimeOffset? rangeEnd = null)
    {
        return NotYetAvailable("GetServiceMap");
    }

    [McpServerTool, Description(
        "Detects services under active pressure by comparing current window against a 30-minute baseline. " +
        "Use this to answer 'Where is the pressure building?' or 'What's about to break?'.")]
    public static string GetPressurePoints(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the current window (optional)")] DateTimeOffset? rangeStart = null,
        [Description("End of the current window (optional)")] DateTimeOffset? rangeEnd = null)
    {
        return NotYetAvailable("GetPressurePoints");
    }

    [McpServerTool, Description(
        "Analyses latency, error rate, and throughput trends. " +
        "Use this to answer 'Is latency getting worse?' or 'Are error rates trending up?'.")]
    public static string GetTrendAnalysis(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the analysis window (optional)")] DateTimeOffset? rangeStart = null,
        [Description("End of the analysis window (optional)")] DateTimeOffset? rangeEnd = null)
    {
        return NotYetAvailable("GetTrendAnalysis");
    }

    [McpServerTool, Description(
        "Shows current alert status derived from system health thresholds. " +
        "Use this to answer 'Are there any alerts?' or 'Should someone be paged?'.")]
    public static string GetAlertSummary(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional)")] DateTimeOffset? rangeEnd = null)
    {
        return NotYetAvailable("GetAlertSummary");
    }

    [McpServerTool, Description(
        "Reconstructs a chronological incident timeline. " +
        "Use this to answer 'What happened?' or 'When did things start going wrong?'.")]
    public static string GetIncidentTimeline(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional)")] DateTimeOffset? rangeEnd = null)
    {
        return NotYetAvailable("GetIncidentTimeline");
    }

    [McpServerTool, Description(
        "Deep-dives into a single service: error rate, latency, top errors, top endpoints, dependencies. " +
        "Use this to answer 'Tell me about [service]' or 'How is [service] doing?'.")]
    public static string GetServiceDetail(IAPMClient client, GridAnchor anchor,
        [Description("The service name to query")] string serviceName,
        [Description("Beginning of the range (optional)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional)")] DateTimeOffset? rangeEnd = null)
    {
        return NotYetAvailable("GetServiceDetail");
    }

    [McpServerTool, Description(
        "Shows the top 20 slowest endpoints ranked by P99 latency. " +
        "Use this to answer 'What's slow?' or 'Show me the bottlenecks'.")]
    public static string GetSlowestEndpoints(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional)")] DateTimeOffset? rangeEnd = null)
    {
        return NotYetAvailable("GetSlowestEndpoints");
    }

    [McpServerTool, Description(
        "Detects what changed: new service versions, new services, new endpoints, new hosts. " +
        "Use this to answer 'What changed?' or 'Was there a deployment?'.")]
    public static string GetDeploymentChanges(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional)")] DateTimeOffset? rangeEnd = null)
    {
        return NotYetAvailable("GetDeploymentChanges");
    }

    [McpServerTool, Description(
        "Compares system performance between two time windows. " +
        "Use this to answer 'How does this compare to yesterday?' or 'Is this normal?'.")]
    public static string GetComparison(IAPMClient client, GridAnchor anchor,
        [Description("Start of current window (optional)")] DateTimeOffset? currentStart = null,
        [Description("End of current window (optional)")] DateTimeOffset? currentEnd = null,
        [Description("Start of baseline window (optional)")] DateTimeOffset? baselineStart = null,
        [Description("End of baseline window (optional)")] DateTimeOffset? baselineEnd = null)
    {
        return NotYetAvailable("GetComparison");
    }
}
