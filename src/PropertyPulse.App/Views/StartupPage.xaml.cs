using PropertyPulse.App.ViewModels;

namespace PropertyPulse.App.Views;

public partial class StartupPage : ContentPage
{
	public StartupPage(StartupViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
