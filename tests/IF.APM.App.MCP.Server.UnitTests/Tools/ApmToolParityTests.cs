using System.Text.Json;
using FluentAssertions;
using IF.APM.App.Http.Api.Client;
using IF.APM.App.MCP.Server.Tools;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace IF.APM.App.MCP.Server.UnitTests.Tools;

/// <summary>
/// Covers the ApmTool entry points added to reach parity with the Unity
/// client's ApmTools.cs: span events, trace logs, and the two facet tools.
/// </summary>
[TestFixture]
public class ApmToolParityTests
{
    private IAPMClient _client = null!;
    private GridAnchor _anchor = null!;

    [SetUp]
    public void SetUp()
    {
        _client = Substitute.For<IAPMClient>();
        _anchor = new GridAnchor { GridSecondaryId = Guid.NewGuid() };
    }

    private static SpanEventListViewModel BuildSpanEventList() =>
        new(new List<SpanEventRowViewModel>(), 0);

    private static TraceLogListViewModel BuildTraceLogList() =>
        new(new List<TraceLogRowViewModel>(), false, 0);

    private static TraceFilterListViewModel BuildTraceFilterList() =>
        new(new Dictionary<string, ICollection<Facet>>());

    private static LogFilterListViewModel BuildLogFilterList() =>
        new(new Dictionary<string, ICollection<Facet>>());

    // ── GetTraceSpanEvents ──

    [Test]
    public async Task GetTraceSpanEvents_Returns_Valid_Json()
    {
        _client.GetTraceSpanEventsAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(Task.FromResult(BuildSpanEventList()));

        var result = await ApmTool.GetTraceSpanEvents(_client, _anchor, "a1b2c3d4e5f60718");

        var parsed = JsonSerializer.Deserialize<JsonElement>(result);
        parsed.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Test]
    public async Task GetTraceSpanEvents_Passes_Correct_Parameters()
    {
        _client.GetTraceSpanEventsAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(Task.FromResult(BuildSpanEventList()));

        await ApmTool.GetTraceSpanEvents(_client, _anchor, "a1b2c3d4e5f60718");

        await _client.Received(1).GetTraceSpanEventsAsync(_anchor.GridSecondaryId, "a1b2c3d4e5f60718");
    }

    [Test]
    public async Task GetTraceSpanEvents_Rejects_Invalid_SpanId()
    {
        var result = await ApmTool.GetTraceSpanEvents(_client, _anchor, "../../../etc/passwd");

        result.Should().Contain("Invalid spanId format");
        await _client.DidNotReceive().GetTraceSpanEventsAsync(Arg.Any<Guid>(), Arg.Any<string>());
    }

    [Test]
    public async Task GetTraceSpanEvents_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetTraceSpanEventsAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .ThrowsAsync(new Exception("Connection to db01.internal:5432 refused"));

        var result = await ApmTool.GetTraceSpanEvents(_client, _anchor, "abc");

