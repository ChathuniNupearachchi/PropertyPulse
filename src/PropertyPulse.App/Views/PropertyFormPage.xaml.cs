using PropertyPulse.App.ViewModels;

namespace PropertyPulse.App.Views;

public partial class PropertyFormPage : ContentPage, IQueryAttributable
{
	private readonly PropertyEditViewModel _viewModel;

	public PropertyFormPage(PropertyEditViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		_viewModel.PropertyId = query.TryGetValue("id", out var id) ? id as string : null;
	}
}
