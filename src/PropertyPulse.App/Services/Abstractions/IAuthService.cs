using PropertyPulse.Shared.Auth;

namespace PropertyPulse.App.Services;

public interface IAuthService
{
	Task<ApiResult<AuthResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
