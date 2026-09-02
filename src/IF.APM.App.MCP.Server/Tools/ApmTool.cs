using System.ComponentModel;
using System.Text.Json;
using IF.APM.App.Http.Api.Client;
using ModelContextProtocol.Server;

namespace IF.APM.App.MCP.Server.Tools;

/// <summary>
/// Raw telemetry tools: traces, spans, errors, and logs.
/// Tool names, descriptions, and call shapes mirror the Unity client's
/// ApmTools.cs so an LLM picks the same tool on either surface.
///
/// Every tool pins its Name explicitly. Without it the MCP SDK derives the
/// advertised name from the method name and snake_cases it (GetTraces becomes
/// get_traces), which would rename the whole surface on an SDK upgrade and
/// break parity with the Unity client.
/// </summary>
[McpServerToolType]
public static class ApmTool
{
    // ── Filter facets ──

    [McpServerTool(Name = "GetTraceFilterFacets"), Description(
        "Returns all available filter facets (service names, HTTP methods, status codes, span kinds, etc.) " +
        "for narrowing trace queries within a time range. " +
        "Call this BEFORE GetTraces to discover which dimensions you can filter on.")]
    public static async Task<string> GetTraceFilterFacets(IAPMClient client, GridAnchor anchor,
        [Description("Start of the time range (inclusive). ISO 8601 format, e.g. '2025-03-19T00:00:00Z'. Must be earlier than rangeEnd.")] DateTimeOffset rangeStart,
        [Description("End of the time range (inclusive). ISO 8601 format, e.g. '2025-03-20T00:00:00Z'. Must be later than rangeStart.")] DateTimeOffset rangeEnd)
    {
        try
        {
            var rangeError = ToolGuards.ValidateTimeRange(rangeStart, rangeEnd);
            if (rangeError != null) return rangeError;

            var filters = await client.GetTraceFiltersAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd));
            return JsonSerializer.Serialize(filters);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve trace filter facets.");
        }
    }

    [McpServerTool(Name = "GetLogFilterFacets"), Description(
        "Returns all available filter facets (service names, log levels, etc.) " +
        "for narrowing log queries within a time range. " +
        "Call this BEFORE GetLogs to discover which dimensions you can filter on.")]
    public static async Task<string> GetLogFilterFacets(IAPMClient client, GridAnchor anchor,
        [Description("Start of the time range (inclusive). ISO 8601 format, e.g. '2025-03-19T00:00:00Z'. Must be earlier than rangeEnd.")] DateTimeOffset rangeStart,
        [Description("End of the time range (inclusive). ISO 8601 format, e.g. '2025-03-20T00:00:00Z'. Must be later than rangeStart.")] DateTimeOffset rangeEnd)
    {
        try
        {
            var rangeError = ToolGuards.ValidateTimeRange(rangeStart, rangeEnd);
            if (rangeError != null) return rangeError;

            var filters = await client.GetLogFiltersAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd));
            return JsonSerializer.Serialize(filters);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve log filter facets.");
        }
    }

    // ── Traces (list + single) ──

    [McpServerTool(Name = "GetTraces"), Description(
        "Lists all root trace entries (top-level distributed requests) within a time range. " +
        "Returns trace IDs, service names, operation names, durations, status codes, and timestamps. " +
        "PREFER GetSystemHealth or GetDiagnosis first for high-level views. " +
        "Use this for detailed trace browsing when you need specific traces by time range. " +
        "To drill into the child spans of a trace, pass the traceId to GetTraceSpans. " +
        "To see errors in a trace, use GetTraceErrors.")]
    public static async Task<string> GetTraces(IAPMClient client, GridAnchor anchor,
        [Description("Start of the time range (inclusive). ISO 8601 format, e.g. '2025-03-19T00:00:00Z'. Must be earlier than rangeEnd.")] DateTimeOffset rangeStart,
        [Description("End of the time range (inclusive). ISO 8601 format, e.g. '2025-03-20T00:00:00Z'. Must be later than rangeStart.")] DateTimeOffset rangeEnd)
    {
        try
        {
            var rangeError = ToolGuards.ValidateTimeRange(rangeStart, rangeEnd);
            if (rangeError != null) return rangeError;

            var filter = new TraceListFilterModel(new List<Facet>(), "");
            var traces = await client.GetTracesAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd), filter);
            return JsonSerializer.Serialize(traces);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve traces.");
        }
    }

    [McpServerTool(Name = "GetTrace"), Description(
        "Retrieves a single trace entry (root span) by its OpenTelemetry span ID. " +
        "Returns full details: service name, operation name, duration, HTTP status code, and all span attributes. " +
        "Use a spanId obtained from GetTraces results.")]
    public static async Task<string> GetTrace(IAPMClient client, GridAnchor anchor,
        [Description("The OpenTelemetry span ID that uniquely identifies the trace entry. A hex string (e.g. 'a1b2c3d4e5f60718'), NOT a GUID.")] string spanId)
    {
        try
        {
            var validationError = ToolGuards.ValidateHexId(spanId, "spanId");
            if (validationError != null) return validationError;

            var trace = await client.GetTraceAsync(anchor.GridSecondaryId, spanId);
            return JsonSerializer.Serialize(trace);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve trace.");
        }
    }

    // ── Trace spans (children of a distributed trace) ──

    [McpServerTool(Name = "GetTraceSpans"), Description(
        "Lists all spans belonging to a distributed trace by its OpenTelemetry trace ID. " +
        "Shows the full call tree across services: parent-child span relationships, service names, operation names, durations, and status codes. " +
        "Use this to understand the flow of a request through microservices. " +
        "To get tags/attributes for a specific span, use GetTraceSpanTags. " +
        "To get events (logs attached to spans), use GetTraceSpanEvents.")]
    public static async Task<string> GetTraceSpans(IAPMClient client, GridAnchor anchor,
        [Description("The OpenTelemetry trace ID that correlates all spans in a distributed trace. A 32-character hex string (e.g. 'a1b2c3d4e5f60718a1b2c3d4e5f60718'), NOT a GUID.")] string traceId)
    {
        try
        {
            var validationError = ToolGuards.ValidateHexId(traceId, "traceId");
            if (validationError != null) return validationError;

            var spans = await client.GetTraceSpansAsync(anchor.GridSecondaryId, traceId);
            return JsonSerializer.Serialize(spans);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve trace spans.");
        }
    }

    [McpServerTool(Name = "GetTraceSpanTags"), Description(
        "Retrieves all key-value tags (OpenTelemetry span attributes) for a specific span by its span ID. " +
        "Tags include: HTTP metadata (method, URL, status code, route), database queries (db.statement, db.system), " +
        "messaging attributes (messaging.system, messaging.destination), error details, and custom application tags. " +
        "Use a spanId obtained from GetTraceSpans results.")]
    public static async Task<string> GetTraceSpanTags(IAPMClient client, GridAnchor anchor,
        [Description("The OpenTelemetry span ID. A hex string (e.g. 'a1b2c3d4e5f60718'), NOT a GUID.")] string spanId)
    {
        try
        {
            var validationError = ToolGuards.ValidateHexId(spanId, "spanId");
            if (validationError != null) return validationError;

            var tags = await client.GetTraceSpanTagsAsync(anchor.GridSecondaryId, spanId);
            return JsonSerializer.Serialize(tags);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve span tags.");
        }
    }

    [McpServerTool(Name = "GetTraceSpanEvents"), Description(
        "Retrieves all events attached to a specific span by its OpenTelemetry span ID. " +
        "Span events include: exception events (name, message, stack trace), log entries emitted during the span, " +
        "and custom annotations. Each event has a timestamp, name, and key-value attributes. " +
        "This is the primary way to find stack traces and exception details for a failing span.")]
    public static async Task<string> GetTraceSpanEvents(IAPMClient client, GridAnchor anchor,
        [Description("The OpenTelemetry span ID. A hex string (e.g. 'a1b2c3d4e5f60718'), NOT a GUID.")] string spanId)
    {
        try
        {
            var validationError = ToolGuards.ValidateHexId(spanId, "spanId");
            if (validationError != null) return validationError;

            var events = await client.GetTraceSpanEventsAsync(anchor.GridSecondaryId, spanId);
            return JsonSerializer.Serialize(events);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve span events.");
        }
    }

    // ── Trace errors and logs ──

    [McpServerTool(Name = "GetTraceErrors"), Description(
        "Retrieves all error records for a specific distributed trace by its OpenTelemetry trace ID. " +
        "Returns spans with error status codes, exception events, or failed HTTP responses (4xx/5xx). " +
        "PREFER GetDiagnosis first for grouped error analysis across all traces. " +
        "Use this for raw error records of a specific trace (e.g., from a sampleTraceId returned by GetDiagnosis). " +
        "For full exception details and stack traces, call GetTraceSpanEvents with a spanId taken from " +
        "GetTraceSpans or from the sampleSpanId on a GetDiagnosis issue: these error records carry the " +
        "message and stack trace but not a span ID, so they cannot be chained directly.")]
    public static async Task<string> GetTraceErrors(IAPMClient client, GridAnchor anchor,
        [Description("The OpenTelemetry trace ID. A 32-character hex string (e.g. 'a1b2c3d4e5f60718a1b2c3d4e5f60718'), NOT a GUID.")] string traceId)
    {
        try
        {
            var validationError = ToolGuards.ValidateHexId(traceId, "traceId");
            if (validationError != null) return validationError;

            var errors = await client.GetTraceErrorsAsync(anchor.GridSecondaryId, traceId);
            return JsonSerializer.Serialize(errors);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve trace errors.");
        }
    }

    [McpServerTool(Name = "GetTraceLogs"), Description(
        "Retrieves all structured log entries emitted during a distributed trace by its OpenTelemetry trace ID. " +
        "Returns log records with level (Info/Warn/Error/etc.), message body, timestamp, service name, and span ID. " +
        "Use this to see application-level log output correlated to a specific distributed request.")]
    public static async Task<string> GetTraceLogs(IAPMClient client, GridAnchor anchor,
        [Description("The OpenTelemetry trace ID. A 32-character hex string (e.g. 'a1b2c3d4e5f60718a1b2c3d4e5f60718'), NOT a GUID.")] string traceId)
    {
        try
        {
            var validationError = ToolGuards.ValidateHexId(traceId, "traceId");
            if (validationError != null) return validationError;

            var logs = await client.GetTraceLogsAsync(anchor.GridSecondaryId, traceId);
            return JsonSerializer.Serialize(logs);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve trace logs.");
        }
    }

    // ── Logs (list + single) ──

    [McpServerTool(Name = "GetLogs"), Description(
        "Lists all log entries for the grid within a time range. " +
        "Returns log level (Info/Warn/Error/Fatal), message body, service name, timestamp, duration, " +
        "and trace/span IDs when correlated to a distributed trace. " +
        "PREFER GetDiagnosis first for error analysis. Use this for detailed log browsing or searching specific log messages. " +
        "Use GetLogFilterFacets first to discover filterable dimensions. " +
        "To get full details for a specific log entry, use GetLog with its spanId.")]
    public static async Task<string> GetLogs(IAPMClient client, GridAnchor anchor,
        [Description("Start of the time range (inclusive). ISO 8601 format, e.g. '2025-03-19T00:00:00Z'. Must be earlier than rangeEnd.")] DateTimeOffset rangeStart,
        [Description("End of the time range (inclusive). ISO 8601 format, e.g. '2025-03-20T00:00:00Z'. Must be later than rangeStart.")] DateTimeOffset rangeEnd)
    {
        try
        {
            var rangeError = ToolGuards.ValidateTimeRange(rangeStart, rangeEnd);
            if (rangeError != null) return rangeError;

            var filter = new LogListFilterModel(new List<Facet>(), "");
            var logs = await client.GetLogsAsync(anchor.GridSecondaryId, ToolGuards.ToUtc(rangeStart), ToolGuards.ToUtc(rangeEnd), filter);
            return JsonSerializer.Serialize(logs);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve logs.");
        }
    }

    [McpServerTool(Name = "GetLog"), Description(
        "Retrieves a single log entry by its OpenTelemetry span ID. " +
        "Returns full details: log level, message body, service name, timestamp, duration, " +
        "all key-value tags, error information, and correlated trace/span IDs. " +
        "Use a spanId obtained from GetLogs results.")]
    public static async Task<string> GetLog(IAPMClient client, GridAnchor anchor,
        [Description("The OpenTelemetry span ID of the log entry. A hex string (e.g. 'a1b2c3d4e5f60718'), NOT a GUID.")] string spanId)
    {
        try
        {
            var validationError = ToolGuards.ValidateHexId(spanId, "spanId");
            if (validationError != null) return validationError;

            var log = await client.GetLogAsync(anchor.GridSecondaryId, spanId);
            return JsonSerializer.Serialize(log);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve log.");
        }
    }
}
