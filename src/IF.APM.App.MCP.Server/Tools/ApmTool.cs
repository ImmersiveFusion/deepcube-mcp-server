using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using IF.APM.App.Http.Api.Client;
using ModelContextProtocol.Server;

namespace IF.APM.App.MCP.Server.Tools;

[McpServerToolType]
public static partial class ApmTool
{
    private static readonly TimeSpan MaxTimeRange = TimeSpan.FromDays(7);

    [GeneratedRegex("^[0-9a-fA-F]{1,64}$")]
    private static partial Regex HexIdPattern();

    private static string? ValidateHexId(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value) || !HexIdPattern().IsMatch(value))
            return JsonSerializer.Serialize(new { error = $"Invalid {paramName} format. Expected a hex string." });
        return null;
    }

    private static string? ValidateTimeRange(DateTimeOffset rangeStart, DateTimeOffset rangeEnd)
    {
        if (rangeEnd <= rangeStart)
            return JsonSerializer.Serialize(new { error = "rangeEnd must be after rangeStart." });
        if (rangeEnd - rangeStart > MaxTimeRange)
            return JsonSerializer.Serialize(new { error = $"Time range exceeds maximum of {MaxTimeRange.TotalDays} days." });
        return null;
    }

    [McpServerTool, Description("Gets all traces between two instants of time. For example, yesterday 11AM to 11:15AM")]
    public static async Task<string> GetTraces(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range")] DateTimeOffset rangeStart,
        [Description("End of the range")] DateTimeOffset rangeEnd)
    {
        try
        {
            var rangeError = ValidateTimeRange(rangeStart, rangeEnd);
            if (rangeError != null) return rangeError;

            var filter = new TraceListFilterModel(new List<Facet>(), "");
            var traces = await client.GetTracesAsync(anchor.GridSecondaryId, rangeStart, rangeEnd, filter);
            return JsonSerializer.Serialize(traces);
        }
        catch (Exception)
        {
            return JsonSerializer.Serialize(new { error = "Failed to retrieve traces." });
        }
    }

    [McpServerTool, Description("Gets the trace with the specific span ID")]
    public static async Task<string> GetTrace(IAPMClient client, GridAnchor anchor,
        [Description("Span ID of the trace (hex string, e.g. 'a1b2c3d4e5f60718')")] string spanId)
    {
        try
        {
            var validationError = ValidateHexId(spanId, "spanId");
            if (validationError != null) return validationError;

            var traces = await client.GetTraceAsync(anchor.GridSecondaryId, spanId);
            return JsonSerializer.Serialize(traces);
        }
        catch (Exception)
        {
            return JsonSerializer.Serialize(new { error = "Failed to retrieve trace." });
        }
    }

    [McpServerTool, Description("Gets the tags for the span with the specific ID")]
    public static async Task<string> GetTraceSpanTags(IAPMClient client, GridAnchor anchor,
        [Description("Span ID (hex string)")] string spanId)
    {
        try
        {
            var validationError = ValidateHexId(spanId, "spanId");
            if (validationError != null) return validationError;

            var traces = await client.GetTraceSpanTagsAsync(anchor.GridSecondaryId, spanId);
            return JsonSerializer.Serialize(traces);
        }
        catch (Exception)
        {
            return JsonSerializer.Serialize(new { error = "Failed to retrieve span tags." });
        }
    }

    [McpServerTool, Description("Gets the spans within a correlation (distributed trace)")]
    public static async Task<string> GetTraceSpans(IAPMClient client, GridAnchor anchor,
        [Description("Trace ID / correlation ID (hex string, e.g. 'a1b2c3d4e5f60718a1b2c3d4e5f60718')")] string traceId)
    {
        try
        {
            var validationError = ValidateHexId(traceId, "traceId");
            if (validationError != null) return validationError;

            var traces = await client.GetTraceSpansAsync(anchor.GridSecondaryId, traceId);
            return JsonSerializer.Serialize(traces);
        }
        catch (Exception)
        {
            return JsonSerializer.Serialize(new { error = "Failed to retrieve trace spans." });
        }
    }

    [McpServerTool, Description("Gets the errors within a correlation (distributed trace)")]
    public static async Task<string> GetTraceSpanErrors(IAPMClient client, GridAnchor anchor,
        [Description("Trace ID / correlation ID (hex string)")] string traceId)
    {
        try
        {
            var validationError = ValidateHexId(traceId, "traceId");
            if (validationError != null) return validationError;

            var traces = await client.GetTraceErrorsAsync(anchor.GridSecondaryId, traceId);
            return JsonSerializer.Serialize(traces);
        }
        catch (Exception)
        {
            return JsonSerializer.Serialize(new { error = "Failed to retrieve trace errors." });
        }
    }

    [McpServerTool, Description("Gets all logs between two instants of time. For example, yesterday 11AM to 11:15AM")]
    public static async Task<string> GetLogs(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range")] DateTimeOffset rangeStart,
        [Description("End of the range")] DateTimeOffset rangeEnd)
    {
        try
        {
            var rangeError = ValidateTimeRange(rangeStart, rangeEnd);
            if (rangeError != null) return rangeError;

            var filter = new LogListFilterModel(new List<Facet>(), "");
            var traces = await client.GetLogsAsync(anchor.GridSecondaryId, rangeStart, rangeEnd, filter);
            return JsonSerializer.Serialize(traces);
        }
        catch (Exception)
        {
            return JsonSerializer.Serialize(new { error = "Failed to retrieve logs." });
        }
    }

    [McpServerTool, Description("Gets the log with the specific span ID")]
    public static async Task<string> GetLog(IAPMClient client, GridAnchor anchor,
        [Description("Span ID of the log entry (hex string)")] string spanId)
    {
        try
        {
            var validationError = ValidateHexId(spanId, "spanId");
            if (validationError != null) return validationError;

            var traces = await client.GetLogAsync(anchor.GridSecondaryId, spanId);
            return JsonSerializer.Serialize(traces);
        }
        catch (Exception)
        {
            return JsonSerializer.Serialize(new { error = "Failed to retrieve log." });
        }
    }
}
