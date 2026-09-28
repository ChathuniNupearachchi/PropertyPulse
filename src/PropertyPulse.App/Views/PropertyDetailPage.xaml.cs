using PropertyPulse.App.ViewModels;

namespace PropertyPulse.App.Views;

public partial class PropertyDetailPage : ContentPage, IQueryAttributable
{
	private readonly PropertyDetailViewModel _viewModel;

	public PropertyDetailPage(PropertyDetailViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue("id", out var id))
		{
			_viewModel.PropertyId = id as string;
		}
	}
}
