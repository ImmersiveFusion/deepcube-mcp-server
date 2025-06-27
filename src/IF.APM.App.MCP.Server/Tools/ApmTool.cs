using System.ComponentModel;
using System.Text.Json;
using IF.APM.App.Http.Api.Client;
using ModelContextProtocol.Server;

namespace IF.APM.App.MCP.Server.Tools;

[McpServerToolType]
public static class ApmTool
{
    [McpServerTool, Description("Gets all traces between now two instants of time. For example, yesterday 11AM to 11 fifteen AM")]
    public static string GetTraces(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range")] DateTimeOffset rangeStart,
        [Description("End of the range")] DateTimeOffset rangeEnd)
    {
        try
        {
            var filter = new TraceListFilterModel(new List<Facet>(), "");
            var traces = client.GetTracesAsync(anchor.GridSecondaryId, rangeStart, rangeEnd, filter).GetAwaiter().GetResult();

            return JsonSerializer.Serialize(traces);

        }
        catch (Exception e)
        {
            return e.Message;
        }

    }

    [McpServerTool, Description("Gets the trace with the specific ID")]
    public static string GetTrace(IAPMClient client, GridAnchor anchor,
        [Description("ID of the trace")] Guid trackedConsumableUniqueId)
    {
        try
        {
            var traces = client.GetTraceAsync(anchor.GridSecondaryId, trackedConsumableUniqueId).GetAwaiter().GetResult();

            return JsonSerializer.Serialize(traces);

        }
        catch (Exception e)
        {
            return e.Message;
        }

    }

    [McpServerTool, Description("Gets the tags for the span with the specific ID")]
    public static string GetTraceSpanTags(IAPMClient client, GridAnchor anchor,
        [Description("ID of the trace")] Guid trackedConsumableUniqueId)
    {
        try
        {
            var filter = new LogListFilterModel(new List<Facet>(), "");
            var traces = client.GetTraceSpanTagsAsync(anchor.GridSecondaryId, trackedConsumableUniqueId).GetAwaiter().GetResult();

            return JsonSerializer.Serialize(traces);

        }
        catch (Exception e)
        {
            return e.Message;
        }

    }

    [McpServerTool, Description("Gets the spans within a correlation ID")]
    public static string GetTraceSpans(IAPMClient client, GridAnchor anchor,
        [Description("Correlation ID")] Guid correlationId)
    {
        try
        {
            var traces = client.GetTraceSpansAsync(anchor.GridSecondaryId, correlationId).GetAwaiter().GetResult();

            return JsonSerializer.Serialize(traces);

        }
        catch (Exception e)
        {
            return e.Message;
        }

    }


    [McpServerTool, Description("Gets the errors within a correlation ID")]
    public static string GetTraceSpanErrors(IAPMClient client, GridAnchor anchor,
        [Description("Correlation ID")] Guid correlationId)
    {
        try
        {
            var traces = client.GetTraceErrorsAsync(anchor.GridSecondaryId, correlationId).GetAwaiter().GetResult();

            return JsonSerializer.Serialize(traces);

        }
        catch (Exception e)
        {
            return e.Message;
        }

    }


    [McpServerTool, Description("Gets all logs between now two instants of time. For example, yesterday 11AM to 11 fifteen AM")]
    public static string GetLogs(IAPMClient client, GridAnchor anchor,
        [Description("Beginning of the range")] DateTimeOffset rangeStart,
        [Description("End of the range")] DateTimeOffset rangeEnd)
    {
        try
        {
            var filter = new LogListFilterModel(new List<Facet>(), "");
            var traces = client.GetLogsAsync(anchor.GridSecondaryId, rangeStart, rangeEnd, filter).GetAwaiter().GetResult();

            return JsonSerializer.Serialize(traces);

        }
        catch (Exception e)
        {
            return e.Message;
        }

    }

    [McpServerTool, Description("Gets the log with the specific ID")]
    public static string GetLog(IAPMClient client, GridAnchor anchor,
        [Description("Unique ID of the log")] Guid trackedConsumableUniqueId)
    {
        try
        {
            var traces = client.GetLogAsync(anchor.GridSecondaryId, trackedConsumableUniqueId).GetAwaiter().GetResult();

            return JsonSerializer.Serialize(traces);

        }
        catch (Exception e)
        {
            return e.Message;
        }

    }
}