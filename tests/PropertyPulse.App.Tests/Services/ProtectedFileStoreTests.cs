using System.Security.Cryptography;
using System.Text;
using PropertyPulse.App.Services;
using PropertyPulse.Shared.Auth;

namespace PropertyPulse.App.Tests;

public sealed class ProtectedFileStoreTests : IDisposable
{
	private readonly string _directory = Path.Combine(Path.GetTempPath(), "pp-store-tests-" + Guid.NewGuid().ToString("N"));
	private readonly FakeValueProtector _protector = new();
	private readonly ProtectedFileStore _store;

	public ProtectedFileStoreTests()
	{
		_store = new ProtectedFileStore(_directory, _protector);
	}

	public void Dispose()
	{
		if (Directory.Exists(_directory))
		{
			Directory.Delete(_directory, recursive: true);
		}
	}

	[Fact]
	public async Task SetAsync_ThenGetAsync_ReturnsTheValue()
	{
		await _store.SetAsync("session", "hello ✓ wörld");

		Assert.Equal("hello ✓ wörld", await _store.GetAsync("session"));
	}

	[Fact]
	public async Task SetAsync_OverwritesAnExistingValue()
	{
		await _store.SetAsync("session", "first");
		await _store.SetAsync("session", "second");

		Assert.Equal("second", await _store.GetAsync("session"));
		Assert.Single(Directory.GetFiles(_directory));
	}

	[Fact]
	public async Task GetAsync_WithAnUnknownKey_ReturnsNull()
	{
		Assert.Null(await _store.GetAsync("missing"));
	}

	[Fact]
	public async Task GetAsync_BeforeAnythingWasStored_ReturnsNullWithoutCreatingTheDirectory()
	{
		Assert.Null(await _store.GetAsync("session"));
		Assert.False(Directory.Exists(_directory));
	}

	[Fact]
	public async Task Remove_DeletesTheValue()
	{
		await _store.SetAsync("session", "value");

		_store.Remove("session");

		Assert.Null(await _store.GetAsync("session"));
	}

	[Fact]
	public void Remove_WithAnUnknownKey_DoesNothing()
	{
		_store.Remove("missing");
	}

	[Fact]
	public async Task SetAsync_DoesNotWriteThePlainTextToDisk()
	{
		await _store.SetAsync("session", "secret-token-value");

		var content = Encoding.UTF8.GetString(await File.ReadAllBytesAsync(Directory.GetFiles(_directory).Single()));

		Assert.DoesNotContain("secret-token-value", content);
	}

	[Theory]
	[InlineData("a/b")]
	[InlineData("..\\..\\escape")]
	[InlineData("with space:and*chars")]
	public async Task Keys_WithUnusualCharacters_StayInsideTheStoreDirectory(string key)
	{
		await _store.SetAsync(key, "value");

		Assert.Equal("value", await _store.GetAsync(key));
		Assert.All(Directory.GetFiles(_directory), file => Assert.Equal(_directory, Path.GetDirectoryName(file)));
	}

	[Fact]
	public async Task GetAsync_WhenTheProtectorRejectsTheFile_Throws()
	{
		await _store.SetAsync("session", "value");
		_protector.RejectUnprotect = true;

		await Assert.ThrowsAsync<CryptographicException>(() => _store.GetAsync("session"));
	}

	[Fact]
	public async Task SessionService_WhenTheStoredFileCannotBeUnprotected_RemovesItAndAsksForSignIn()
	{
		var sessionService = new SessionService(_store, TimeProvider.System);
		await sessionService.StartAsync(new AuthResponse("token", DateTime.UtcNow.AddHours(1), new UserDto(Guid.NewGuid(), "Test", "t@e.com", "Agent")));
		var restoredService = new SessionService(_store, TimeProvider.System);
		_protector.RejectUnprotect = true;

		var restored = await restoredService.RestoreAsync();

		Assert.False(restored);
		Assert.Empty(Directory.GetFiles(_directory));
	}

	[Fact]
	public async Task SessionService_RestoresASessionAfterARestart()
	{
		var user = new UserDto(Guid.NewGuid(), "Nimali Perera", "manager@propertypulse.lk", "Manager");
		await new SessionService(_store, TimeProvider.System).StartAsync(new AuthResponse("token", DateTime.UtcNow.AddHours(1), user));

		var afterRestart = new SessionService(_store, TimeProvider.System);
		var restored = await afterRestart.RestoreAsync();

		Assert.True(restored);
		Assert.Equal(user, afterRestart.Current?.User);
	}
}
