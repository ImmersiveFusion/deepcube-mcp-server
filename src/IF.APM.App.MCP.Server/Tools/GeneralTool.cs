using System.ComponentModel;
using System.Text.Json;
using IF.APM.App.Http.Api.Client;
using ModelContextProtocol.Server;

namespace IF.APM.App.MCP.Server.Tools;

[McpServerToolType]
public static class GeneralTool
{
    [McpServerTool, Description("Gets the current grid information")]
    public static async Task<string> GridInformation(IGeneralClient client, GridAnchor anchor)
    {
        try
        {
            var grid = await client.GetGridAsync(anchor.GridSecondaryId);
            return JsonSerializer.Serialize(grid);
        }
        catch (Exception)
        {
            return JsonSerializer.Serialize(new { error = "Failed to retrieve grid information." });
        }
    }
}
