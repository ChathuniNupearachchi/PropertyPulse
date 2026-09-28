using System.Text.Json;
using PropertyPulse.Shared.Auth;

namespace PropertyPulse.App.Services;

public class SessionService(ISecureStore secureStore, TimeProvider timeProvider) : ISessionService
{
	private const string StorageKey = "session";

	private AuthResponse? _current;
	private SessionEndReason _endReason;

	public AuthResponse? Current => _current;

	public event EventHandler? SessionChanged;

	public async Task StartAsync(AuthResponse session)
	{
		_current = session;
		_endReason = SessionEndReason.None;

		try
		{
			await secureStore.SetAsync(StorageKey, JsonSerializer.Serialize(session));
		}
		catch (Exception)
		{
			// Secure storage can be unavailable on some setups. The user stays signed in for this run and signs in again next launch.
		}

		SessionChanged?.Invoke(this, EventArgs.Empty);
	}

	public async Task<bool> RestoreAsync()
	{
		AuthResponse? stored;

		try
		{
			var json = await secureStore.GetAsync(StorageKey);
			if (json is null)
			{
				return false;
			}

			stored = JsonSerializer.Deserialize<AuthResponse>(json);
		}
		catch (Exception)
		{
			TryRemoveStoredSession();
			return false;
		}

		if (stored?.User is null || string.IsNullOrEmpty(stored.Token))
		{
			TryRemoveStoredSession();
			return false;
		}

		if (stored.ExpiresAt.ToUniversalTime() <= timeProvider.GetUtcNow().UtcDateTime)
		{
			_endReason = SessionEndReason.Expired;
			TryRemoveStoredSession();
			return false;
		}

		_current = stored;
		SessionChanged?.Invoke(this, EventArgs.Empty);
		return true;
	}

	public Task EndAsync(SessionEndReason reason)
	{
		// Several requests can fail with 401 at once; only the first one ends the session.
		if (Interlocked.Exchange(ref _current, null) is null)
		{
			return Task.CompletedTask;
		}

		_endReason = reason;
		TryRemoveStoredSession();
		SessionChanged?.Invoke(this, EventArgs.Empty);
		return Task.CompletedTask;
	}

	public SessionEndReason ConsumeEndReason()
	{
		var reason = _endReason;
		_endReason = SessionEndReason.None;
		return reason;
	}

	private void TryRemoveStoredSession()
	{
		try
		{
			secureStore.Remove(StorageKey);
		}
		catch (Exception)
		{
			// Nothing to clean up if the store cannot be reached.
		}
	}
}
