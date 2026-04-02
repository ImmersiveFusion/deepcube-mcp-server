using System.Text.Json;
using FluentAssertions;
using IF.APM.App.Http.Api.Client;
using IF.APM.App.MCP.Server.Tools;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace IF.APM.App.MCP.Server.UnitTests.Tools;

[TestFixture]
public class GeneralToolTests
{
    private IGeneralClient _client = null!;
    private GridAnchor _anchor = null!;

    [SetUp]
    public void SetUp()
    {
        _client = Substitute.For<IGeneralClient>();
        _anchor = new GridAnchor { GridSecondaryId = Guid.NewGuid() };
    }

    [Test]
    public async Task GridInformation_Returns_Valid_Json()
    {
        var grid = new GridViewModel("TestGrid", null, "dev", null);
        _client.GetGridAsync(_anchor.GridSecondaryId).Returns(Task.FromResult(grid));

        var result = await GeneralTool.GridInformation(_client, _anchor);

        var parsed = JsonSerializer.Deserialize<JsonElement>(result);
        parsed.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Test]
    public async Task GridInformation_Passes_Correct_GridSecondaryId()
    {
        _client.GetGridAsync(Arg.Any<Guid>()).Returns(Task.FromResult(new GridViewModel("g", null, "e", null)));

        await GeneralTool.GridInformation(_client, _anchor);

        await _client.Received(1).GetGridAsync(_anchor.GridSecondaryId);
    }

    [Test]
    public async Task GridInformation_Returns_Safe_Error_When_Client_Throws()
    {
        _client.GetGridAsync(Arg.Any<Guid>()).ThrowsAsync(new Exception("Internal server details: host=db01.internal"));

        var result = await GeneralTool.GridInformation(_client, _anchor);

        result.Should().Contain("Failed to retrieve grid information");
        result.Should().NotContain("db01.internal");
    }
}
