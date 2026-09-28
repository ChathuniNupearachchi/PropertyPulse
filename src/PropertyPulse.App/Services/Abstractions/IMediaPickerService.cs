namespace PropertyPulse.App.Services;

public interface IMediaPickerService
{
	bool IsCaptureSupported { get; }

	Task<PendingPhoto?> CapturePhotoAsync();

	Task<IReadOnlyList<PendingPhoto>> PickPhotosAsync();
}
