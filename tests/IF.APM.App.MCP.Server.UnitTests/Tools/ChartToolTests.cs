using System.Text.Json;
using FluentAssertions;
using IF.APM.App.Http.Api.Client;
using IF.APM.App.MCP.Server.Tools;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace IF.APM.App.MCP.Server.UnitTests.Tools;

[TestFixture]
public class ChartToolTests
{
    private IAPMClient _client = null!;
    private GridAnchor _anchor = null!;

    [SetUp]
    public void SetUp()
    {
        _client = Substitute.For<IAPMClient>();
        _anchor = new GridAnchor { GridSecondaryId = Guid.NewGuid() };
    }

    private static RangeDataViewModel BuildRangeData() => new(null, null, "success");

    private static DashboardViewModel BuildDashboard() =>
        new(new List<DashboardTileViewModel>(), "Overview", 0);

    // ── GetAvailableCharts ──

    [Test]
    public void GetAvailableCharts_Returns_A_Non_Empty_Array()
    {
        var result = ChartTool.GetAvailableCharts();

        var parsed = JsonSerializer.Deserialize<JsonElement>(result);
        parsed.ValueKind.Should().Be(JsonValueKind.Array);
        parsed.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Test]
    public void GetAvailableCharts_Exposes_Id_And_Title_For_Every_Chart()
    {
        var parsed = JsonSerializer.Deserialize<JsonElement>(ChartTool.GetAvailableCharts());

        foreach (var chart in parsed.EnumerateArray())
        {
            chart.TryGetProperty("Id", out var id).Should().BeTrue();
            id.GetGuid().Should().NotBe(Guid.Empty);
            chart.TryGetProperty("Title", out var title).Should().BeTrue();
            title.GetString().Should().NotBeNullOrWhiteSpace();
        }
    }

    // ── GetRangeData ──

    [Test]
    public async Task GetRangeData_Returns_Valid_Json()
    {
        _client.GetRangeDataAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>())
            .Returns(Task.FromResult(BuildRangeData()));

        var result = await ChartTool.GetRangeData(_client, _anchor, Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow);

        var parsed = JsonSerializer.Deserialize<JsonElement>(result);
        parsed.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Test]
    public async Task GetRangeData_Passes_Correct_Parameters()
    {
        var chartId = Guid.NewGuid();
        var rangeStart = DateTimeOffset.UtcNow.AddHours(-1);
        var rangeEnd = DateTimeOffset.UtcNow;
        _client.GetRangeDataAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>())
            .Returns(Task.FromResult(BuildRangeData()));

        await ChartTool.GetRangeData(_client, _anchor, chartId, rangeStart, rangeEnd);

        await _client.Received(1).GetRangeDataAsync(
            _anchor.GridSecondaryId, chartId, rangeStart, rangeEnd, null, null);
    }

    [Test]
    public async Task GetRangeData_Rejects_Inverted_Range()
    {
        var now = DateTimeOffset.UtcNow;

        var result = await ChartTool.GetRangeData(_client, _anchor, Guid.NewGuid(), now, now.AddHours(-1));

        result.Should().Contain("rangeEnd must be after rangeStart");
    }

    [Test]
    public async Task GetRangeData_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetRangeDataAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>())
            .ThrowsAsync(new Exception("Prometheus at metrics01.internal:9090 refused"));

        var result = await ChartTool.GetRangeData(_client, _anchor, Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow);

        result.Should().Contain("Failed to retrieve chart range data");
        result.Should().NotContain("metrics01.internal");
    }

    // ── GetDashboard ──

    [Test]
    public async Task GetDashboard_Returns_Valid_Json()
    {
        _client.GetDashboardAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns(Task.FromResult(BuildDashboard()));

        var result = await ChartTool.GetDashboard(_client, _anchor, Guid.NewGuid());

        var parsed = JsonSerializer.Deserialize<JsonElement>(result);
        parsed.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Test]
    public async Task GetDashboard_Passes_Correct_Parameters()
    {
        var dashboardId = Guid.NewGuid();
        _client.GetDashboardAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns(Task.FromResult(BuildDashboard()));

        await ChartTool.GetDashboard(_client, _anchor, dashboardId);

        await _client.Received(1).GetDashboardAsync(_anchor.GridSecondaryId, dashboardId);
    }

    [Test]
    public async Task GetDashboard_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetDashboardAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .ThrowsAsync(new Exception("Connection to db01.internal:5432 refused"));

        var result = await ChartTool.GetDashboard(_client, _anchor, Guid.NewGuid());

        result.Should().Contain("Failed to retrieve dashboard");
        result.Should().NotContain("db01.internal");
    }
}
