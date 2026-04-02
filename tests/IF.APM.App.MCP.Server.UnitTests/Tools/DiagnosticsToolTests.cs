using System.Text.Json;
using FluentAssertions;
using IF.APM.App.Http.Api.Client;
using IF.APM.App.MCP.Server.Tools;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace IF.APM.App.MCP.Server.UnitTests.Tools;

[TestFixture]
public class DiagnosticsToolTests
{
    private IAPMClient _client = null!;
    private GridAnchor _anchor = null!;

    [SetUp]
    public void SetUp()
    {
        _client = Substitute.For<IAPMClient>();
        _anchor = new GridAnchor { GridSecondaryId = Guid.NewGuid() };
    }

    private static SystemHealthViewModel BuildSystemHealth(
        string status = "healthy", double apdex = 0.95,
        ICollection<ServiceHealthRowViewModel>? services = null) =>
        new(3, apdex, 0, services ?? new List<ServiceHealthRowViewModel>(),
            DateTimeOffset.UtcNow.AddMinutes(-15), status, "All systems operational",
            new List<TopErrorViewModel>());

    // ── GetSystemHealth ──

    [Test]
    public async Task GetSystemHealth_Returns_Valid_Json()
    {
        _client.GetSystemHealthAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .Returns(Task.FromResult(BuildSystemHealth()));

        var result = await DiagnosticsTool.GetSystemHealth(_client, _anchor);

        var parsed = JsonSerializer.Deserialize<JsonElement>(result);
        parsed.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Test]
    public async Task GetSystemHealth_Passes_Correct_Parameters()
    {
        var start = DateTimeOffset.UtcNow.AddMinutes(-15);
        var end = DateTimeOffset.UtcNow;
        _client.GetSystemHealthAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .Returns(Task.FromResult(BuildSystemHealth()));

        await DiagnosticsTool.GetSystemHealth(_client, _anchor, start, end);

        await _client.Received(1).GetSystemHealthAsync(_anchor.GridSecondaryId, start, end);
    }

    [Test]
    public async Task GetSystemHealth_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetSystemHealthAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .ThrowsAsync(new Exception("Internal details"));

        var result = await DiagnosticsTool.GetSystemHealth(_client, _anchor);

        result.Should().Contain("Failed to retrieve system health");
        result.Should().NotContain("Internal details");
    }

    // ── GetDiagnosis ──

    [Test]
    public async Task GetDiagnosis_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetDiagnosisAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .ThrowsAsync(new Exception("error"));

        var result = await DiagnosticsTool.GetDiagnosis(_client, _anchor);

        result.Should().Contain("Failed to retrieve diagnosis");
    }

    // ── GetServiceMap ──

    [Test]
    public async Task GetServiceMap_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetServiceMapAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .ThrowsAsync(new Exception("error"));

        var result = await DiagnosticsTool.GetServiceMap(_client, _anchor);

        result.Should().Contain("Failed to retrieve service map");
    }

    // ── GetPressurePoints ──

    [Test]
    public async Task GetPressurePoints_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetPressurePointsAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .ThrowsAsync(new Exception("error"));

        var result = await DiagnosticsTool.GetPressurePoints(_client, _anchor);

        result.Should().Contain("Failed to retrieve pressure points");
    }

    // ── GetTrendAnalysis ──

    [Test]
    public async Task GetTrendAnalysis_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetTrendAnalysisAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .ThrowsAsync(new Exception("error"));

        var result = await DiagnosticsTool.GetTrendAnalysis(_client, _anchor);

        result.Should().Contain("Failed to retrieve trend analysis");
    }

    // ── GetAlertSummary ──

    [Test]
    public async Task GetAlertSummary_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetAlertSummaryAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .ThrowsAsync(new Exception("error"));

        var result = await DiagnosticsTool.GetAlertSummary(_client, _anchor);

        result.Should().Contain("Failed to retrieve alert summary");
    }

    // ── Tier 2 Tools ──

    [Test]
    public async Task GetIncidentTimeline_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetIncidentTimelineAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .ThrowsAsync(new Exception("error"));

        var result = await DiagnosticsTool.GetIncidentTimeline(_client, _anchor);

        result.Should().Contain("Failed to retrieve incident timeline");
    }

    [Test]
    public async Task GetServiceDetail_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetServiceDetailAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .ThrowsAsync(new Exception("error"));

        var result = await DiagnosticsTool.GetServiceDetail(_client, _anchor, "svc");

        result.Should().Contain("Failed to retrieve service detail");
    }

    [Test]
    public async Task GetSlowestEndpoints_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetSlowestEndpointsAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .ThrowsAsync(new Exception("error"));

        var result = await DiagnosticsTool.GetSlowestEndpoints(_client, _anchor);

        result.Should().Contain("Failed to retrieve slowest endpoints");
    }

    [Test]
    public async Task GetDeploymentChanges_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetDeploymentChangesAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .ThrowsAsync(new Exception("error"));

        var result = await DiagnosticsTool.GetDeploymentChanges(_client, _anchor);

        result.Should().Contain("Failed to retrieve deployment changes");
    }

    [Test]
    public async Task GetComparison_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetComparisonAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>())
            .ThrowsAsync(new Exception("error"));

        var result = await DiagnosticsTool.GetComparison(_client, _anchor);

        result.Should().Contain("Failed to retrieve comparison");
    }
}
