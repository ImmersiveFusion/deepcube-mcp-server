using System.ComponentModel;
using System.Text.Json;
using IF.APM.App.Http.Api.Client;
using ModelContextProtocol.Server;

namespace IF.APM.App.MCP.Server.Tools;

/// <summary>
/// Grid metadata. Mirrors the Unity client's GetGridInfo tool.
///
/// Every tool pins its Name explicitly. Without it the MCP SDK derives the
/// advertised name from the method name and snake_cases it (GetTraces becomes
/// get_traces), which would rename the whole surface on an SDK upgrade and
/// break parity with the Unity client.
/// </summary>
[McpServerToolType]
public static class GeneralTool
{
    [McpServerTool(Name = "GetGridInfo"), Description(
        "Retrieves metadata about the current grid (monitoring environment/workspace). " +
        "Returns the grid's name, configuration, and properties. " +
        "Use this only when you need to know which environment the user is viewing. " +
        "For system health or diagnostics, use GetSystemHealth or GetDiagnosis instead.")]
    public static async Task<string> GetGridInfo(IGeneralClient client, GridAnchor anchor)
    {
        try
        {
            var grid = await client.GetGridAsync(anchor.GridSecondaryId);
            return JsonSerializer.Serialize(grid);
        }
        catch (Exception)
        {
            return ToolGuards.Error("Failed to retrieve grid information.");
        }
    }
}
