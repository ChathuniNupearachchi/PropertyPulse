using PropertyPulse.Shared.Auth;

namespace PropertyPulse.App.Services;

public class AuthService(IApiClient apiClient, ISessionService sessionService) : IAuthService
{
	public async Task<ApiResult<AuthResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
	{
		var result = await apiClient.PostAsync<LoginRequest, AuthResponse>(
			"api/auth/login",
			new LoginRequest(email, password),
			authenticated: false,
			cancellationToken);

		if (result.IsSuccess)
		{
			await sessionService.StartAsync(result.Value!);
		}

		return result;
	}
}
