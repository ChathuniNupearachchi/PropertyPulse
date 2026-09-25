using PropertyPulse.App.ViewModels;

namespace PropertyPulse.App.Views;

public partial class LeadsPage : ContentPage
{
	public LeadsPage(LeadsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
