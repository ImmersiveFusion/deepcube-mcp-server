using System.Text.Json;
using FluentAssertions;
using IF.APM.App.Http.Api.Client;
using IF.APM.App.MCP.Server.Tools;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace IF.APM.App.MCP.Server.UnitTests.Tools;

[TestFixture]
public class ApmToolTests
{
    private IAPMClient _client = null!;
    private GridAnchor _anchor = null!;

    [SetUp]
    public void SetUp()
    {
        _client = Substitute.For<IAPMClient>();
        _anchor = new GridAnchor { GridSecondaryId = Guid.NewGuid() };
    }

    private static TraceListViewModel BuildTraceList() =>
        new(new List<TraceRowViewModel>(), null, null);

    private static TraceRowViewModel BuildTraceRow() =>
        new(null, null, new List<OperationError>(), "", "", "", null, "", null, null, new List<Tag>(), "", null);

    private static SpanListViewModel BuildSpanList() =>
        new(new List<SpanRowViewModel>(), null, null);

    private static SpanTagListViewModel BuildSpanTagList() =>
        new(new List<SpanTagRowViewModel>(), null);

    private static TraceErrorListViewModel BuildTraceErrorList() =>
        new(new List<TraceErrorRowViewModel>(), null, null);

    private static LogListViewModel BuildLogList() =>
        new(new List<LogRowViewModel>(), null, null);

    private static LogRowViewModel BuildLogRow() =>
        new("", null, new List<OperationError>(), "", null, "", null, "", null, new List<Tag>(), "", null);

    // ── GetTraces ──

    [Test]
    public async Task GetTraces_Returns_Valid_Json()
    {
        _client.GetTracesAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<TraceListFilterModel>())
            .Returns(Task.FromResult(BuildTraceList()));

        var result = await ApmTool.GetTraces(_client, _anchor, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow);

