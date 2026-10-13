using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using FluentAssertions;
using IF.APM.App.Http.Api.Client;
using IF.APM.App.MCP.Server;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;

namespace IF.APM.App.MCP.Server.UnitTests;

[TestFixture]
public class BootstrapperTests
{
    private ISecurityClient _securityClient = null!;
    private static readonly SymmetricSecurityKey TestKey = new(new byte[32]);

    [SetUp]
    public void SetUp()
    {
        _securityClient = Substitute.For<ISecurityClient>();
    }

    private static string CreateTestJwt(Guid gridSecondaryId, DateTime? expires = null, DateTime? notBefore = null,
        params string[] scopes)
    {
        var handler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var claims = new List<Claim> { new("gsid", gridSecondaryId.ToString()) };
        claims.AddRange((scopes.Length == 0 ? new[] { "member" } : scopes).Select(s => new Claim("scp", s)));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = notBefore ?? now.AddMinutes(-5),
            Expires = expires ?? now.AddHours(1),
            SigningCredentials = new SigningCredentials(TestKey, SecurityAlgorithms.HmacSha256Signature)
        };
        return handler.CreateEncodedJwt(tokenDescriptor);
    }

    private static string CreateTestJwtWithClaims(IEnumerable<Claim> claims, DateTime? expires = null, DateTime? notBefore = null)
    {
        var handler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = notBefore ?? now.AddMinutes(-5),
            Expires = expires ?? now.AddHours(1),
            SigningCredentials = new SigningCredentials(TestKey, SecurityAlgorithms.HmacSha256Signature)
        };
        return handler.CreateEncodedJwt(tokenDescriptor);
    }

    // ── InitializeAsync ──

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task InitializeAsync_Throws_When_No_Credential_Is_Set(string? blank)
    {
        var bootstrapper = new Bootstrapper(_securityClient);

        var act = async () => await bootstrapper.InitializeAsync(blank, blank, null);

        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("*IF_Token*personal access token*");
    }

    [Test]
    public async Task InitializeAsync_Returns_Result_With_Valid_Token()
    {
        var gridId = Guid.NewGuid();
        var token = CreateTestJwt(gridId);
        _securityClient.UnwrapApiKeyAsync(Arg.Any<UnwrapApiKeyModel>())
            .Returns(Task.FromResult(new UnwrappedApiKeyViewModel(token)));

        var bootstrapper = new Bootstrapper(_securityClient);
        var result = await bootstrapper.InitializeAsync(null, "valid-key", null);

        result.GridSecondaryId.Should().Be(gridId);
        result.AccessToken.Should().Be(token);
        result.BaseUrl.Should().Be("https://api-azure.deepcube.ai");
    }

    [Test]
    public async Task InitializeAsync_Uses_Custom_BaseUrl_When_Provided()
    {
        var gridId = Guid.NewGuid();
        var token = CreateTestJwt(gridId);
        _securityClient.UnwrapApiKeyAsync(Arg.Any<UnwrapApiKeyModel>())
            .Returns(Task.FromResult(new UnwrappedApiKeyViewModel(token)));

        var bootstrapper = new Bootstrapper(_securityClient);
        var result = await bootstrapper.InitializeAsync(null, "valid-key", "https://custom.api.com");

        result.BaseUrl.Should().Be("https://custom.api.com");
    }

    // ── a personal access token (IF_Token) ──
    //
    // CR-1 (App.Http PR 4556) makes new and rotated grid keys otlp-only: they may send telemetry and nothing else,
    // so every read this server makes would 403. A member PAT bound to one grid is the credential for reading.

    [Test]
    public async Task InitializeAsync_Uses_A_Personal_Access_Token_Directly()
    {
        var gridId = Guid.NewGuid();
        var token = CreateTestJwt(gridId);

        var result = await new Bootstrapper(_securityClient).InitializeAsync(token, null, null);

        result.GridSecondaryId.Should().Be(gridId);
        result.AccessToken.Should().Be(token);
        await _securityClient.DidNotReceive().UnwrapApiKeyAsync(Arg.Any<UnwrapApiKeyModel>());
    }

    [Test]
    public async Task InitializeAsync_Refuses_A_Token_Not_Bound_To_A_Grid()
    {
        var token = CreateTestJwtWithClaims(new[] { new Claim("scp", "member") });

        var act = async () => await new Bootstrapper(_securityClient).InitializeAsync(token, null, null);

        await act.Should().ThrowAsync<AuthenticationException>().WithMessage("*not bound to a grid*");
    }

    [Test]
    public async Task InitializeAsync_Refuses_A_Token_That_Can_Only_Send_Telemetry()
    {
        var token = CreateTestJwt(Guid.NewGuid(), scopes: "otlp");

        var act = async () => await new Bootstrapper(_securityClient).InitializeAsync(token, null, null);

        await act.Should().ThrowAsync<AuthenticationException>().WithMessage("*only send telemetry*IF_Token*");
    }

    [Test]
    public async Task InitializeAsync_Refuses_A_Grid_Key_That_Can_Only_Send_Telemetry_And_Points_At_A_Pat()
    {
        _securityClient.UnwrapApiKeyAsync(Arg.Any<UnwrapApiKeyModel>())
            .Returns(Task.FromResult(new UnwrappedApiKeyViewModel(CreateTestJwt(Guid.NewGuid(), scopes: "otlp"))));

        var act = async () => await new Bootstrapper(_securityClient).InitializeAsync(null, "new-grid-key", null);

        await act.Should().ThrowAsync<AuthenticationException>().WithMessage("*only send telemetry*personal access token*IF_Token*");
    }

    [Test]
    public async Task InitializeAsync_Still_Accepts_A_Grid_Key_Made_Before_The_Change()
    {
        // Keys created before CR-1 carry member, and keep working until they are rotated.
        var gridId = Guid.NewGuid();
        _securityClient.UnwrapApiKeyAsync(Arg.Any<UnwrapApiKeyModel>())
            .Returns(Task.FromResult(new UnwrappedApiKeyViewModel(CreateTestJwt(gridId, scopes: "member"))));

        var result = await new Bootstrapper(_securityClient).InitializeAsync(null, "old-grid-key", null);

        result.GridSecondaryId.Should().Be(gridId);
    }

    [Test]
    public async Task InitializeAsync_Leaves_A_Token_Without_Scopes_To_The_Api()
    {
        // A token that names no scopes is not narrowed here; the API decides what it may read.
        var gridId = Guid.NewGuid();
        var token = CreateTestJwtWithClaims(new[] { new Claim("gsid", gridId.ToString()) });

        var result = await new Bootstrapper(_securityClient).InitializeAsync(token, null, null);

        result.GridSecondaryId.Should().Be(gridId);
    }

    [Test]
    public async Task InitializeAsync_Refuses_Both_A_Token_And_A_Key()
    {
        var act = async () => await new Bootstrapper(_securityClient)
            .InitializeAsync(CreateTestJwt(Guid.NewGuid()), "a-key", null);

        await act.Should().ThrowAsync<AuthenticationException>().WithMessage("*not both*");
    }

    [Test]
    public async Task InitializeAsync_Refuses_A_Token_That_Is_Not_A_Jwt()
    {
        var act = async () => await new Bootstrapper(_securityClient).InitializeAsync("dck_p_not-yet", null, null);

        await act.Should().ThrowAsync<AuthenticationException>().WithMessage("*IF_Token*not a personal access token*");
    }

    // ── ValidateToken ──

    [Test]
    public void ValidateToken_Throws_When_Token_Is_Expired()
    {
        var token = CreateTestJwt(Guid.NewGuid(),
            expires: DateTime.UtcNow.AddHours(-1),
            notBefore: DateTime.UtcNow.AddHours(-2));

        var act = () => Bootstrapper.ValidateToken(token);

        act.Should().Throw<SecurityTokenExpiredException>();
    }

    [Test]
    public void ValidateToken_Returns_JwtSecurityToken_For_Valid_Token()
    {
        var gridId = Guid.NewGuid();
        var token = CreateTestJwt(gridId);

        var result = Bootstrapper.ValidateToken(token);

        result.Should().BeOfType<JwtSecurityToken>();
        result.Claims.Should().Contain(c => c.Type == "gsid" && c.Value == gridId.ToString());
    }

    // ── ExtractGridSecondaryId ──

    [Test]
    public void ExtractGridSecondaryId_Returns_Guid_From_Gsid_Claim()
    {
        var gridId = Guid.NewGuid();
        var token = CreateTestJwt(gridId);
        var jwtToken = Bootstrapper.ValidateToken(token);

        var result = Bootstrapper.ExtractGridSecondaryId(jwtToken);

        result.Should().Be(gridId);
    }

    [Test]
    public void ExtractGridSecondaryId_Throws_When_Gsid_Claim_Missing()
    {
        var token = CreateTestJwtWithClaims(new[] { new Claim("sub", "test-user") });
        var jwtToken = Bootstrapper.ValidateToken(token);

        var act = () => Bootstrapper.ExtractGridSecondaryId(jwtToken);

        act.Should().Throw<AuthenticationException>()
            .WithMessage("*Grid secondary ID not found*");
    }

    [Test]
    public void ExtractGridSecondaryId_Throws_When_Gsid_Is_Not_Valid_Guid()
    {
        var token = CreateTestJwtWithClaims(new[] { new Claim("gsid", "not-a-guid") });
        var jwtToken = Bootstrapper.ValidateToken(token);

        var act = () => Bootstrapper.ExtractGridSecondaryId(jwtToken);

        act.Should().Throw<AuthenticationException>()
            .WithMessage("*Invalid grid secondary ID format*");
    }
}
