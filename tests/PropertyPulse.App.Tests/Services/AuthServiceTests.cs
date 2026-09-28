using Moq;
using PropertyPulse.App.Services;
using PropertyPulse.Shared.Auth;

namespace PropertyPulse.App.Tests;

public class AuthServiceTests
{
	private readonly Mock<IApiClient> _apiClient = new();
	private readonly Mock<ISessionService> _sessionService = new();
	private readonly AuthService _authService;

	public AuthServiceTests()
	{
		_authService = new AuthService(_apiClient.Object, _sessionService.Object);
	}

	[Fact]
	public async Task LoginAsync_OnSuccess_StartsTheSession()
	{
		var response = new AuthResponse("token", DateTime.UtcNow.AddHours(1), new UserDto(Guid.NewGuid(), "Nimali Perera", "manager@propertypulse.lk", "Manager"));
		SetupLogin(ApiResult<AuthResponse>.Success(response));

		var result = await _authService.LoginAsync("manager@propertypulse.lk", "Password123!");

		Assert.True(result.IsSuccess);
		_sessionService.Verify(s => s.StartAsync(response), Times.Once);
	}

	[Fact]
	public async Task LoginAsync_OnFailure_DoesNotStartASession()
	{
		SetupLogin(ApiResult<AuthResponse>.Failure(ApiError.Unauthorized, "Invalid email or password."));

		var result = await _authService.LoginAsync("manager@propertypulse.lk", "wrong");

		Assert.Equal(ApiError.Unauthorized, result.Error);
		_sessionService.Verify(s => s.StartAsync(It.IsAny<AuthResponse>()), Times.Never);
	}

	[Fact]
	public async Task LoginAsync_PostsTheCredentialsWithoutAToken()
	{
		LoginRequest? sent = null;
		_apiClient
			.Setup(c => c.PostAsync<LoginRequest, AuthResponse>("api/auth/login", It.IsAny<LoginRequest>(), false, It.IsAny<CancellationToken>()))
			.Callback<string, LoginRequest, bool, CancellationToken>((_, body, _, _) => sent = body)
			.ReturnsAsync(ApiResult<AuthResponse>.Failure(ApiError.Network));

		await _authService.LoginAsync("agent1@propertypulse.lk", "Password123!");

		Assert.Equal(new LoginRequest("agent1@propertypulse.lk", "Password123!"), sent);
	}

	private void SetupLogin(ApiResult<AuthResponse> result) =>
		_apiClient
			.Setup(c => c.PostAsync<LoginRequest, AuthResponse>("api/auth/login", It.IsAny<LoginRequest>(), false, It.IsAny<CancellationToken>()))
			.ReturnsAsync(result);
}
