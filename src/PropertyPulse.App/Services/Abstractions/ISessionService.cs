using PropertyPulse.Shared.Auth;

namespace PropertyPulse.App.Services;

public interface ISessionService
{
	AuthResponse? Current { get; }

	event EventHandler? SessionChanged;

	Task StartAsync(AuthResponse session);

	Task<bool> RestoreAsync();

	Task EndAsync(SessionEndReason reason);

	SessionEndReason ConsumeEndReason();
}
