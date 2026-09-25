using PropertyPulse.App.ViewModels;

namespace PropertyPulse.App.Views;

public partial class VisitsPage : ContentPage
{
	public VisitsPage(VisitsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
