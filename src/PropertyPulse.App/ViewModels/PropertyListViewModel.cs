using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PropertyPulse.App.Services;
using PropertyPulse.Shared.Properties;

namespace PropertyPulse.App.ViewModels;

public partial class PropertyListViewModel : BaseViewModel
{
	private const string AnyOption = "Any";
	private const int PageSize = 20;

	private readonly IPropertyService _propertyService;
	private readonly INavigationService _navigationService;

	private int _page = 1;
	private bool _isLastPage;
	private bool _hasLoadedOnce;

	public ObservableCollection<PropertyDto> Properties { get; } = [];

	public IReadOnlyList<string> PropertyTypeOptions { get; } = [AnyOption, "House", "Apartment", "Land", "Commercial"];
	public IReadOnlyList<string> StatusOptions { get; } = [AnyOption, "Available", "Reserved", "Sold", "Rented"];

	[ObservableProperty]
	public partial string SearchText { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string SelectedType { get; set; } = AnyOption;

	[ObservableProperty]
	public partial string SelectedStatus { get; set; } = AnyOption;

	[ObservableProperty]
	public partial string City { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string MinPriceText { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string MaxPriceText { get; set; } = string.Empty;

	[ObservableProperty]
	public partial bool IsRefreshing { get; set; }

	[ObservableProperty]
	public partial bool IsLoadingMore { get; set; }

	[ObservableProperty]
	public partial string ErrorMessage { get; set; } = string.Empty;

	[ObservableProperty]
	public partial PropertyDto? SelectedProperty { get; set; }

	public PropertyListViewModel(IPropertyService propertyService, INavigationService navigationService)
	{
		_propertyService = propertyService;
		_navigationService = navigationService;
		Title = "Properties";
	}

	[RelayCommand]
	private Task AppearingAsync()
	{
		if (_hasLoadedOnce)
		{
			return Task.CompletedTask;
		}

		_hasLoadedOnce = true;
		return LoadAsync(reset: true);
	}

	[RelayCommand]
	private Task SearchAsync() => LoadAsync(reset: true);

	[RelayCommand]
	private Task ApplyFiltersAsync() => LoadAsync(reset: true);

	[RelayCommand]
	private Task ClearFiltersAsync()
	{
		SearchText = string.Empty;
		SelectedType = AnyOption;
		SelectedStatus = AnyOption;
		City = string.Empty;
		MinPriceText = string.Empty;
		MaxPriceText = string.Empty;
		return LoadAsync(reset: true);
	}

	[RelayCommand]
	private async Task RefreshAsync()
	{
		IsRefreshing = true;
		await LoadAsync(reset: true, showBusy: false);
		IsRefreshing = false;
	}

	[RelayCommand]
	private Task LoadMoreAsync() =>
		IsBusy || IsLoadingMore || IsRefreshing || _isLastPage ? Task.CompletedTask : LoadAsync(reset: false);

	[RelayCommand]
	private Task RetryAsync() => LoadAsync(reset: Properties.Count == 0);

	[RelayCommand]
	private Task OpenPropertyAsync(PropertyDto? property) =>
		property is null ? Task.CompletedTask : _navigationService.GoToAsync($"{Routes.PropertyDetail}?id={property.Id}");

	[RelayCommand]
	private Task AddPropertyAsync() => _navigationService.GoToAsync(Routes.PropertyForm);

	partial void OnSelectedPropertyChanged(PropertyDto? value)
	{
		if (value is null)
		{
			return;
		}

		SelectedProperty = null;
		OpenPropertyCommand.Execute(value);
	}

	private async Task LoadAsync(bool reset, bool showBusy = true)
	{
		if (reset)
		{
			_page = 1;
			_isLastPage = false;
			ErrorMessage = string.Empty;
			Properties.Clear();
			IsBusy = showBusy;
		}
		else
		{
			IsLoadingMore = true;
		}

		try
		{
			var filter = BuildFilter();
			var result = await _propertyService.ListAsync(filter, _page, PageSize);

			if (!result.IsSuccess)
			{
				ErrorMessage = DescribeError(result.Error);
				return;
			}

			foreach (var property in result.Value!.Items)
			{
				Properties.Add(property);
			}

			_isLastPage = _page * PageSize >= result.Value.TotalCount;
			_page++;
		}
		finally
		{
			IsBusy = false;
			IsLoadingMore = false;
		}
	}

	private PropertyFilter BuildFilter() => new(
		PropertyType: SelectedType == AnyOption ? null : SelectedType,
		Status: SelectedStatus == AnyOption ? null : SelectedStatus,
		City: string.IsNullOrWhiteSpace(City) ? null : City,
		MinPrice: decimal.TryParse(MinPriceText, out var min) ? min : null,
		MaxPrice: decimal.TryParse(MaxPriceText, out var max) ? max : null,
		Search: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText);

	private static string DescribeError(ApiError error) => error switch
	{
		ApiError.Network => "Could not reach the server. Check your connection and try again.",
		ApiError.Unauthorized => "Your session has expired. Please sign in again.",
		_ => "Something went wrong loading properties. Please try again."
	};
}
