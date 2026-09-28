using System.Net;
using Moq;
using PropertyPulse.App.Services;
using PropertyPulse.Shared.Auth;

namespace PropertyPulse.App.Tests;

public class ApiClientTests
{
	private readonly Mock<IApiSettings> _apiSettings = new();
	private readonly Mock<ISessionService> _sessionService = new();

	public ApiClientTests()
	{
		_apiSettings.SetupProperty(s => s.BaseUrl, "http://localhost:5000");
		_sessionService.SetupGet(s => s.Current).Returns(
			new AuthResponse("test-token", DateTime.UtcNow.AddHours(1), new UserDto(Guid.NewGuid(), "Kasun Fernando", "agent1@propertypulse.lk", "Agent")));
	}

	[Fact]
	public async Task GetAsync_WhenSignedIn_AttachesBearerToken()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, "{}");

		await CreateClient(handler).GetAsync<UserDto>("api/auth/me");

		Assert.Equal("Bearer test-token", handler.Requests.Single().Authorization);
	}

	[Fact]
	public async Task PostAsync_WhenNotAuthenticated_SendsNoAuthorizationHeader()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, "{}");

		await CreateClient(handler).PostAsync<LoginRequest, UserDto>("api/auth/login", new LoginRequest("a@b.com", "secret"), authenticated: false);

		var request = handler.Requests.Single();
		Assert.Null(request.Authorization);
		Assert.Contains("\"email\":\"a@b.com\"", request.Body);
	}

	[Fact]
	public async Task GetAsync_UsesTheCurrentBaseUrlForEveryRequest()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, "{}");
		var client = CreateClient(handler);

		await client.GetAsync<UserDto>("api/users");
		_apiSettings.Object.BaseUrl = "http://192.168.1.20:5000/";
		await client.GetAsync<UserDto>("/api/users");

		Assert.Equal("http://localhost:5000/api/users", handler.Requests[0].Uri?.ToString());
		Assert.Equal("http://192.168.1.20:5000/api/users", handler.Requests[1].Uri?.ToString());
	}

	[Fact]
	public async Task GetAsync_WithSuccessResponse_ReturnsTheDeserializedValue()
	{
		var id = Guid.NewGuid();
		var handler = FakeHttpMessageHandler.Returning(
			HttpStatusCode.OK,
			$$"""{"id":"{{id}}","fullName":"Kasun Fernando","email":"agent1@propertypulse.lk","role":"Agent"}""");

		var result = await CreateClient(handler).GetAsync<UserDto>("api/auth/me");

		Assert.True(result.IsSuccess);
		Assert.Equal(new UserDto(id, "Kasun Fernando", "agent1@propertypulse.lk", "Agent"), result.Value);
	}

	[Fact]
	public async Task GetAsync_WithUnreadableSuccessBody_ReturnsServerError()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, "not json");

		var result = await CreateClient(handler).GetAsync<UserDto>("api/auth/me");

		Assert.Equal(ApiError.Server, result.Error);
	}

	[Fact]
	public async Task GetAsync_WhenUnauthorizedWhileSignedIn_EndsTheSessionAsExpired()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.Unauthorized);

		var result = await CreateClient(handler).GetAsync<UserDto>("api/auth/me");

		Assert.Equal(ApiError.Unauthorized, result.Error);
		_sessionService.Verify(s => s.EndAsync(SessionEndReason.Expired), Times.Once);
	}

	[Fact]
	public async Task PostAsync_WhenNotAuthenticatedAndUnauthorized_KeepsTheSession()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.Unauthorized, """{"title":"Invalid credentials","detail":"Invalid email or password."}""");

		var result = await CreateClient(handler).PostAsync<LoginRequest, UserDto>("api/auth/login", new LoginRequest("a@b.com", "wrong"), authenticated: false);

		Assert.Equal(ApiError.Unauthorized, result.Error);
		_sessionService.Verify(s => s.EndAsync(It.IsAny<SessionEndReason>()), Times.Never);
	}

	[Fact]
	public async Task GetAsync_WhenForbidden_KeepsTheSession()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.Forbidden);

		var result = await CreateClient(handler).GetAsync<UserDto>("api/users");

		Assert.Equal(ApiError.Forbidden, result.Error);
		_sessionService.Verify(s => s.EndAsync(It.IsAny<SessionEndReason>()), Times.Never);
	}

	[Fact]
	public async Task GetAsync_WhenTheServerIsUnreachable_ReturnsNetworkErrorWithoutThrowing()
	{
		var handler = FakeHttpMessageHandler.Throwing(new HttpRequestException("Connection refused"));

		var result = await CreateClient(handler).GetAsync<UserDto>("api/auth/me");

		Assert.Equal(ApiError.Network, result.Error);
	}

	[Fact]
	public async Task GetAsync_WhenTheRequestTimesOut_ReturnsNetworkError()
	{
		var handler = FakeHttpMessageHandler.Throwing(new TaskCanceledException("The request timed out.", new TimeoutException()));

		var result = await CreateClient(handler).GetAsync<UserDto>("api/auth/me");

		Assert.Equal(ApiError.Network, result.Error);
	}

	[Fact]
	public async Task GetAsync_WhenTheCallerCancels_Throws()
	{
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		var handler = new FakeHttpMessageHandler(_ => throw new TaskCanceledException());

		await Assert.ThrowsAsync<TaskCanceledException>(() => CreateClient(handler).GetAsync<UserDto>("api/auth/me", cancellation.Token));
	}

	[Fact]
	public async Task GetAsync_WithProblemDetailsBody_UsesDetailAsTheMessage()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.BadRequest, """{"title":"Bad request","detail":"Email is invalid."}""");

		var result = await CreateClient(handler).GetAsync<UserDto>("api/thing");

		Assert.Equal(ApiError.Rejected, result.Error);
		Assert.Equal("Email is invalid.", result.Message);
	}

	[Fact]
	public async Task GetAsync_WithOnlyATitle_UsesTheTitleAsTheMessage()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.Conflict, """{"title":"Email already in use"}""");

		var result = await CreateClient(handler).GetAsync<UserDto>("api/thing");

		Assert.Equal("Email already in use", result.Message);
	}

	[Fact]
	public async Task GetAsync_WhenTheServerFails_ReturnsServerError()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.InternalServerError, """{"title":"An unexpected error occurred"}""");

		var result = await CreateClient(handler).GetAsync<UserDto>("api/auth/me");

		Assert.Equal(ApiError.Server, result.Error);
	}

	[Fact]
	public async Task GetAsync_WithAnInvalidBaseUrl_ReturnsNetworkError()
	{
		_apiSettings.Object.BaseUrl = "not a url";
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, "{}");

		var result = await CreateClient(handler).GetAsync<UserDto>("api/auth/me");

		Assert.Equal(ApiError.Network, result.Error);
		Assert.Empty(handler.Requests);
	}

	[Fact]
	public async Task PutAsync_AttachesBearerTokenAndSendsTheBody()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, "{}");

		await CreateClient(handler).PutAsync<LoginRequest, UserDto>("api/auth/login", new LoginRequest("a@b.com", "secret"));

		var request = handler.Requests.Single();
		Assert.Equal("Bearer test-token", request.Authorization);
		Assert.Contains("\"email\":\"a@b.com\"", request.Body);
	}

	[Fact]
	public async Task PutAsync_WhenUnauthorized_EndsTheSession()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.Unauthorized);

		var result = await CreateClient(handler).PutAsync<LoginRequest, UserDto>("api/thing", new LoginRequest("a@b.com", "secret"));

		Assert.Equal(ApiError.Unauthorized, result.Error);
		_sessionService.Verify(s => s.EndAsync(SessionEndReason.Expired), Times.Once);
	}

	[Fact]
	public async Task DeleteAsync_AttachesBearerTokenAndSucceedsWithNoResponseBody()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.NoContent);

		var result = await CreateClient(handler).DeleteAsync("api/properties/" + Guid.NewGuid());

		Assert.True(result.IsSuccess);
		Assert.Equal("Bearer test-token", handler.Requests.Single().Authorization);
	}

	[Fact]
	public async Task DeleteAsync_WhenUnauthorized_EndsTheSession()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.Unauthorized);

		var result = await CreateClient(handler).DeleteAsync("api/properties/" + Guid.NewGuid());

		Assert.Equal(ApiError.Unauthorized, result.Error);
		_sessionService.Verify(s => s.EndAsync(SessionEndReason.Expired), Times.Once);
	}

	[Fact]
	public async Task SendMultipartAsync_SendsFieldsAndFilesWithTheBearerToken()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, "{}");
		var fields = new Dictionary<string, string> { ["Title"] = "Test House", ["PriceLkr"] = "25000000" };
		var photos = new[] { new PendingPhoto(new MemoryStream("photo-bytes"u8.ToArray()), "photo.jpg", "image/jpeg") };

		await CreateClient(handler).SendMultipartAsync<UserDto>(HttpMethod.Post, "api/properties", fields, photos);

		var request = handler.Requests.Single();
		Assert.Equal("Bearer test-token", request.Authorization);
		Assert.Contains("name=Title", request.Body);
		Assert.Contains("Test House", request.Body);
		Assert.Contains("name=PriceLkr", request.Body);
		Assert.Contains("filename=photo.jpg", request.Body);
		Assert.Contains("photo-bytes", request.Body);
	}

	[Fact]
	public async Task SendMultipartAsync_WhenUnauthorized_EndsTheSession()
	{
		var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.Unauthorized);

		var result = await CreateClient(handler).SendMultipartAsync<UserDto>(
			HttpMethod.Post, "api/properties", new Dictionary<string, string>(), []);

		Assert.Equal(ApiError.Unauthorized, result.Error);
		_sessionService.Verify(s => s.EndAsync(SessionEndReason.Expired), Times.Once);
	}

	private ApiClient CreateClient(FakeHttpMessageHandler handler) =>
		new(new HttpClient(handler), _apiSettings.Object, _sessionService.Object);
}
