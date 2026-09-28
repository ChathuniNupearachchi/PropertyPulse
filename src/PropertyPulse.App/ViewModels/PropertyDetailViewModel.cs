using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PropertyPulse.App.Services;
using PropertyPulse.Shared.Properties;

namespace PropertyPulse.App.ViewModels;

// PropertyId is set from the "id" query parameter by the page's IQueryAttributable implementation, not by
// [QueryProperty] here, so this ViewModel stays free of MAUI references and compiles in the plain test project.
public partial class PropertyDetailViewModel : BaseViewModel
{
	private readonly IPropertyService _propertyService;
	private readonly ISessionService _sessionService;
	private readonly INavigationService _navigationService;
	private readonly IDialogService _dialogService;

	[ObservableProperty]
	public partial string? PropertyId { get; set; }

	[ObservableProperty]
	public partial PropertyDto? Property { get; set; }

	[ObservableProperty]
	public partial bool CanEdit { get; set; }

	[ObservableProperty]
	public partial bool NotFound { get; set; }

	[ObservableProperty]
	public partial string ErrorMessage { get; set; } = string.Empty;

	public PropertyDetailViewModel(
		IPropertyService propertyService, ISessionService sessionService, INavigationService navigationService, IDialogService dialogService)
	{
		_propertyService = propertyService;
		_sessionService = sessionService;
		_navigationService = navigationService;
		_dialogService = dialogService;
		Title = "Property";
	}

	[RelayCommand]
	private Task AppearingAsync() => LoadAsync();

	[RelayCommand]
	private Task RetryAsync() => LoadAsync();

	[RelayCommand]
	private Task EditAsync() =>
		Property is null ? Task.CompletedTask : _navigationService.GoToAsync($"{Routes.PropertyForm}?id={Property.Id}");

	[RelayCommand]
	private async Task DeleteAsync()
	{
		if (Property is null)
		{
			return;
		}

		var confirmed = await _dialogService.ConfirmAsync(
			"Delete property", $"Delete \"{Property.Title}\"? This cannot be undone.", "Delete", "Cancel");
		if (!confirmed)
		{
			return;
		}

		IsBusy = true;
		try
		{
			var result = await _propertyService.DeleteAsync(Property.Id);
			if (!result.IsSuccess)
			{
				ErrorMessage = DescribeError(result.Error);
				return;
			}

			await _navigationService.GoToAsync("..");
		}
		finally
		{
			IsBusy = false;
		}
	}

	private async Task LoadAsync()
	{
		ErrorMessage = string.Empty;
		NotFound = false;
		CanEdit = false;

		if (!Guid.TryParse(PropertyId, out var id))
		{
			NotFound = true;
			return;
		}

		IsBusy = true;
		try
		{
			var result = await _propertyService.GetAsync(id);
			if (!result.IsSuccess)
			{
				if (result.Error == ApiError.Rejected)
				{
					NotFound = true;
				}
				else
				{
					ErrorMessage = DescribeError(result.Error);
				}

				return;
			}

			Property = result.Value;
			var user = _sessionService.Current?.User;
			CanEdit = user is not null && Property is not null &&
				(user.Role == UserRoles.Manager || user.Id == Property.AgentId);
		}
		finally
		{
			IsBusy = false;
		}
	}

	private static string DescribeError(ApiError error) => error switch
	{
		ApiError.Network => "Could not reach the server. Check your connection and try again.",
		ApiError.Unauthorized => "Your session has expired. Please sign in again.",
		_ => "Something went wrong. Please try again."
	};
}