        var act = () => JsonSerializer.Deserialize<JsonElement>(result);
        act.Should().NotThrow();
    }

    [Test]
    public async Task GetTraces_Passes_Correct_Parameters()
    {
        var rangeStart = DateTimeOffset.UtcNow.AddHours(-1);
        var rangeEnd = DateTimeOffset.UtcNow;
        _client.GetTracesAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<TraceListFilterModel>())
            .Returns(Task.FromResult(BuildTraceList()));

        await ApmTool.GetTraces(_client, _anchor, rangeStart, rangeEnd);

        await _client.Received(1).GetTracesAsync(
            _anchor.GridSecondaryId,
            rangeStart,
            rangeEnd,
            Arg.Any<TraceListFilterModel>());
    }

    [Test]
    public async Task GetTraces_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetTracesAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<TraceListFilterModel>())
            .ThrowsAsync(new Exception("Connection to db01.internal:5432 refused"));

        var result = await ApmTool.GetTraces(_client, _anchor, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow);

        result.Should().Contain("Failed to retrieve traces");
        result.Should().NotContain("db01.internal");
    }

    [Test]
    public async Task GetTraces_Rejects_Invalid_Time_Range()
    {
        var result = await ApmTool.GetTraces(_client, _anchor, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(-1));

        result.Should().Contain("rangeEnd must be after rangeStart");
    }

    [Test]
    public async Task GetTraces_Rejects_Excessive_Time_Range()
    {
        var result = await ApmTool.GetTraces(_client, _anchor, DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow);

        result.Should().Contain("Time range exceeds maximum");
    }

    // ── GetTrace ──

    [Test]
    public async Task GetTrace_Returns_Valid_Json()
    {
        _client.GetTraceAsync(_anchor.GridSecondaryId, "a1b2c3d4e5f60718")
            .Returns(Task.FromResult(BuildTraceRow()));

        var result = await ApmTool.GetTrace(_client, _anchor, "a1b2c3d4e5f60718");

        var act = () => JsonSerializer.Deserialize<JsonElement>(result);
        act.Should().NotThrow();
    }

    [Test]
    public async Task GetTrace_Passes_Correct_SpanId()
    {
        var spanId = "a1b2c3d4e5f60718";
        _client.GetTraceAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(Task.FromResult(BuildTraceRow()));

        await ApmTool.GetTrace(_client, _anchor, spanId);

        await _client.Received(1).GetTraceAsync(_anchor.GridSecondaryId, spanId);
    }

    [Test]
    public async Task GetTrace_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetTraceAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .ThrowsAsync(new Exception("Internal error"));

        var result = await ApmTool.GetTrace(_client, _anchor, "abc123");

        result.Should().Contain("Failed to retrieve trace");
        result.Should().NotContain("Internal error");
    }

    [Test]
    public async Task GetTrace_Rejects_Invalid_SpanId()
    {
        var result = await ApmTool.GetTrace(_client, _anchor, "not-a-hex-value!");

        result.Should().Contain("Invalid spanId format");
    }

    [Test]
    public async Task GetTrace_Rejects_Empty_SpanId()
    {
        var result = await ApmTool.GetTrace(_client, _anchor, "");

        result.Should().Contain("Invalid spanId format");
    }

    // ── GetTraceSpanTags ──

    [Test]
    public async Task GetTraceSpanTags_Returns_Valid_Json()
    {
        _client.GetTraceSpanTagsAsync(_anchor.GridSecondaryId, "span1")
            .Returns(Task.FromResult(BuildSpanTagList()));

        var result = await ApmTool.GetTraceSpanTags(_client, _anchor, "span1");

        var act = () => JsonSerializer.Deserialize<JsonElement>(result);
        act.Should().NotThrow();
    }

    [Test]
    public async Task GetTraceSpanTags_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetTraceSpanTagsAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .ThrowsAsync(new Exception("Span not found"));

        var result = await ApmTool.GetTraceSpanTags(_client, _anchor, "abc123");

        result.Should().Contain("Failed to retrieve span tags");
    }

    [Test]
    public async Task GetTraceSpanTags_Rejects_Invalid_SpanId()
    {
        var result = await ApmTool.GetTraceSpanTags(_client, _anchor, "invalid!");

        result.Should().Contain("Invalid spanId format");
    }

    // ── GetTraceSpans ──

    [Test]
    public async Task GetTraceSpans_Returns_Valid_Json()
    {
        _client.GetTraceSpansAsync(_anchor.GridSecondaryId, "trace1")
            .Returns(Task.FromResult(BuildSpanList()));

        var result = await ApmTool.GetTraceSpans(_client, _anchor, "trace1");

        var act = () => JsonSerializer.Deserialize<JsonElement>(result);
        act.Should().NotThrow();
    }

    [Test]
    public async Task GetTraceSpans_Passes_Correct_TraceId()
    {
        var traceId = "a1b2c3d4e5f60718a1b2c3d4e5f60718";
        _client.GetTraceSpansAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(Task.FromResult(BuildSpanList()));

        await ApmTool.GetTraceSpans(_client, _anchor, traceId);

        await _client.Received(1).GetTraceSpansAsync(_anchor.GridSecondaryId, traceId);
    }

    [Test]
    public async Task GetTraceSpans_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetTraceSpansAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .ThrowsAsync(new Exception("Trace not found"));

        var result = await ApmTool.GetTraceSpans(_client, _anchor, "abc");

        result.Should().Contain("Failed to retrieve trace spans");
    }

    [Test]
    public async Task GetTraceSpans_Rejects_Invalid_TraceId()
    {
        var result = await ApmTool.GetTraceSpans(_client, _anchor, "invalid/path");

        result.Should().Contain("Invalid traceId format");
    }

    // ── GetTraceSpanErrors ──

    [Test]
    public async Task GetTraceSpanErrors_Returns_Valid_Json()
    {
        _client.GetTraceErrorsAsync(_anchor.GridSecondaryId, "trace1")
            .Returns(Task.FromResult(BuildTraceErrorList()));

        var result = await ApmTool.GetTraceSpanErrors(_client, _anchor, "trace1");

        var act = () => JsonSerializer.Deserialize<JsonElement>(result);
        act.Should().NotThrow();
    }

    [Test]
    public async Task GetTraceSpanErrors_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetTraceErrorsAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .ThrowsAsync(new Exception("Server error"));

        var result = await ApmTool.GetTraceSpanErrors(_client, _anchor, "abc");

        result.Should().Contain("Failed to retrieve trace errors");
    }

    [Test]
    public async Task GetTraceSpanErrors_Rejects_Invalid_TraceId()
    {
        var result = await ApmTool.GetTraceSpanErrors(_client, _anchor, "../../../etc/passwd");

        result.Should().Contain("Invalid traceId format");
    }

    // ── GetLogs ──

    [Test]
    public async Task GetLogs_Returns_Valid_Json()
    {
        _client.GetLogsAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<LogListFilterModel>())
            .Returns(Task.FromResult(BuildLogList()));

        var result = await ApmTool.GetLogs(_client, _anchor, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow);

        var act = () => JsonSerializer.Deserialize<JsonElement>(result);
        act.Should().NotThrow();
    }

    [Test]
    public async Task GetLogs_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetLogsAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<LogListFilterModel>())
            .ThrowsAsync(new Exception("Unauthorized"));

        var result = await ApmTool.GetLogs(_client, _anchor, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow);

        result.Should().Contain("Failed to retrieve logs");
        result.Should().NotContain("Unauthorized");
    }

    [Test]
    public async Task GetLogs_Rejects_Invalid_Time_Range()
    {
        var result = await ApmTool.GetLogs(_client, _anchor, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(-1));

        result.Should().Contain("rangeEnd must be after rangeStart");
    }

    // ── GetLog ──

    [Test]
    public async Task GetLog_Returns_Valid_Json()
    {
        _client.GetLogAsync(_anchor.GridSecondaryId, "span1")
            .Returns(Task.FromResult(BuildLogRow()));

        var result = await ApmTool.GetLog(_client, _anchor, "span1");

        var act = () => JsonSerializer.Deserialize<JsonElement>(result);
        act.Should().NotThrow();
    }

    [Test]
    public async Task GetLog_Passes_Correct_SpanId()
    {
        var spanId = "a1b2c3d4e5f60718";
        _client.GetLogAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(Task.FromResult(BuildLogRow()));

        await ApmTool.GetLog(_client, _anchor, spanId);

        await _client.Received(1).GetLogAsync(_anchor.GridSecondaryId, spanId);
    }

    [Test]
    public async Task GetLog_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetLogAsync(Arg.Any<Guid>(), Arg.Any<string>())
            .ThrowsAsync(new Exception("Not found"));

        var result = await ApmTool.GetLog(_client, _anchor, "abc");

        result.Should().Contain("Failed to retrieve log");
        result.Should().NotContain("Not found");
    }

    [Test]
    public async Task GetLog_Rejects_Invalid_SpanId()
    {
        var result = await ApmTool.GetLog(_client, _anchor, "invalid;DROP TABLE");

        result.Should().Contain("Invalid spanId format");
    }
}
