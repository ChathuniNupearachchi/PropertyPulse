using PropertyPulse.App.Services;
using PropertyPulse.App.ViewModels;
using PropertyPulse.App.Views;

namespace PropertyPulse.App;

public partial class AppShell : Shell
{
	public AppShell(ShellViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;

		// Pushed onto a tab's navigation stack rather than shown as a tab, so they are registered rather than declared in XAML.
		Routing.RegisterRoute(Routes.PropertyDetail, typeof(PropertyDetailPage));
		Routing.RegisterRoute(Routes.PropertyForm, typeof(PropertyFormPage));
	}
}
