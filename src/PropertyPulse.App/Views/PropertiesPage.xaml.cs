using PropertyPulse.App.ViewModels;

namespace PropertyPulse.App.Views;

public partial class PropertiesPage : ContentPage
{
	public PropertiesPage(PropertiesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
