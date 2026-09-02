// Ported verbatim from the Unity client's StaticChartProvider
// (Assets/IF/Unity/Assistant/Temp/StaticChartProvider.cs) so the MCP server
// advertises exactly the same chart catalog, IDs included.
//
// The catalog is hard-coded on both surfaces because the API exposes no
// "list charts" endpoint (only GetChart/GetRangeData by ID). That makes this
// file a copy that can drift from Unity's; when the API grows a catalog
// endpoint, both surfaces should read from it and this file should go away.

using System;
using System.Collections.Generic;

namespace IF.APM.App.MCP.Server.Charts
{

    public class StaticChartProvider
    {
        public IEnumerable<RangeChart> GetAllCharts()
        {
            var items = new RangeChart[]
            {
                #region Apdex

                #region Sparkline

                new()
                {
                    Id = new Guid("00000087-0000-0000-0000-000000000000"),
                    Query = @"
#topk(5, 
( 
    #satisfied 
    sum(rate(if_span_duration_milliseconds_bucket{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_CONSUMER"", gsid=""$gsid"",le=~""0|5|10|25|50|75|100|250|500""}[15m])) 
    + 
    #tolerating 
    (sum(rate(if_span_duration_milliseconds_bucket{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_CONSUMER"", gsid=""$gsid"",le=~""0|5|10|25|50|75|100|250|500""}[15m]))  - sum(rate(if_span_duration_milliseconds_bucket{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_CONSUMER"", gsid=""$gsid"",le=~""750|1000""}[15m]))) * .5 
) / sum(rate(if_span_duration_milliseconds_bucket{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_CONSUMER"", gsid=""$gsid""}[15m]))
#)

#Buckets: 
# 0 5 10 25 50 75 100 250
# 500 750 1000 2500 
# 5000 7500 10000 +Inf",
                    YAxisDecimals = 2,
                    Type = "sparkline",
                    Title = "Apdex (T=500)",
                    XAxisTitle = "",
                    YAxisTitle = ""
                },

                #endregion

                new()
                {
                    Id = new Guid("00000088-0000-0000-0000-000000000000"),
                    Query = @"
#topk(5, 
( 
    #satisfied 
    sum(rate(if_span_duration_milliseconds_bucket{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_CONSUMER"", gsid=""$gsid"",le=~""0|5|10|25|50|75|100|250|500""}[15m])) 
    + 
    #tolerating 
    (sum(rate(if_span_duration_milliseconds_bucket{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_CONSUMER"", gsid=""$gsid"",le=~""0|5|10|25|50|75|100|250|500""}[15m]))  - sum(rate(if_span_duration_milliseconds_bucket{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_CONSUMER"", gsid=""$gsid"",le=~""750|1000""}[15m]))) * .5 
) / sum(rate(if_span_duration_milliseconds_bucket{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_CONSUMER"", gsid=""$gsid""}[15m]))
#)

#Buckets: 
# 0 5 10 25 50 75 100 250
# 500 750 1000 2500 
# 5000 7500 10000 +Inf",
                    YAxisDecimals = 2,
                    Type = "scalar",
                    Title = "Apdex (T=500)",
                    XAxisTitle = "",
                    YAxisTitle = "",
                },

                #endregion


                new()
                {
                    Id = new Guid("00000300-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(if_span_duration_milliseconds_count{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_PRODUCER"", gsid=""$gsid""})",
                    YAxisDecimals = 0,
                    Type = "scalar",
                    Title = "Server/Producer Requests",
                    XAxisTitle = "",
                    YAxisTitle = "",

                },
                new()
                {
                    Id = new Guid("00000301-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(if_span_duration_milliseconds_count{gsid=""$gsid"", span_kind=""SPAN_KIND_SERVER"", http_response_status_code=~""^2.*""}) by(http_response_status_code)",
                    YAxisDecimals = 0,
                    Type = "scalar",
                    Title = "Http Status 2XX",
                    XAxisTitle = "",
                    YAxisTitle = "",
                },

                new()
                {
                    Id = new Guid("00000302-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(if_span_duration_milliseconds_count{gsid=""$gsid"", span_kind=""SPAN_KIND_SERVER"", http_response_status_code=~""^3.*""}) by(http_response_status_code)",
                    YAxisDecimals = 0,
                    Type = "scalar",
                    Title = "Http Status 3XX",
                    XAxisTitle = "",
                    YAxisTitle = "",

                },

                new()
                {
                    Id = new Guid("00000303-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(if_span_duration_milliseconds_count{gsid=""$gsid"", span_kind=""SPAN_KIND_SERVER"", http_response_status_code=~""^4.*""}) by(http_response_status_code)",
                    YAxisDecimals = 0,
                    Type = "scalar",
                    Title = "Http Status 4XX",
                    XAxisTitle = "",
                    YAxisTitle = "",

                },

                new()
                {
                    Id = new Guid("00000304-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(if_span_duration_milliseconds_count{gsid=""$gsid"", span_kind=""SPAN_KIND_SERVER"", http_response_status_code=~""^5.*""}) by(http_response_status_code)",
                    YAxisDecimals = 0,
                    Type = "scalar",
                    Title = "Http Status 5XX",
                    XAxisTitle = "",
                    YAxisTitle = "",

                },

                new()
                {
                    Id = new Guid("00000305-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(if_span_duration_milliseconds_count{gsid=""$gsid"", span_kind=""SPAN_KIND_PRODUCER"" }) - sum(if_span_duration_milliseconds_count{gsid=""$gsid"", span_kind=""SPAN_KIND_CONSUMER"" })   ",
                    YAxisDecimals = 0,
                    Type = "scalar",
                    Title = "Lost Producer Requests",
                    XAxisTitle = "",
                    YAxisTitle = "",

                },

                new()
                {
                    Id = new Guid("00000306-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(if_span_duration_milliseconds_count{gsid=""$gsid"", span_kind=""SPAN_KIND_CLIENT"" })",
                    YAxisDecimals = 0,
                    Type = "scalar",
                    Title = "Client Requests",
                    XAxisTitle = "",
                    YAxisTitle = "",

                },
                new()
                {
                    Id = new Guid("00000307-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(if_span_duration_milliseconds_count{gsid=""$gsid"", span_kind=""SPAN_KIND_PRODUCER"" })",
                    YAxisDecimals = 0,
                    Type = "scalar",
                    Title = "Producer Requests",
                    XAxisTitle = "",
                    YAxisTitle = "",

                },
                new()
                {
                    Id = new Guid("00000308-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(if_span_duration_milliseconds_count{gsid=""$gsid"", span_kind=""SPAN_KIND_CONSUMER"" })",
                    YAxisDecimals = 0,
                    Type = "scalar",
                    Title = "Consumer Executes",
                    XAxisTitle = "",
                    YAxisTitle = "",

                },

                new()
                {
                    Id = new Guid("00000310-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(if_span_duration_milliseconds_count{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_PRODUCER"", gsid=""$gsid""}) by(resource)",
                    YAxisDecimals = 0,
                    Type = "donut",
                    Title = "Request Distribution",
                    XAxisTitle = "",
                    YAxisTitle = "",
                },

                new()
                {
                    Id = new Guid("00000311-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(if_span_duration_milliseconds_sum{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_CONSUMER"", gsid=""$gsid""}) by(resource)",
                    YAxisDecimals = 0,
                    Type = "donut",
                    Title = "Loading Distribution",
                    XAxisTitle = "",
                    YAxisTitle = "",

                },

                new()
                {
                    Id = new Guid("00000320-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(rate(if_span_duration_milliseconds_count{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_PRODUCER"", gsid=""$gsid""}[3m])*60)",
                    YAxisDecimals = 0,
                    Type = "gauge",
                    Title = "Overall Request Rate",
                    XAxisTitle = "",
                    YAxisTitle = "reqpm",

                },

                new()
                {
                    Id = new Guid("00000321-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(if_span_duration_milliseconds_count{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_CONSUMER"", gsid=""$gsid"", stage=""failed""})/sum(if_span_duration_milliseconds_count{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_CONSUMER"", gsid=""$gsid""})",
                    YAxisDecimals = 0,
                    Type = "gauge",
                    Title = "Overall Error Rate",
                    XAxisTitle = "",
                    YAxisTitle = "%",

                },

                new()
                {
                    Id = new Guid("00000330-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(rate(if_span_duration_milliseconds_count{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_PRODUCER"", gsid=""$gsid""}[3m])*60)",
                    YAxisDecimals = 0,
                    Type = "sparkline",
                    Title = "Overall Request Rate",
                    XAxisTitle = "",
                    YAxisTitle = "",

                },

                new()
                {
                    Id = new Guid("00000331-0000-0000-0000-000000000000"),
                    Query =
                        @"sum(if_span_duration_milliseconds_count{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_CONSUMER"", gsid=""$gsid"", stage=""failed""})/sum(if_span_duration_milliseconds_count{span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_CONSUMER"", gsid=""$gsid""})",
                    YAxisDecimals = 0,
                    Type = "sparkline",
                    Title = "Overall Error Rate",
                    XAxisTitle = "",
                    YAxisTitle = "",

                },

                new()
                {
                    Id = new Guid("00000340-0000-0000-0000-000000000000"),
                    Query =
                        @"label_join((sort_desc(avg by (http_response_status_code, resource)  (round(if_span_duration_milliseconds_sum{span_kind=""SPAN_KIND_SERVER"", gsid=""$gsid"", http_response_status_code!=""""} / if_span_duration_milliseconds_count{span_kind=""SPAN_KIND_SERVER"", gsid=""$gsid"", http_response_status_code!=""""})))), ""endpoint"", "" "", ""http_response_status_code"", ""resource"")",
                    YAxisDecimals = 0,
                    Type = "flat-bar",
                    Title = "Average Latency",
                    XAxisTitle = "",
                    YAxisTitle = "",
                    DefaultLabel = "endpoint"

                },

                new()
                {
                    Id = new Guid("00000341-0000-0000-0000-000000000000"),
                    Query =
                        @"round(sum(rate(if_span_duration_milliseconds_count{gsid=""$gsid"", span_kind=~""SPAN_KIND_SERVER|SPAN_KIND_PRODUCER""}[10m])*60)  by(resource))",
                    YAxisDecimals = 0,
                    Type = "area",
                    Title = "Request Rate",
                    XAxisTitle = "",
                    YAxisTitle = "reqpm"
                },


                #region Percentile

                #region Sparkline

                new()
                {
                    Id = new Guid("00000201-0000-0000-0000-000000000000"),
                    Query =
                        @"histogram_quantile(0.99, sum(rate(http_server_request_duration_seconds_bucket{gsid=""$gsid""}[$__interval])) by (le))",
                    YAxisDecimals = 1,
                    Type = "sparkline",
                    Title = "p99",
                    XAxisTitle = "",
                    YAxisTitle = "s",

                },
                new()
                {
                    Id = new Guid("00000202-0000-0000-0000-000000000000"),
                    Query =
                        @"histogram_quantile(0.90, sum(rate(http_server_request_duration_seconds_bucket{gsid=""$gsid""}[$__interval])) by (le))",
                    YAxisDecimals = 1,
                    Type = "sparkline",
                    Title = "p90",
                    XAxisTitle = "",
                    YAxisTitle = "s",

                },

                #endregion

                new()
                {
                    Id = new Guid("00000203-0000-0000-0000-000000000000"),
                    Query =
                        @"histogram_quantile(0.99, sum(rate(http_server_request_duration_seconds_bucket{gsid=""$gsid""}[$__interval])) by (le))",
                    YAxisDecimals = 1,
                    Title = "p99 - HTTP",
                    XAxisTitle = "",
                    YAxisTitle = "s"
                },
                new()
                {
                    Id = new Guid("00000108-0000-0000-0000-000000000000"),
                    Query =
                        @"histogram_quantile(0.90, sum(rate(http_server_request_duration_seconds_bucket{gsid=""$gsid""}[$__interval])) by (le))",
                    YAxisDecimals = 1,
                    Title = "p90 - HTTP",
                    XAxisTitle = "",
                    YAxisTitle = "s"
                },
                new()
                {
                    Id = new Guid("00000109-0000-0000-0000-000000000000"),
                    Query =
                        @"histogram_quantile(0.50, sum(rate(http_server_request_duration_seconds_bucket{gsid=""$gsid""}[$__interval])) by (le))",
                    YAxisDecimals = 1,
                    Title = "p50 - HTTP",
                    XAxisTitle = "",
                    YAxisTitle = "s"
                },
                new()
                {
                    Id = new Guid("00000112-0000-0000-0000-000000000000"),
                    Query =
                        @"histogram_quantile(0.25, sum(rate(http_server_request_duration_seconds_bucket{gsid=""$gsid""}[$__interval])) by (le))",
                    YAxisDecimals = 1,
                    Title = "p25 - HTTP",
                    XAxisTitle = "",
                    YAxisTitle = "s"
                },
                new()
                {
                    Id = new Guid("00000111-0000-0000-0000-000000000000"),
                    Query =
                        @"histogram_quantile(0, sum(rate(http_server_request_duration_seconds_bucket{gsid=""$gsid""}[$__interval])) by (le))",
                    YAxisDecimals = 1,
                    Title = "p0 - HTTP",
                    XAxisTitle = "",
                    YAxisTitle = "s"
                },


                new()
                {
                    Id = new Guid("00000130-0000-0000-0000-000000000000"),
                    Query =
                        @"histogram_quantile(0.99, sum(rate(if_span_duration_milliseconds_bucket{span_kind=""SPAN_KIND_CONSUMER"", gsid=""$gsid""}[10m])) by (le))",
                    YAxisDecimals = 1,
                    Title = "p99 - Consumer",
                    XAxisTitle = "",
                    YAxisTitle = "s"
                },
                new()
                {
                    Id = new Guid("00000131-0000-0000-0000-000000000000"),
                    Query =
                        @"histogram_quantile(0.90, sum(rate(if_span_duration_milliseconds_bucket{span_kind=""SPAN_KIND_CONSUMER"", gsid=""$gsid""}[10m])) by (le))",
                    YAxisDecimals = 1,
                    Title = "p90 - Consumer",
                    XAxisTitle = "",
                    YAxisTitle = "s"
                },
                new()
                {
                    Id = new Guid("00000132-0000-0000-0000-000000000000"),
                    Query =
                        @"histogram_quantile(0.50, sum(rate(if_span_duration_milliseconds_bucket{span_kind=""SPAN_KIND_CONSUMER"", gsid=""$gsid""}[10m])) by (le))",
                    YAxisDecimals = 1,
                    Title = "p50 - Consumer",
                    XAxisTitle = "",
                    YAxisTitle = "s"
                },
                new()
                {
                    Id = new Guid("00000133-0000-0000-0000-000000000000"),
                    Query =
                        @"histogram_quantile(0.25, sum(rate(if_span_duration_milliseconds_bucket{span_kind=""SPAN_KIND_CONSUMER"", gsid=""$gsid""}[10m])) by (le))",
                    YAxisDecimals = 1,
                    Title = "p25 - Consumer",
                    XAxisTitle = "",
                    YAxisTitle = "s"
                },
                new()
                {
                    Id = new Guid("00000133-0000-0000-0000-000000000000"),
                    Query =
                        @"histogram_quantile(0, sum(rate(if_span_duration_milliseconds_bucket{span_kind=""SPAN_KIND_CONSUMER"", gsid=""$gsid""}[10m])) by (le))",
                    YAxisDecimals = 1,
                    Title = "p0 - Consumer",
                    XAxisTitle = "",
                    YAxisTitle = "s"
                },

                #endregion


                new()
                {
                    Id = new Guid("00000500-0000-0000-0000-000000000000"),
                    Query =
                        @"topk(50,
  sum by (endpoint) (
    label_join(max_over_time(http_server_request_duration_seconds_count{gsid=""$gsid"", http_route!=""""}[5m]), ""endpoint"", "" "", ""http_request_method"", ""http_route"")
  ))",
                    YAxisDecimals = 1,
                    Title = "Top 50 Requested Endpoints",
                    Type = "table",
                    XAxisTitle = "",
                    YAxisTitle = "",
                    DefaultLabel = "endpoint"

                }
            };
            return items;
        }
    }
}