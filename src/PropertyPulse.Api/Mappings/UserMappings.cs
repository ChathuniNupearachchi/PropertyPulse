using PropertyPulse.Domain.Entities;
using PropertyPulse.Shared.Auth;

namespace PropertyPulse.Api.Mappings;

public static class UserMappings
{
    public static UserDto ToDto(this User user) =>
        new(user.Id, user.FullName, user.Email, user.Role.ToString());
}
