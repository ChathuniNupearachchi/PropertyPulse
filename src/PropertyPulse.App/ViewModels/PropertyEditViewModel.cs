using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PropertyPulse.App.Services;
using PropertyPulse.Shared.Properties;

namespace PropertyPulse.App.ViewModels;

// PropertyId is set from the "id" query parameter by the page's IQueryAttributable implementation, not by
// [QueryProperty] here, so this ViewModel stays free of MAUI references and compiles in the plain test project.
public partial class PropertyEditViewModel : BaseViewModel
{
	private const int MaxPhotos = 10;

	private readonly IPropertyService _propertyService;
	private readonly IMediaPickerService _mediaPickerService;
	private readonly ILocationService _locationService;
	private readonly INavigationService _navigationService;

	private readonly List<Guid> _removedImageIds = [];
	private Guid? _editingId;

	[ObservableProperty]
	public partial string? PropertyId { get; set; }

	public ObservableCollection<PhotoItem> Photos { get; } = [];

	public IReadOnlyList<string> PropertyTypeOptions { get; } = ["House", "Apartment", "Land", "Commercial"];
	public IReadOnlyList<string> StatusOptions { get; } = ["Available", "Reserved", "Sold", "Rented"];

	public bool IsCaptureSupported => _mediaPickerService.IsCaptureSupported;
	public bool IsLocationAvailable => _locationService.IsSupported;

