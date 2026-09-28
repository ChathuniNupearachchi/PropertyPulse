using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PropertyPulse.App.Services;
using PropertyPulse.App.ViewModels;
using PropertyPulse.App.Views;

namespace PropertyPulse.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton(new HttpClient { Timeout = TimeSpan.FromSeconds(15) });
		builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
#if WINDOWS
		builder.Services.AddSingleton<IValueProtector, DpapiValueProtector>();
		builder.Services.AddSingleton<ISecureStore>(services => new ProtectedFileStore(
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PropertyPulse", "secure"),
			services.GetRequiredService<IValueProtector>()));
#else
		builder.Services.AddSingleton<ISecureStore, SecureStoreAdapter>();
#endif
		builder.Services.AddSingleton<IApiSettings, ApiSettings>();
		builder.Services.AddSingleton<ISessionService, SessionService>();
		builder.Services.AddSingleton<IApiClient, ApiClient>();
		builder.Services.AddSingleton<IAuthService, AuthService>();
		builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

		builder.Services.AddSingleton<ShellViewModel>();
		builder.Services.AddSingleton<AppShell>();

		builder.Services.AddTransient<StartupViewModel>();
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<PropertiesViewModel>();
		builder.Services.AddTransient<LeadsViewModel>();
		builder.Services.AddTransient<VisitsViewModel>();
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<SettingsViewModel>();

		builder.Services.AddTransient<StartupPage>();
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<PropertiesPage>();
		builder.Services.AddTransient<LeadsPage>();
		builder.Services.AddTransient<VisitsPage>();
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<SettingsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
