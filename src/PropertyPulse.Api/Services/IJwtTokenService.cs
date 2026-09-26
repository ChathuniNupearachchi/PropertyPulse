using PropertyPulse.Domain.Entities;

namespace PropertyPulse.Api.Services;

public interface IJwtTokenService
{
    AccessToken CreateToken(User user);
}
