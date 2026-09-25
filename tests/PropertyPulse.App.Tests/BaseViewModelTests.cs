using PropertyPulse.App.ViewModels;

namespace PropertyPulse.App.Tests;

public class BaseViewModelTests
{
	private sealed class TestViewModel : BaseViewModel;

	[Fact]
	public void IsBusy_DefaultsToFalse()
	{
		var viewModel = new TestViewModel();

		Assert.False(viewModel.IsBusy);
	}

	[Fact]
	public void IsBusy_RaisesPropertyChanged()
	{
		var viewModel = new TestViewModel();
		var raised = new List<string?>();
		viewModel.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

		viewModel.IsBusy = true;

		Assert.Contains(nameof(BaseViewModel.IsBusy), raised);
	}

	[Fact]
	public void Title_RaisesPropertyChanged()
	{
		var viewModel = new TestViewModel();
		var raised = new List<string?>();
		viewModel.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

		viewModel.Title = "Properties";

		Assert.Contains(nameof(BaseViewModel.Title), raised);
	}

	[Theory]
	[InlineData(typeof(PropertiesViewModel), "Properties")]
	[InlineData(typeof(LeadsViewModel), "Leads")]
	[InlineData(typeof(VisitsViewModel), "Visits")]
	[InlineData(typeof(DashboardViewModel), "Dashboard")]
	[InlineData(typeof(SettingsViewModel), "Settings")]
	public void TabViewModel_SetsTitle(Type viewModelType, string expectedTitle)
	{
		var viewModel = (BaseViewModel)Activator.CreateInstance(viewModelType)!;

		Assert.Equal(expectedTitle, viewModel.Title);
	}
}
