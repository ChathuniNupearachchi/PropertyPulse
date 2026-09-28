using System.Net.Mail;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PropertyPulse.App.Services;

namespace PropertyPulse.App.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
	private readonly IAuthService _authService;
	private readonly IApiSettings _apiSettings;
	private readonly ISessionService _sessionService;
	private readonly INavigationService _navigationService;

	[ObservableProperty]
	public partial string Email { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string Password { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string ServerUrl { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string EmailError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string PasswordError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string ServerUrlError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string ErrorMessage { get; set; } = string.Empty;

	public LoginViewModel(
		IAuthService authService,
		IApiSettings apiSettings,
		ISessionService sessionService,
		INavigationService navigationService)
	{
		_authService = authService;
		_apiSettings = apiSettings;
		_sessionService = sessionService;
		_navigationService = navigationService;
		Title = "Sign in";
	}

	[RelayCommand]
	private void Appearing()
	{
		Password = string.Empty;
		ClearErrors();
		ServerUrl = _apiSettings.BaseUrl;

		if (_sessionService.ConsumeEndReason() == SessionEndReason.Expired)
		{
			ErrorMessage = "Your session has expired. Please sign in again.";
		}
	}

	[RelayCommand]
	private async Task SignInAsync()
	{
		if (!Validate(out var serverUrl))
		{
			return;
		}

		IsBusy = true;
		try
		{
			_apiSettings.BaseUrl = serverUrl;

			var result = await _authService.LoginAsync(Email.Trim(), Password);
			if (result.IsSuccess)
			{
				await _navigationService.GoToAsync(Routes.Properties);
				return;
			}

			ErrorMessage = result.Error switch
			{
				ApiError.Unauthorized => "Invalid email or password.",
				ApiError.Network => $"Could not reach the server at {serverUrl}. Check the server address and your network connection.",
				_ => "Something went wrong. Please try again."
			};
		}
		finally
		{
			IsBusy = false;
		}
	}

	private bool Validate(out string serverUrl)
	{
		ClearErrors();

		EmailError = ValidateEmail(Email);
		PasswordError = string.IsNullOrEmpty(Password) ? "Enter your password." : string.Empty;
		ServerUrlError = TryNormalizeServerUrl(ServerUrl, out serverUrl)
			? string.Empty
			: "Enter a full address such as http://192.168.1.20:5000.";

		return EmailError.Length == 0 && PasswordError.Length == 0 && ServerUrlError.Length == 0;
	}

	private void ClearErrors()
	{
		EmailError = string.Empty;
		PasswordError = string.Empty;
		ServerUrlError = string.Empty;
		ErrorMessage = string.Empty;
	}

	private static string ValidateEmail(string email)
	{
		var trimmed = email.Trim();

		if (trimmed.Length == 0)
		{
			return "Enter your email.";
		}

		// MailAddress also accepts "Name <a@b.com>", so the parsed address must equal the input.
		return MailAddress.TryCreate(trimmed, out var address) && address.Address == trimmed
			? string.Empty
			: "Enter a valid email address.";
	}

	private static bool TryNormalizeServerUrl(string value, out string normalized)
	{
		normalized = value.Trim().TrimEnd('/');

		return Uri.TryCreate(normalized, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
	}
}
