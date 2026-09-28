using Moq;
using PropertyPulse.App.Services;
using PropertyPulse.App.ViewModels;
using PropertyPulse.Shared.Properties;

namespace PropertyPulse.App.Tests;

public class PropertyEditViewModelTests
{
	private readonly Mock<IPropertyService> _propertyService = new();
	private readonly Mock<IMediaPickerService> _mediaPickerService = new();
	private readonly Mock<ILocationService> _locationService = new();
	private readonly Mock<INavigationService> _navigationService = new();
	private readonly PropertyEditViewModel _viewModel;

	public PropertyEditViewModelTests()
	{
		_viewModel = new PropertyEditViewModel(
			_propertyService.Object, _mediaPickerService.Object, _locationService.Object, _navigationService.Object);
	}

	[Fact]
	public async Task Appearing_WithNoId_StartsEmptyForAdd()
	{
		await _viewModel.AppearingCommand.ExecuteAsync(null);

		Assert.Equal("Add property", _viewModel.Title);
		Assert.Empty(_viewModel.PropertyTitle);
		Assert.Empty(_viewModel.Photos);
	}

	[Fact]
	public async Task Appearing_WithAnId_PreFillsTheFieldsAndExistingPhotos()
	{
		var property = new PropertyDto(
			Guid.NewGuid(), "Existing House", "A description", "House", "Available", 5_000_000m, "123 Main St", "Kandy",
			7.29, 80.64, 3, 2, 1500, 10, Guid.NewGuid(), "Kasun Fernando",
			[new PropertyImageDto(Guid.NewGuid(), "https://api/img1.jpg", 0)]);
		_propertyService.Setup(s => s.GetAsync(property.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ApiResult<PropertyDto>.Success(property));
		_viewModel.PropertyId = property.Id.ToString();

		await _viewModel.AppearingCommand.ExecuteAsync(null);

		Assert.Equal("Edit property", _viewModel.Title);
		Assert.Equal("Existing House", _viewModel.PropertyTitle);
		Assert.Equal("5000000", _viewModel.PriceText);
		Assert.Single(_viewModel.Photos);
		Assert.False(_viewModel.Photos[0].IsPending);
	}

	[Fact]
	public async Task Save_WithAnEmptyTitle_ShowsAnErrorAndSendsNothing()
	{
		await _viewModel.AppearingCommand.ExecuteAsync(null);
		FillValidForm();
		_viewModel.PropertyTitle = string.Empty;

		await _viewModel.SaveCommand.ExecuteAsync(null);

		Assert.NotEmpty(_viewModel.TitleError);
		VerifyNothingSaved();
	}

	[Fact]
	public async Task Save_WithAZeroPrice_ShowsAnErrorAndSendsNothing()
	{
		await _viewModel.AppearingCommand.ExecuteAsync(null);
		FillValidForm();
		_viewModel.PriceText = "0";

		await _viewModel.SaveCommand.ExecuteAsync(null);

		Assert.NotEmpty(_viewModel.PriceError);
		VerifyNothingSaved();
	}

	[Fact]
	public async Task Save_WithANegativeFloorArea_ShowsAnErrorAndSendsNothing()
	{
		await _viewModel.AppearingCommand.ExecuteAsync(null);
		FillValidForm();
		_viewModel.FloorAreaText = "-10";

		await _viewModel.SaveCommand.ExecuteAsync(null);

		Assert.NotEmpty(_viewModel.FloorAreaError);
		VerifyNothingSaved();
	}

	[Fact]
	public async Task Save_WithAValidForm_CreatesTheProperty()
	{
		var created = CreateDto();
		_propertyService
			.Setup(s => s.CreateAsync(It.IsAny<PropertyFormRequest>(), It.IsAny<IReadOnlyList<PendingPhoto>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(ApiResult<PropertyDto>.Success(created));
		await _viewModel.AppearingCommand.ExecuteAsync(null);
		FillValidForm();

		await _viewModel.SaveCommand.ExecuteAsync(null);

		_propertyService.Verify(
			s => s.CreateAsync(It.IsAny<PropertyFormRequest>(), It.IsAny<IReadOnlyList<PendingPhoto>>(), It.IsAny<CancellationToken>()),
			Times.Once);
		_navigationService.Verify(n => n.GoToAsync($"{Routes.PropertyDetail}?id={created.Id}"), Times.Once);
	}

	[Fact]
	public async Task Save_WhenEditing_UpdatesAndNavigatesBack()
	{
		var property = CreateDto();
		_propertyService.Setup(s => s.GetAsync(property.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ApiResult<PropertyDto>.Success(property));
		_propertyService
			.Setup(s => s.UpdateAsync(
				property.Id, It.IsAny<PropertyFormRequest>(), It.IsAny<IReadOnlyList<PendingPhoto>>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(ApiResult<PropertyDto>.Success(property));
		_viewModel.PropertyId = property.Id.ToString();
		await _viewModel.AppearingCommand.ExecuteAsync(null);

		await _viewModel.SaveCommand.ExecuteAsync(null);

		_navigationService.Verify(n => n.GoToAsync(".."), Times.Once);
	}

	[Fact]
	public async Task Save_WhenTheServerRejectsIt_ShowsAnErrorAndKeepsTheEnteredData()
	{
		_propertyService
			.Setup(s => s.CreateAsync(It.IsAny<PropertyFormRequest>(), It.IsAny<IReadOnlyList<PendingPhoto>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(ApiResult<PropertyDto>.Failure(ApiError.Network));
		await _viewModel.AppearingCommand.ExecuteAsync(null);
		FillValidForm();

		await _viewModel.SaveCommand.ExecuteAsync(null);

		Assert.NotEmpty(_viewModel.ErrorMessage);
		Assert.Equal("Valid Title", _viewModel.PropertyTitle);
		_navigationService.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public async Task AddPhotos_AppendsThePickedPhotosToTheList()
	{
		_mediaPickerService
			.Setup(m => m.PickPhotosAsync())
			.ReturnsAsync([new PendingPhoto(new MemoryStream(), "a.jpg", "image/jpeg"), new PendingPhoto(new MemoryStream(), "b.jpg", "image/jpeg")]);
		await _viewModel.AppearingCommand.ExecuteAsync(null);

		await _viewModel.AddPhotosCommand.ExecuteAsync(null);

		Assert.Equal(2, _viewModel.Photos.Count);
		Assert.All(_viewModel.Photos, p => Assert.True(p.IsPending));
	}

	[Fact]
	public async Task RemovePhoto_WithAPendingPhoto_RemovesItWithoutTrackingAnyRemoval()
	{
		var pending = new PhotoItem(null, null, new PendingPhoto(new MemoryStream(), "a.jpg", "image/jpeg"));
		await _viewModel.AppearingCommand.ExecuteAsync(null);
		_viewModel.Photos.Add(pending);

		_viewModel.RemovePhotoCommand.Execute(pending);

		Assert.Empty(_viewModel.Photos);
	}

	[Fact]
	public async Task RemovePhoto_WithAnExistingPhoto_MarksItForRemovalOnSave()
	{
		var imageId = Guid.NewGuid();
		var property = CreateDto() with { Images = [new PropertyImageDto(imageId, "https://api/img.jpg", 0)] };
		_propertyService.Setup(s => s.GetAsync(property.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ApiResult<PropertyDto>.Success(property));
		IReadOnlyList<Guid>? removedIds = null;
		_propertyService
			.Setup(s => s.UpdateAsync(
				property.Id, It.IsAny<PropertyFormRequest>(), It.IsAny<IReadOnlyList<PendingPhoto>>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
			.Callback<Guid, PropertyFormRequest, IReadOnlyList<PendingPhoto>, IReadOnlyList<Guid>, CancellationToken>((_, _, _, removed, _) => removedIds = removed)
			.ReturnsAsync(ApiResult<PropertyDto>.Success(property));
		_viewModel.PropertyId = property.Id.ToString();
		await _viewModel.AppearingCommand.ExecuteAsync(null);

		_viewModel.RemovePhotoCommand.Execute(_viewModel.Photos[0]);
		await _viewModel.SaveCommand.ExecuteAsync(null);

		Assert.Contains(imageId, removedIds!);
	}

	[Fact]
	public async Task AddPhotos_AtTheLimit_ShowsAMessageAndPicksNoMore()
	{
		await _viewModel.AppearingCommand.ExecuteAsync(null);
		for (var i = 0; i < 10; i++)
		{
			_viewModel.Photos.Add(new PhotoItem(null, null, new PendingPhoto(new MemoryStream(), $"{i}.jpg", "image/jpeg")));
		}

		await _viewModel.AddPhotosCommand.ExecuteAsync(null);

		Assert.Equal(10, _viewModel.Photos.Count);
		Assert.NotEmpty(_viewModel.PhotosError);
		_mediaPickerService.Verify(m => m.PickPhotosAsync(), Times.Never);
	}

	[Fact]
	public async Task UseCurrentLocation_FillsLatitudeAndLongitude()
	{
		_locationService.Setup(l => l.GetCurrentLocationAsync()).ReturnsAsync((6.9271, 79.8612));
		await _viewModel.AppearingCommand.ExecuteAsync(null);

		await _viewModel.UseCurrentLocationCommand.ExecuteAsync(null);

		Assert.Equal("6.9271", _viewModel.LatitudeText);
		Assert.Equal("79.8612", _viewModel.LongitudeText);
	}

	[Fact]
	public async Task UseCurrentLocation_WhenUnavailable_ShowsAMessageAndLeavesFieldsUnchanged()
	{
		_locationService.Setup(l => l.GetCurrentLocationAsync()).ReturnsAsync((ValueTuple<double, double>?)null);
		await _viewModel.AppearingCommand.ExecuteAsync(null);
		_viewModel.LatitudeText = "1.23";

		await _viewModel.UseCurrentLocationCommand.ExecuteAsync(null);

		Assert.Equal("1.23", _viewModel.LatitudeText);
		Assert.NotEmpty(_viewModel.LocationError);
	}

	[Fact]
	public void IsCaptureSupported_ReflectsTheMediaPickerService()
	{
		_mediaPickerService.SetupGet(m => m.IsCaptureSupported).Returns(true);

		var viewModel = new PropertyEditViewModel(_propertyService.Object, _mediaPickerService.Object, _locationService.Object, _navigationService.Object);

		Assert.True(viewModel.IsCaptureSupported);
	}

	[Fact]
	public void IsLocationAvailable_ReflectsTheLocationService()
	{
		_locationService.SetupGet(l => l.IsSupported).Returns(false);

		var viewModel = new PropertyEditViewModel(_propertyService.Object, _mediaPickerService.Object, _locationService.Object, _navigationService.Object);

		Assert.False(viewModel.IsLocationAvailable);
	}

	private void FillValidForm()
	{
		_viewModel.PropertyTitle = "Valid Title";
		_viewModel.Description = "A valid description.";
		_viewModel.Address = "Valid Address";
		_viewModel.City = "Colombo";
		_viewModel.PriceText = "25000000";
		_viewModel.BedroomsText = "3";
		_viewModel.BathroomsText = "2";
	}

	private void VerifyNothingSaved()
	{
		_propertyService.Verify(
			s => s.CreateAsync(It.IsAny<PropertyFormRequest>(), It.IsAny<IReadOnlyList<PendingPhoto>>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_propertyService.Verify(
			s => s.UpdateAsync(
				It.IsAny<Guid>(), It.IsAny<PropertyFormRequest>(), It.IsAny<IReadOnlyList<PendingPhoto>>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	private static PropertyDto CreateDto() => new(
		Guid.NewGuid(), "Test House", "Description", "House", "Available", 1_000_000m, "Address", "Colombo",
		null, null, 3, 2, null, null, Guid.NewGuid(), "Agent Name", []);
}
