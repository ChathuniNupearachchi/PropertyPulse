namespace PropertyPulse.App.Services;

// Windows has no consistent Geolocation implementation in MAUI, so the form hides "use current location" there
// and lets the user type coordinates by hand instead.
public class LocationAdapter : ILocationService
{
	public bool IsSupported => DeviceInfo.Platform != DevicePlatform.WinUI;

	public async Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync()
	{
		if (!IsSupported)
		{
			return null;
		}

		try
		{
			var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
			var location = await Geolocation.Default.GetLocationAsync(request);
			return location is null ? null : (location.Latitude, location.Longitude);
		}
		catch (Exception)
		{
			// Covers a refused permission, a disabled location service, and any other platform failure:
			// the caller only needs to know the location could not be read.
			return null;
		}
	}
}
