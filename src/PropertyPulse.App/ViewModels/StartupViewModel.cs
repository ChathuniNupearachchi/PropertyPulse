using CommunityToolkit.Mvvm.Input;
using PropertyPulse.App.Services;

namespace PropertyPulse.App.ViewModels;

public partial class StartupViewModel : BaseViewModel
{
	private readonly ISessionService _sessionService;
	private readonly INavigationService _navigationService;

	public StartupViewModel(ISessionService sessionService, INavigationService navigationService)
	{
		_sessionService = sessionService;
		_navigationService = navigationService;
		Title = "PropertyPulse";
	}

	[RelayCommand]
	private async Task AppearingAsync()
	{
		var restored = await _sessionService.RestoreAsync();
		await _navigationService.GoToAsync(restored ? Routes.Properties : Routes.Login);
	}
}
