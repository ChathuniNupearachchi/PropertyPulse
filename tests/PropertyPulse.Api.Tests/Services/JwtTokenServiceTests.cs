using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PropertyPulse.Api.Settings;
using PropertyPulse.Api.Services;
using PropertyPulse.Domain.Entities;
using PropertyPulse.Domain.Enums;

namespace PropertyPulse.Api.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtOptions _options = new()
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        SigningKey = new string('k', 48),
        LifetimeMinutes = 30
    };

    private readonly User _manager = new()
    {
        FullName = "Test Manager",
        Email = "manager@example.com",
        Role = UserRole.Manager
    };

    [Fact]
    public void CreateToken_ContainsUserIdEmailNameAndRole()
    {
        var token = CreateService().CreateToken(_manager);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token.Value);

        Assert.Equal(_manager.Id.ToString(), jwt.Subject);
        Assert.Equal("manager@example.com", jwt.GetClaim("email").Value);
        Assert.Equal("Test Manager", jwt.GetClaim("name").Value);
        Assert.Equal("Manager", jwt.GetClaim("role").Value);
    }

    [Fact]
    public void CreateToken_ExpiresAfterConfiguredLifetime()
    {
        var before = DateTime.UtcNow;

        var token = CreateService().CreateToken(_manager);

        var after = DateTime.UtcNow;
        Assert.InRange(token.ExpiresAt, before.AddMinutes(30), after.AddMinutes(30));
    }

    [Fact]
    public async Task CreateToken_IsAcceptedWithTheSigningKey_AndRejectedWithAnotherKey()
    {
        var token = CreateService().CreateToken(_manager);
        var handler = new JsonWebTokenHandler();

        var accepted = await handler.ValidateTokenAsync(token.Value, CreateValidationParameters(_options.SigningKey));
        var rejected = await handler.ValidateTokenAsync(token.Value, CreateValidationParameters(new string('x', 48)));

        Assert.True(accepted.IsValid);
        Assert.False(rejected.IsValid);
    }

    private JwtTokenService CreateService() => new(Options.Create(_options));

    private TokenValidationParameters CreateValidationParameters(string signingKey) => new()
    {
        ValidIssuer = _options.Issuer,
        ValidAudience = _options.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
    };
}
