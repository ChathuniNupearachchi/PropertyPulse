using PropertyPulse.Api.Mappings;
using PropertyPulse.Infrastructure.Repositories;
using PropertyPulse.Shared.Auth;

namespace PropertyPulse.Api.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByIdAsync(id, cancellationToken);
        return user?.ToDto();
    }

    public async Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.ListAsync(cancellationToken);
        return users.Select(u => u.ToDto()).ToList();
    }
}