	[ObservableProperty]
	public partial string PropertyTitle { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string Description { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string SelectedType { get; set; } = "House";

	[ObservableProperty]
	public partial string SelectedStatus { get; set; } = "Available";

	[ObservableProperty]
	public partial string PriceText { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string Address { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string City { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string LatitudeText { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string LongitudeText { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string BedroomsText { get; set; } = "0";

	[ObservableProperty]
	public partial string BathroomsText { get; set; } = "0";

	[ObservableProperty]
	public partial string FloorAreaText { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string LandSizeText { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string TitleError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string DescriptionError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string AddressError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string CityError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string PriceError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string BedroomsError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string BathroomsError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string FloorAreaError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string LandSizeError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string PhotosError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string LocationError { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string ErrorMessage { get; set; } = string.Empty;

	public PropertyEditViewModel(
		IPropertyService propertyService,
		IMediaPickerService mediaPickerService,
		ILocationService locationService,
		INavigationService navigationService)
	{
		_propertyService = propertyService;
		_mediaPickerService = mediaPickerService;
		_locationService = locationService;
		_navigationService = navigationService;
		Title = "Add property";
	}

	[RelayCommand]
	private async Task AppearingAsync()
	{
		ClearErrors();
		Photos.Clear();
		_removedImageIds.Clear();

		if (!Guid.TryParse(PropertyId, out var id))
		{
			_editingId = null;
			Title = "Add property";
			ResetFieldsForAdd();
			return;
		}

		_editingId = id;
		Title = "Edit property";

		IsBusy = true;
		try
		{
			var result = await _propertyService.GetAsync(id);
			if (!result.IsSuccess || result.Value is null)
			{
				ErrorMessage = "Could not load this property.";
				return;
			}

			ApplyToFields(result.Value);
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task CapturePhotoAsync()
	{
		if (!TryReservePhotoSlot())
		{
			return;
		}

		var photo = await _mediaPickerService.CapturePhotoAsync();
		if (photo is not null)
		{
			Photos.Add(new PhotoItem(null, null, photo));
		}
	}

	[RelayCommand]
	private async Task AddPhotosAsync()
	{
		if (!TryReservePhotoSlot())
		{
			return;
		}

		var photos = await _mediaPickerService.PickPhotosAsync();
		foreach (var photo in photos)
		{
			if (Photos.Count >= MaxPhotos)
			{
				PhotosError = $"You can add up to {MaxPhotos} photos.";
				break;
			}

			Photos.Add(new PhotoItem(null, null, photo));
		}
	}

	[RelayCommand]
	private void RemovePhoto(PhotoItem? item)
	{
		if (item is null)
		{
			return;
		}

		if (item.ExistingImageId is { } id)
		{
			_removedImageIds.Add(id);
		}

		Photos.Remove(item);
		PhotosError = string.Empty;
	}

	[RelayCommand]
	private async Task UseCurrentLocationAsync()
	{
		var location = await _locationService.GetCurrentLocationAsync();
		if (location is null)
		{
			LocationError = "Could not read the device's current location.";
			return;
		}

		LocationError = string.Empty;
		LatitudeText = location.Value.Latitude.ToString(CultureInfo.InvariantCulture);
		LongitudeText = location.Value.Longitude.ToString(CultureInfo.InvariantCulture);
	}

	[RelayCommand]
	private async Task SaveAsync()
	{
		if (!Validate())
		{
			return;
		}

		IsBusy = true;
		try
		{
			var request = new PropertyFormRequest(
				PropertyTitle.Trim(),
				Description.Trim(),
				SelectedType,
				SelectedStatus,
				decimal.Parse(PriceText, CultureInfo.InvariantCulture),
				Address.Trim(),
				City.Trim(),
				ParseOptionalDouble(LatitudeText),
				ParseOptionalDouble(LongitudeText),
				int.Parse(BedroomsText, CultureInfo.InvariantCulture),
				int.Parse(BathroomsText, CultureInfo.InvariantCulture),
				ParseOptionalDouble(FloorAreaText),
				ParseOptionalDouble(LandSizeText));

			var pendingPhotos = Photos.Where(p => p.Pending is not null).Select(p => p.Pending!).ToList();

			var result = _editingId is { } id
				? await _propertyService.UpdateAsync(id, request, pendingPhotos, _removedImageIds)
				: await _propertyService.CreateAsync(request, pendingPhotos);

			if (!result.IsSuccess || result.Value is null)
			{
				ErrorMessage = DescribeError(result.Error);
				return;
			}

			if (_editingId is not null)
			{
				await _navigationService.GoToAsync("..");
			}
			else
			{
				await _navigationService.GoToAsync($"{Routes.PropertyDetail}?id={result.Value.Id}");
			}
		}
		finally
		{
			IsBusy = false;
		}
	}

	private bool TryReservePhotoSlot()
	{
		PhotosError = string.Empty;

		if (Photos.Count < MaxPhotos)
		{
			return true;
		}

		PhotosError = $"You can add up to {MaxPhotos} photos.";
		return false;
	}

	private void ResetFieldsForAdd()
	{
		PropertyTitle = string.Empty;
		Description = string.Empty;
		SelectedType = PropertyTypeOptions[0];
		SelectedStatus = StatusOptions[0];
		PriceText = string.Empty;
		Address = string.Empty;
		City = string.Empty;
		LatitudeText = string.Empty;
		LongitudeText = string.Empty;
		BedroomsText = "0";
		BathroomsText = "0";
		FloorAreaText = string.Empty;
		LandSizeText = string.Empty;
	}

	private void ApplyToFields(PropertyDto property)
	{
		PropertyTitle = property.Title;
		Description = property.Description;
		SelectedType = property.PropertyType;
		SelectedStatus = property.Status;
		PriceText = property.PriceLkr.ToString(CultureInfo.InvariantCulture);
		Address = property.Address;
		City = property.City;
		LatitudeText = property.Latitude?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
		LongitudeText = property.Longitude?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
		BedroomsText = property.Bedrooms.ToString(CultureInfo.InvariantCulture);
		BathroomsText = property.Bathrooms.ToString(CultureInfo.InvariantCulture);
		FloorAreaText = property.FloorAreaSqFt?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
		LandSizeText = property.LandSizePerches?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

		foreach (var image in property.Images)
		{
			Photos.Add(new PhotoItem(image.Id, image.Url, null));
		}
	}

	private bool Validate()
	{
		ClearErrors();

		TitleError = string.IsNullOrWhiteSpace(PropertyTitle) ? "Enter a title." : string.Empty;
		DescriptionError = string.IsNullOrWhiteSpace(Description) ? "Enter a description." : string.Empty;
		AddressError = string.IsNullOrWhiteSpace(Address) ? "Enter an address." : string.Empty;
		CityError = string.IsNullOrWhiteSpace(City) ? "Enter a city." : string.Empty;

		PriceError = decimal.TryParse(PriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) && price > 0
			? string.Empty
			: "Enter a price greater than zero.";

		BedroomsError = int.TryParse(BedroomsText, out var bedrooms) && bedrooms >= 0
			? string.Empty
			: "Enter zero or more bedrooms.";

		BathroomsError = int.TryParse(BathroomsText, out var bathrooms) && bathrooms >= 0
			? string.Empty
			: "Enter zero or more bathrooms.";

		FloorAreaError = IsBlankOrPositive(FloorAreaText) ? string.Empty : "Enter a positive floor area, or leave it blank.";
		LandSizeError = IsBlankOrPositive(LandSizeText) ? string.Empty : "Enter a positive land size, or leave it blank.";

		return TitleError.Length == 0 && DescriptionError.Length == 0 && AddressError.Length == 0 && CityError.Length == 0 &&
			PriceError.Length == 0 && BedroomsError.Length == 0 && BathroomsError.Length == 0 &&
			FloorAreaError.Length == 0 && LandSizeError.Length == 0;
	}

	private void ClearErrors()
	{
		TitleError = string.Empty;
		DescriptionError = string.Empty;
		AddressError = string.Empty;
		CityError = string.Empty;
		PriceError = string.Empty;
		BedroomsError = string.Empty;
		BathroomsError = string.Empty;
		FloorAreaError = string.Empty;
		LandSizeError = string.Empty;
		PhotosError = string.Empty;
		LocationError = string.Empty;
		ErrorMessage = string.Empty;
	}

	private static bool IsBlankOrPositive(string text) =>
		string.IsNullOrWhiteSpace(text) ||
		(double.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) && value > 0);

	private static double? ParseOptionalDouble(string text) =>
		double.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : null;

	private static string DescribeError(ApiError error) => error switch
	{
		ApiError.Network => "Could not reach the server. Check your connection and try again.",
		ApiError.Unauthorized => "Your session has expired. Please sign in again.",
		ApiError.Forbidden => "You do not have permission to do that.",
		ApiError.Rejected => "Check the form for problems and try again.",
		_ => "Something went wrong. Please try again."
	};
}
