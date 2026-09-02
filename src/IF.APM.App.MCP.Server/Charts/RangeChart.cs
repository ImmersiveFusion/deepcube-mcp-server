using System;

namespace IF.APM.App.MCP.Server.Charts
{

    public class RangeChart
    {
        public Guid Id { get; set; }
        public string? Query { get; set; }
        public int YAxisDecimals { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? XAxisTitle { get; set; }
        public string? YAxisTitle { get; set; }
        public string? DefaultLabel { get; set; }
    }
}