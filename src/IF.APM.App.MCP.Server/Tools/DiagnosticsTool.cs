using System.ComponentModel;
using System.Text.Json;
using IF.APM.App.Http.Api.Client;
using ModelContextProtocol.Server;

namespace IF.APM.App.MCP.Server.Tools;

/// <summary>
/// Composite diagnostics tools for intent-driven APM queries.
/// These mirror the Unity client's DiagnosticsTools.cs — same endpoints, same data.
/// SP-053 Wave 3: MCP Server Parity.
///
/// Every tool pins its Name explicitly. Without it the MCP SDK derives the
/// advertised name from the method name and snake_cases it (GetTraces becomes
/// get_traces), which would rename the whole surface on an SDK upgrade and
/// break parity with the Unity client.
/// </summary>
[McpServerToolType]
public static class DiagnosticsTool
{
    [McpServerTool(Name = "GetSystemHealth"), Description(
        "Gets overall system health status including per-service states, error rates, Apdex score, and top errors. " +
        "Use this FIRST to answer 'Is everything ok?', 'How is the system?', or check overall status. " +
        "If status is 'critical' or 'degraded', follow up with GetDiagnosis for root-cause details.")]
    public static async Task<string> GetSystemHealth(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional, defaults to 15 minutes ago)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional, defaults to now)")] DateTimeOffset? rangeEnd = null)
    {
        try
        {
            var result = await client.GetSystemHealthAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd));
            return JsonSerializer.Serialize(result);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve system health.");
        }
    }

    [McpServerTool(Name = "GetDiagnosis"), Description(
        "Diagnoses current system issues by analyzing recent errors, grouping by root cause, and ranking by severity. " +
        "Use this AFTER GetSystemHealth shows 'critical' or 'degraded', OR directly for 'What's wrong?', 'What broke?'. " +
        "Returns issues with stack traces, affected services, and sample trace IDs for drill-down.")]
    public static async Task<string> GetDiagnosis(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional, defaults to 15 minutes ago)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional, defaults to now)")] DateTimeOffset? rangeEnd = null)
    {
        try
        {
            var result = await client.GetDiagnosisAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd));
            return JsonSerializer.Serialize(result);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve diagnosis.");
        }
    }

    [McpServerTool(Name = "GetServiceMap"), Description(
        "Gets the service dependency map with per-service health, latency, error rates, and inter-service dependencies. " +
        "Use this to answer 'Show me the services', 'What services are running?', or understand system topology.")]
    public static async Task<string> GetServiceMap(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional, defaults to 15 minutes ago)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional, defaults to now)")] DateTimeOffset? rangeEnd = null)
    {
        try
        {
            var result = await client.GetServiceMapAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd));
            return JsonSerializer.Serialize(result);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve service map.");
        }
    }

    [McpServerTool(Name = "GetPressurePoints"), Description(
        "Detects services under active pressure by comparing current window against a 30-minute baseline. " +
        "Use this to answer 'Where is the pressure building?', 'What's about to break?', or 'Which services are under load?'. " +
        "Returns services ranked by composite pressure score with latency spikes, error surges, and traffic spikes identified.")]
    public static async Task<string> GetPressurePoints(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the current window (optional, defaults to 15 minutes ago)")] DateTimeOffset? rangeStart = null,
        [Description("End of the current window (optional, defaults to now)")] DateTimeOffset? rangeEnd = null)
    {
        try
        {
            var result = await client.GetPressurePointsAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd));
            return JsonSerializer.Serialize(result);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve pressure points.");
        }
    }

    [McpServerTool(Name = "GetTrendAnalysis"), Description(
        "Analyses latency, error rate, and throughput trends by comparing recent performance against a baseline. " +
        "Use this to answer 'Is latency getting worse?', 'Are error rates trending up?', or 'Is the system degrading?'. " +
        "Returns per-service trend classification (improving/stable/degrading) with percent-change breakdowns.")]
    public static async Task<string> GetTrendAnalysis(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the analysis window (optional, defaults to 30 minutes ago)")] DateTimeOffset? rangeStart = null,
        [Description("End of the analysis window (optional, defaults to now)")] DateTimeOffset? rangeEnd = null)
    {
        try
        {
            var result = await client.GetTrendAnalysisAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd));
            return JsonSerializer.Serialize(result);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve trend analysis.");
        }
    }

    [McpServerTool(Name = "GetAlertSummary"), Description(
        "Shows current alert status derived from system health thresholds. " +
        "Use this to answer 'Are there any alerts?', 'Is anyone being notified?', or 'Should someone be paged?'. " +
        "This is a summary — for root cause details, follow up with GetDiagnosis.")]
    public static async Task<string> GetAlertSummary(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional, defaults to 15 minutes ago)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional, defaults to now)")] DateTimeOffset? rangeEnd = null)
    {
        try
        {
            var result = await client.GetAlertSummaryAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd));
            return JsonSerializer.Serialize(result);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve alert summary.");
        }
    }

    // ── Tier 2: Operational Workflows ──

    [McpServerTool(Name = "GetIncidentTimeline"), Description(
        "Reconstructs a chronological incident timeline showing when each service first errored, spiked in latency, or degraded. " +
        "Use this to answer 'What happened?', 'Walk me through the incident', or 'When did things start going wrong?'.")]
    public static async Task<string> GetIncidentTimeline(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional, defaults to 1 hour ago)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional, defaults to now)")] DateTimeOffset? rangeEnd = null)
    {
        try
        {
            var result = await client.GetIncidentTimelineAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd));
            return JsonSerializer.Serialize(result);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve incident timeline.");
        }
    }

    [McpServerTool(Name = "GetServiceDetail"), Description(
        "Deep-dives into a single service: error rate, latency, top errors, top endpoints, and dependencies. " +
        "Use this to answer 'Tell me about [service]' or 'How is [service] doing?'.")]
    public static async Task<string> GetServiceDetail(IAPMClient client, GridAnchor anchor,
        [Description("The service name to query (e.g. 'payment-service')")] string serviceName,
        [Description("Beginning of the range (optional)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional)")] DateTimeOffset? rangeEnd = null)
    {
        try
        {
            var result = await client.GetServiceDetailAsync(anchor.GridSecondaryId, serviceName, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd));
            return JsonSerializer.Serialize(result);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve service detail.");
        }
    }

    [McpServerTool(Name = "GetSlowestEndpoints"), Description(
        "Shows the top 20 slowest endpoints ranked by P99 latency. " +
        "Use this to answer 'What's slow?', 'Which endpoints are the slowest?', or 'Show me the bottlenecks'.")]
    public static async Task<string> GetSlowestEndpoints(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional)")] DateTimeOffset? rangeEnd = null)
    {
        try
        {
            var result = await client.GetSlowestEndpointsAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd));
            return JsonSerializer.Serialize(result);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve slowest endpoints.");
        }
    }

    [McpServerTool(Name = "GetDeploymentCorrelation"), Description(
        "Detects what changed by comparing current spans against a 30-minute baseline: new service versions, new services, new endpoints, new hosts. " +
        "Use this to answer 'What changed?', 'Was there a deployment?', or 'Did anything change before the errors started?'. " +
        "Flags changes that correlate with error onset.")]
    public static async Task<string> GetDeploymentCorrelation(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range (optional)")] DateTimeOffset? rangeStart = null,
        [Description("End of the range (optional)")] DateTimeOffset? rangeEnd = null)
    {
        try
        {
            var result = await client.GetDeploymentChangesAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd));
            return JsonSerializer.Serialize(result);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve deployment changes.");
        }
    }

    [McpServerTool(Name = "GetComparison"), Description(
        "Compares system performance between two time windows (default: last 15 min vs same window yesterday). " +
        "Use this to answer 'How does this compare to yesterday?', 'Is this normal?', or 'What's different?'. " +
        "Flags services with >20% change.")]
    public static async Task<string> GetComparison(IAPMClient client, GridAnchor anchor,
        [Description("Start of current window (optional)")] DateTimeOffset? currentStart = null,
        [Description("End of current window (optional)")] DateTimeOffset? currentEnd = null,
        [Description("Start of baseline window (optional, defaults to same window yesterday)")] DateTimeOffset? baselineStart = null,
        [Description("End of baseline window (optional)")] DateTimeOffset? baselineEnd = null)
    {
        try
        {
            var result = await client.GetComparisonAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(currentStart), ToolGuards.ToUtc(currentEnd), ToolGuards.ToUtc(baselineStart), ToolGuards.ToUtc(baselineEnd));
            return JsonSerializer.Serialize(result);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve comparison.");
        }
    }
}
