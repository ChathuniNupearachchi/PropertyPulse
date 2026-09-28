using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PropertyPulse.App.Services;

namespace PropertyPulse.App.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
	private readonly ISessionService _sessionService;

	[ObservableProperty]
	public partial string FullName { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string Email { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string Role { get; set; } = string.Empty;

	public SettingsViewModel(ISessionService sessionService)
	{
		_sessionService = sessionService;
		Title = "Settings";
	}

	[RelayCommand]
	private void Appearing()
	{
		var user = _sessionService.Current?.User;
		FullName = user?.FullName ?? string.Empty;
		Email = user?.Email ?? string.Empty;
		Role = user?.Role ?? string.Empty;
	}

	[RelayCommand]
	private Task SignOutAsync() => _sessionService.EndAsync(SessionEndReason.SignedOut);
}
