using System.ComponentModel;
using System.Text.Json;
using IF.APM.App.Http.Api.Client;
using IF.APM.App.MCP.Server.Charts;
using ModelContextProtocol.Server;

namespace IF.APM.App.MCP.Server.Tools;

/// <summary>
/// Chart and dashboard tools. Mirrors the chart section of the Unity client's
/// ApmTools.cs: discover chart IDs with GetAvailableCharts, then pull points
/// with GetRangeData.
///
/// Every tool pins its Name explicitly. Without it the MCP SDK derives the
/// advertised name from the method name and snake_cases it (GetTraces becomes
/// get_traces), which would rename the whole surface on an SDK upgrade and
/// break parity with the Unity client.
/// </summary>
[McpServerToolType]
public static class ChartTool
{
    private static readonly StaticChartProvider ChartProvider = new();

    [McpServerTool(Name = "GetAvailableCharts"), Description(
        "Lists all pre-configured APM charts (Apdex, latency percentiles, request rates, error rates, distributions). " +
        "Returns chart IDs, titles, types (sparkline/scalar/gauge/donut/area/flat-bar/table), and PromQL queries. " +
        "Call this FIRST to discover valid chartId values before using GetRangeData.")]
    public static string GetAvailableCharts()
    {
        try
        {
            return JsonSerializer.Serialize(ChartProvider.GetAllCharts());
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve available charts.");
        }
    }

    [McpServerTool(Name = "GetRangeData"), Description(
        "Retrieves time-series data points for a specific chart within a time range. " +
        "Returns timestamped metric values suitable for trend analysis. " +
        "You MUST call GetAvailableCharts first to obtain a valid chartId.")]
    public static async Task<string> GetRangeData(IAPMClient client, GridAnchor anchor,
        [Description("The GUID of the chart to query. Call GetAvailableCharts first to discover valid chart IDs and their titles.")] Guid chartId,
        [Description("Start of the time range (inclusive). ISO 8601 format, e.g. '2025-03-19T00:00:00Z'. Must be earlier than rangeEnd.")] DateTimeOffset rangeStart,
        [Description("End of the time range (inclusive). ISO 8601 format, e.g. '2025-03-20T00:00:00Z'. Must be later than rangeStart.")] DateTimeOffset rangeEnd)
    {
        try
        {
            var rangeError = ToolGuards.ValidateTimeRange(rangeStart, rangeEnd);
            if (rangeError != null) return rangeError;

            var data = await client.GetRangeDataAsync(
                anchor.GridSecondaryId, chartId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd), null, null);
            return JsonSerializer.Serialize(data);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve chart range data.");
        }
    }

    [McpServerTool(Name = "GetDashboard"), Description(
        "Retrieves a dashboard definition by its secondary ID, including its layout configuration and the charts it contains.")]
    public static async Task<string> GetDashboard(IAPMClient client, GridAnchor anchor,
        [Description("The GUID that uniquely identifies the dashboard to retrieve.")] Guid dashboardSecondaryId)
    {
        try
        {
            var dashboard = await client.GetDashboardAsync(anchor.GridSecondaryId, dashboardSecondaryId);
            return JsonSerializer.Serialize(dashboard);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve dashboard.");
        }
    }
}
