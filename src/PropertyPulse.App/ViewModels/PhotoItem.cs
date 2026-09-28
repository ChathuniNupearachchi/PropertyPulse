using PropertyPulse.App.Services;

namespace PropertyPulse.App.ViewModels;

/// <summary>One entry in the add/edit form's photo list: either a property's existing photo or one just picked, pending save.</summary>
public record PhotoItem(Guid? ExistingImageId, string? ExistingUrl, PendingPhoto? Pending)
{
	public bool IsPending => Pending is not null;

	public string DisplayName => IsPending ? Pending!.FileName : "Existing photo";
}
