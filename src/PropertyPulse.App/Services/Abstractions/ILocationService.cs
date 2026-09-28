namespace PropertyPulse.App.Services;

public interface ILocationService
{
	bool IsSupported { get; }

	Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync();
}
