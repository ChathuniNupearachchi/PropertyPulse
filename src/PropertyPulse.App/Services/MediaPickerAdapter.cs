namespace PropertyPulse.App.Services;

// On Windows, MediaPicker.Default.IsCaptureSupported is false (no unified camera capture there), so both
// actions fall back to the file picker, matching the "choose a file" fallback the form offers on that platform.
public class MediaPickerAdapter : IMediaPickerService
{
	public bool IsCaptureSupported => MediaPicker.Default.IsCaptureSupported;

	public async Task<PendingPhoto?> CapturePhotoAsync()
	{
		if (!IsCaptureSupported)
		{
			var picked = await PickPhotosAsync();
			return picked.Count > 0 ? picked[0] : null;
		}

		var photo = await MediaPicker.Default.CapturePhotoAsync();
		return photo is null ? null : await ToPendingPhotoAsync(photo);
	}

	public async Task<IReadOnlyList<PendingPhoto>> PickPhotosAsync()
	{
		var files = await FilePicker.Default.PickMultipleAsync(new PickOptions
		{
			PickerTitle = "Choose photos",
			FileTypes = FilePickerFileType.Images
		});

		if (files is null)
		{
			return [];
		}

		var photos = new List<PendingPhoto>();
		foreach (var file in files)
		{
			if (file is not null)
			{
				photos.Add(await ToPendingPhotoAsync(file));
			}
		}

		return photos;
	}

	private static async Task<PendingPhoto> ToPendingPhotoAsync(FileResult file) =>
		new(await file.OpenReadAsync(), file.FileName, GetContentType(file.FileName));

	private static string GetContentType(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
	{
		".png" => "image/png",
		".webp" => "image/webp",
		_ => "image/jpeg"
	};
}