        result.Should().Contain("Failed to retrieve span events");
        result.Should().NotContain("db01.internal");
    }

    // ── GetTraceLogs ──

    [Test]
    public async Task GetTraceLogs_Returns_Valid_Json()
    {
        _client.GetTraceLogsAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(Task.FromResult(BuildTraceLogList()));

        var result = await ApmTool.GetTraceLogs(_client, _anchor, "a1b2c3d4e5f60718a1b2c3d4e5f60718");

        var parsed = JsonSerializer.Deserialize<JsonElement>(result);
        parsed.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Test]
    public async Task GetTraceLogs_Passes_Correct_Parameters()
    {
        _client.GetTraceLogsAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(Task.FromResult(BuildTraceLogList()));

        await ApmTool.GetTraceLogs(_client, _anchor, "a1b2c3d4e5f60718a1b2c3d4e5f60718");

        await _client.Received(1).GetTraceLogsAsync(_anchor.GridSecondaryId, "a1b2c3d4e5f60718a1b2c3d4e5f60718");
    }

    [Test]
    public async Task GetTraceLogs_Rejects_Invalid_TraceId()
    {
        var result = await ApmTool.GetTraceLogs(_client, _anchor, "not a trace id");

        result.Should().Contain("Invalid traceId format");
        await _client.DidNotReceive().GetTraceLogsAsync(Arg.Any<Guid>(), Arg.Any<string>());
    }

    [Test]
    public async Task GetTraceLogs_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetTraceLogsAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .ThrowsAsync(new Exception("Connection to db01.internal:5432 refused"));

        var result = await ApmTool.GetTraceLogs(_client, _anchor, "abc");

        result.Should().Contain("Failed to retrieve trace logs");
        result.Should().NotContain("db01.internal");
    }

    // ── GetTraceFilterFacets ──

    [Test]
    public async Task GetTraceFilterFacets_Returns_Valid_Json()
    {
        _client.GetTraceFiltersAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .Returns(Task.FromResult(BuildTraceFilterList()));

        var result = await ApmTool.GetTraceFilterFacets(_client, _anchor,
            DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow);

        var parsed = JsonSerializer.Deserialize<JsonElement>(result);
        parsed.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Test]
    public async Task GetTraceFilterFacets_Passes_Correct_Parameters()
    {
        var rangeStart = DateTimeOffset.UtcNow.AddHours(-1);
        var rangeEnd = DateTimeOffset.UtcNow;
        _client.GetTraceFiltersAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .Returns(Task.FromResult(BuildTraceFilterList()));

        await ApmTool.GetTraceFilterFacets(_client, _anchor, rangeStart, rangeEnd);

        await _client.Received(1).GetTraceFiltersAsync(_anchor.GridSecondaryId, rangeStart, rangeEnd);
    }

    [Test]
    public async Task GetTraceFilterFacets_Rejects_Excessive_Range()
    {
        var result = await ApmTool.GetTraceFilterFacets(_client, _anchor,
            DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow);

        result.Should().Contain("Time range exceeds maximum");
    }

    [Test]
    public async Task GetTraceFilterFacets_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetTraceFiltersAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .ThrowsAsync(new Exception("Connection to db01.internal:5432 refused"));

        var result = await ApmTool.GetTraceFilterFacets(_client, _anchor,
            DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow);

        result.Should().Contain("Failed to retrieve trace filter facets");
        result.Should().NotContain("db01.internal");
    }

    // ── GetLogFilterFacets ──

    [Test]
    public async Task GetLogFilterFacets_Returns_Valid_Json()
    {
        _client.GetLogFiltersAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .Returns(Task.FromResult(BuildLogFilterList()));

        var result = await ApmTool.GetLogFilterFacets(_client, _anchor,
            DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow);

        var parsed = JsonSerializer.Deserialize<JsonElement>(result);
        parsed.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Test]
    public async Task GetLogFilterFacets_Passes_Correct_Parameters()
    {
        var rangeStart = DateTimeOffset.UtcNow.AddHours(-1);
        var rangeEnd = DateTimeOffset.UtcNow;
        _client.GetLogFiltersAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .Returns(Task.FromResult(BuildLogFilterList()));

        await ApmTool.GetLogFilterFacets(_client, _anchor, rangeStart, rangeEnd);

        await _client.Received(1).GetLogFiltersAsync(_anchor.GridSecondaryId, rangeStart, rangeEnd);
    }

    [Test]
    public async Task GetLogFilterFacets_Rejects_Inverted_Range()
    {
        var now = DateTimeOffset.UtcNow;

        var result = await ApmTool.GetLogFilterFacets(_client, _anchor, now, now.AddHours(-1));

        result.Should().Contain("rangeEnd must be after rangeStart");
    }

    [Test]
    public async Task GetLogFilterFacets_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetLogFiltersAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .ThrowsAsync(new Exception("Connection to db01.internal:5432 refused"));

        var result = await ApmTool.GetLogFilterFacets(_client, _anchor,
            DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow);

        result.Should().Contain("Failed to retrieve log filter facets");
        result.Should().NotContain("db01.internal");
    }

    // ── UTC normalisation (mirrors ApmToolContext.ToUtc in the Unity client) ──

    [Test]
    public async Task GetTraces_Converts_Non_Utc_Range_To_Utc_Before_Calling_The_Api()
    {
        var rangeStart = new DateTimeOffset(2026, 3, 19, 9, 0, 0, TimeSpan.FromHours(-5));
        var rangeEnd = new DateTimeOffset(2026, 3, 19, 10, 0, 0, TimeSpan.FromHours(-5));
        _client.GetTracesAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(),
                Arg.Any<TraceListFilterModel>())
            .Returns(Task.FromResult(new TraceListViewModel(new List<TraceRowViewModel>(), null, null)));

        await ApmTool.GetTraces(_client, _anchor, rangeStart, rangeEnd);

        await _client.Received(1).GetTracesAsync(
            _anchor.GridSecondaryId,
            Arg.Is<DateTimeOffset?>(v => v!.Value.Offset == TimeSpan.Zero && v.Value.Hour == 14),
            Arg.Is<DateTimeOffset?>(v => v!.Value.Offset == TimeSpan.Zero && v.Value.Hour == 15),
            Arg.Any<TraceListFilterModel>());
    }
}
