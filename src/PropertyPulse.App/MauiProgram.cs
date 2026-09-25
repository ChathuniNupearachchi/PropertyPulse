using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
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

		builder.Services.AddTransient<PropertiesViewModel>();
		builder.Services.AddTransient<LeadsViewModel>();
		builder.Services.AddTransient<VisitsViewModel>();
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<SettingsViewModel>();

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
