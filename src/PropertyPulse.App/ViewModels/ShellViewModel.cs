using CommunityToolkit.Mvvm.ComponentModel;
using PropertyPulse.App.Services;

namespace PropertyPulse.App.ViewModels;

public partial class ShellViewModel : BaseViewModel
{
	private readonly ISessionService _sessionService;
	private readonly INavigationService _navigationService;

	[ObservableProperty]
	public partial bool IsManager { get; set; }

	public ShellViewModel(ISessionService sessionService, INavigationService navigationService)
	{
		_sessionService = sessionService;
		_navigationService = navigationService;
		_sessionService.SessionChanged += OnSessionChanged;
	}

	// Every way a session can end (sign out, a 401, expiry) goes through here, so all of them land on the Login screen.
	private async void OnSessionChanged(object? sender, EventArgs e)
	{
		var session = _sessionService.Current;
		IsManager = session?.User.Role == UserRoles.Manager;

		if (session is null)
		{
			await _navigationService.GoToAsync(Routes.Login);
		}
	}
}
