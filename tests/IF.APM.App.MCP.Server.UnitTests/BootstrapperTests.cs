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

    private static string CreateTestJwt(Guid gridSecondaryId, DateTime? expires = null, DateTime? notBefore = null)
    {
        var handler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("gsid", gridSecondaryId.ToString())
            }),
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

    [Test]
    public void InitializeAsync_Throws_When_ApiKey_Is_Null()
    {
        var bootstrapper = new Bootstrapper(_securityClient);

        var act = async () => await bootstrapper.InitializeAsync(null, null);

        act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("*API Key*");
    }

    [Test]
    public void InitializeAsync_Throws_When_ApiKey_Is_Empty()
    {
        var bootstrapper = new Bootstrapper(_securityClient);

        var act = async () => await bootstrapper.InitializeAsync("", null);

        act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("*API Key*");
    }

    [Test]
    public void InitializeAsync_Throws_When_ApiKey_Is_Whitespace()
    {
        var bootstrapper = new Bootstrapper(_securityClient);

        var act = async () => await bootstrapper.InitializeAsync("   ", null);

        act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("*API Key*");
    }

    [Test]
    public async Task InitializeAsync_Returns_Result_With_Valid_Token()
    {
        var gridId = Guid.NewGuid();
        var token = CreateTestJwt(gridId);
        _securityClient.UnwrapApiKeyAsync(Arg.Any<UnwrapApiKeyModel>())
            .Returns(Task.FromResult(new UnwrappedApiKeyViewModel(token)));

        var bootstrapper = new Bootstrapper(_securityClient);
        var result = await bootstrapper.InitializeAsync("valid-key", null);

        result.GridSecondaryId.Should().Be(gridId);
        result.AccessToken.Should().Be(token);
        result.BaseUrl.Should().Be("https://api-azure.iapm.app");
    }

    [Test]
    public async Task InitializeAsync_Uses_Custom_BaseUrl_When_Provided()
    {
        var gridId = Guid.NewGuid();
        var token = CreateTestJwt(gridId);
        _securityClient.UnwrapApiKeyAsync(Arg.Any<UnwrapApiKeyModel>())
            .Returns(Task.FromResult(new UnwrappedApiKeyViewModel(token)));

        var bootstrapper = new Bootstrapper(_securityClient);
        var result = await bootstrapper.InitializeAsync("valid-key", "https://custom.api.com");

        result.BaseUrl.Should().Be("https://custom.api.com");
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
