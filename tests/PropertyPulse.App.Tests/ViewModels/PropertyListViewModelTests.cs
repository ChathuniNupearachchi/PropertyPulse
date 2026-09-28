using Moq;
using PropertyPulse.App.Services;
using PropertyPulse.App.ViewModels;
using PropertyPulse.Shared;
using PropertyPulse.Shared.Properties;

namespace PropertyPulse.App.Tests;

public class PropertyListViewModelTests
{
	private readonly Mock<IPropertyService> _propertyService = new();
	private readonly Mock<INavigationService> _navigationService = new();
	private readonly PropertyListViewModel _viewModel;

	public PropertyListViewModelTests()
	{
		_viewModel = new PropertyListViewModel(_propertyService.Object, _navigationService.Object);
	}

	[Fact]
	public async Task Appearing_LoadsTheFirstPage()
	{
		SetupPage(1, [CreateProperty("Colombo House")], totalCount: 1);

		await _viewModel.AppearingCommand.ExecuteAsync(null);

		Assert.Single(_viewModel.Properties);
		Assert.Equal("Colombo House", _viewModel.Properties[0].Title);
	}

	[Fact]
	public async Task Appearing_CalledTwice_OnlyLoadsOnce()
	{
		SetupPage(1, [CreateProperty("Colombo House")], totalCount: 1);

		await _viewModel.AppearingCommand.ExecuteAsync(null);
		await _viewModel.AppearingCommand.ExecuteAsync(null);

		_propertyService.Verify(
			s => s.ListAsync(It.IsAny<PropertyFilter>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Search_ResetsToPageOneAndFiltersBySearchText()
	{
		SetupPage(1, [CreateProperty("Galle Villa")], totalCount: 1);
		_viewModel.SearchText = "Galle";

		await _viewModel.SearchCommand.ExecuteAsync(null);

		_propertyService.Verify(
			s => s.ListAsync(It.Is<PropertyFilter>(f => f.Search == "Galle"), 1, It.IsAny<int>(), It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task ApplyFilters_SendsTheSelectedFilterValues()
	{
		SetupPage(1, [], totalCount: 0);
		_viewModel.SelectedType = "House";
		_viewModel.SelectedStatus = "Available";
		_viewModel.City = "Kandy";
		_viewModel.MinPriceText = "1000000";
		_viewModel.MaxPriceText = "5000000";

		await _viewModel.ApplyFiltersCommand.ExecuteAsync(null);

		_propertyService.Verify(
			s => s.ListAsync(
				It.Is<PropertyFilter>(f =>
					f.PropertyType == "House" && f.Status == "Available" && f.City == "Kandy" &&
					f.MinPrice == 1_000_000m && f.MaxPrice == 5_000_000m),
				1, It.IsAny<int>(), It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task ClearFilters_ResetsFiltersAndSearchThenReloads()
	{
		SetupPage(1, [], totalCount: 0);
		_viewModel.SearchText = "Galle";
		_viewModel.SelectedType = "House";
		_viewModel.City = "Kandy";

		await _viewModel.ClearFiltersCommand.ExecuteAsync(null);

		Assert.Empty(_viewModel.SearchText);
		Assert.Equal("Any", _viewModel.SelectedType);
		Assert.Empty(_viewModel.City);
		_propertyService.Verify(
			s => s.ListAsync(It.Is<PropertyFilter>(f => f.PropertyType == null && f.Search == null), 1, It.IsAny<int>(), It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task LoadMore_AppendsTheNextPage()
	{
		SetupPage(1, [CreateProperty("First")], totalCount: 25);
		await _viewModel.AppearingCommand.ExecuteAsync(null);
		SetupPage(2, [CreateProperty("Second")], totalCount: 25);

		await _viewModel.LoadMoreCommand.ExecuteAsync(null);

		Assert.Equal(2, _viewModel.Properties.Count);
		Assert.Equal("Second", _viewModel.Properties[1].Title);
	}

	[Fact]
	public async Task LoadMore_OnTheLastPage_MakesNoFurtherRequest()
	{
		SetupPage(1, [CreateProperty("Only one")], totalCount: 1);
		await _viewModel.AppearingCommand.ExecuteAsync(null);

		await _viewModel.LoadMoreCommand.ExecuteAsync(null);

		_propertyService.Verify(
			s => s.ListAsync(It.IsAny<PropertyFilter>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Refresh_ReplacesTheListWithTheFirstPage()
	{
		SetupPage(1, [CreateProperty("Stale")], totalCount: 1);
		await _viewModel.AppearingCommand.ExecuteAsync(null);
		SetupPage(1, [CreateProperty("Fresh")], totalCount: 1);

		await _viewModel.RefreshCommand.ExecuteAsync(null);

		Assert.Single(_viewModel.Properties);
		Assert.Equal("Fresh", _viewModel.Properties[0].Title);
		Assert.False(_viewModel.IsRefreshing);
	}

	[Fact]
	public async Task Appearing_WhenTheFirstPageFails_ShowsAnErrorAndNoCards()
	{
		_propertyService
			.Setup(s => s.ListAsync(It.IsAny<PropertyFilter>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(ApiResult<PagedResult<PropertyDto>>.Failure(ApiError.Network));

		await _viewModel.AppearingCommand.ExecuteAsync(null);

		Assert.Empty(_viewModel.Properties);
		Assert.NotEmpty(_viewModel.ErrorMessage);
	}

	[Fact]
	public async Task LoadMore_WhenTheNextPageFails_KeepsAlreadyLoadedProperties()
	{
		SetupPage(1, [CreateProperty("Already loaded")], totalCount: 25);
		await _viewModel.AppearingCommand.ExecuteAsync(null);
		_propertyService
			.Setup(s => s.ListAsync(It.IsAny<PropertyFilter>(), 2, It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(ApiResult<PagedResult<PropertyDto>>.Failure(ApiError.Server));

		await _viewModel.LoadMoreCommand.ExecuteAsync(null);

		Assert.Single(_viewModel.Properties);
		Assert.NotEmpty(_viewModel.ErrorMessage);
	}

	[Fact]
	public async Task OpenProperty_NavigatesToTheDetailRouteWithTheId()
	{
		var property = CreateProperty("Some House");

		await _viewModel.OpenPropertyCommand.ExecuteAsync(property);

		_navigationService.Verify(n => n.GoToAsync($"{Routes.PropertyDetail}?id={property.Id}"), Times.Once);
	}

	[Fact]
	public async Task AddProperty_NavigatesToTheFormRoute()
	{
		await _viewModel.AddPropertyCommand.ExecuteAsync(null);

		_navigationService.Verify(n => n.GoToAsync(Routes.PropertyForm), Times.Once);
	}

	private void SetupPage(int page, PropertyDto[] items, int totalCount) =>
		_propertyService
			.Setup(s => s.ListAsync(It.IsAny<PropertyFilter>(), page, It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(ApiResult<PagedResult<PropertyDto>>.Success(new PagedResult<PropertyDto>(items, page, 20, totalCount)));

	private static PropertyDto CreateProperty(string title) => new(
		Guid.NewGuid(), title, "Description", "House", "Available", 1_000_000m, "Address", "Colombo",
		null, null, 3, 2, null, null, Guid.NewGuid(), "Agent Name", []);
}
