using System.ComponentModel;
using System.Text.Json;
using IF.APM.App.Http.Api.Client;
using ModelContextProtocol.Server;

namespace IF.APM.App.MCP.Server.Tools;

[McpServerToolType]
public static class GeneralTool
{
    [McpServerTool, Description("Gets the current grid information")]
    public static string GridInformation(IGeneralClient client, GridAnchor anchor)
    {
        try
        {
            var grid = client.GetGridAsync(anchor.GridSecondaryId).GetAwaiter().GetResult();

            return JsonSerializer.Serialize(grid);


        }
        catch (Exception e)
        {
            return e.Message;
        }

    }
}