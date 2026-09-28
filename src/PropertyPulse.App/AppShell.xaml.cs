using PropertyPulse.App.ViewModels;

namespace PropertyPulse.App;

public partial class AppShell : Shell
{
	public AppShell(ShellViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
