using PropertyPulse.App.ViewModels;

namespace PropertyPulse.App.Views;

public partial class PropertiesPage : ContentPage
{
	public PropertiesPage(PropertyListViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
