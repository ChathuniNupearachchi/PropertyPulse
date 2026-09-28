using System.Text.Json;
using PropertyPulse.App.Services;
using PropertyPulse.Shared.Auth;

namespace PropertyPulse.App.Tests;

public class SessionServiceTests
{
	private static readonly DateTimeOffset Now = new(2026, 9, 26, 10, 0, 0, TimeSpan.Zero);

	private readonly FakeSecureStore _store = new();
	private readonly SessionService _sessionService;

	public SessionServiceTests()
	{
		_sessionService = new SessionService(_store, new FakeTimeProvider(Now));
	}

	[Fact]
	public async Task StartAsync_SetsCurrentAndStoresSession()
	{
		var session = CreateSession(Now.AddMinutes(60));

		await _sessionService.StartAsync(session);

		Assert.Equal(session, _sessionService.Current);
		Assert.True(_store.Values.ContainsKey("session"));
	}

	[Fact]
	public async Task StartAsync_WhenStorageFails_StillSignsIn()
	{
		_store.ThrowOnSet = true;
		var session = CreateSession(Now.AddMinutes(60));

		await _sessionService.StartAsync(session);

		Assert.Equal(session, _sessionService.Current);
	}

	[Fact]
	public async Task RestoreAsync_WithValidSession_RestoresIt()
	{
		var session = CreateSession(Now.AddMinutes(30));
		_store.Values["session"] = JsonSerializer.Serialize(session);

		var restored = await _sessionService.RestoreAsync();

		Assert.True(restored);
		Assert.Equal(session.Token, _sessionService.Current?.Token);
		Assert.Equal("Manager", _sessionService.Current?.User.Role);
	}

	[Fact]
	public async Task RestoreAsync_WithExpiredSession_RemovesItAndRemembersExpiry()
	{
		_store.Values["session"] = JsonSerializer.Serialize(CreateSession(Now.AddMinutes(-1)));

		var restored = await _sessionService.RestoreAsync();

		Assert.False(restored);
		Assert.Null(_sessionService.Current);
		Assert.Empty(_store.Values);
		Assert.Equal(SessionEndReason.Expired, _sessionService.ConsumeEndReason());
	}

	[Theory]
	[InlineData("not json")]
	[InlineData("{}")]
	public async Task RestoreAsync_WithCorruptValue_RemovesItAndReturnsFalse(string storedValue)
	{
		_store.Values["session"] = storedValue;

		var restored = await _sessionService.RestoreAsync();

		Assert.False(restored);
		Assert.Null(_sessionService.Current);
		Assert.Empty(_store.Values);
		Assert.Equal(SessionEndReason.None, _sessionService.ConsumeEndReason());
	}

	[Fact]
	public async Task RestoreAsync_WithNothingStored_ReturnsFalse()
	{
		var restored = await _sessionService.RestoreAsync();

		Assert.False(restored);
		Assert.Null(_sessionService.Current);
	}

	[Fact]
	public async Task EndAsync_ClearsSessionAndStoredValue()
	{
		await _sessionService.StartAsync(CreateSession(Now.AddMinutes(60)));

		await _sessionService.EndAsync(SessionEndReason.SignedOut);

		Assert.Null(_sessionService.Current);
		Assert.Empty(_store.Values);
	}

	[Fact]
	public async Task EndAsync_CalledTwice_RaisesSessionChangedOnce()
	{
		await _sessionService.StartAsync(CreateSession(Now.AddMinutes(60)));
		var endedCount = 0;
		_sessionService.SessionChanged += (_, _) => endedCount++;

		await _sessionService.EndAsync(SessionEndReason.Expired);
		await _sessionService.EndAsync(SessionEndReason.Expired);

		Assert.Equal(1, endedCount);
	}

	[Fact]
	public async Task SessionChanged_IsRaisedOnStartAndOnRestoreSuccess()
	{
		var changedCount = 0;
		_sessionService.SessionChanged += (_, _) => changedCount++;

		await _sessionService.StartAsync(CreateSession(Now.AddMinutes(60)));
		_store.Values["session"] = JsonSerializer.Serialize(CreateSession(Now.AddMinutes(30)));
		await _sessionService.RestoreAsync();

		Assert.Equal(2, changedCount);
	}

	[Fact]
	public async Task SessionChanged_IsNotRaisedWhenRestoreFindsNothing()
	{
		var changedCount = 0;
		_sessionService.SessionChanged += (_, _) => changedCount++;

		await _sessionService.RestoreAsync();

		Assert.Equal(0, changedCount);
	}

	[Fact]
	public async Task ConsumeEndReason_ReturnsTheReasonOnlyOnce()
	{
		await _sessionService.StartAsync(CreateSession(Now.AddMinutes(60)));
		await _sessionService.EndAsync(SessionEndReason.Expired);

		Assert.Equal(SessionEndReason.Expired, _sessionService.ConsumeEndReason());
		Assert.Equal(SessionEndReason.None, _sessionService.ConsumeEndReason());
	}

	private static AuthResponse CreateSession(DateTimeOffset expiresAt) =>
		new("test-token", expiresAt.UtcDateTime, new UserDto(Guid.NewGuid(), "Nimali Perera", "manager@propertypulse.lk", "Manager"));
}
