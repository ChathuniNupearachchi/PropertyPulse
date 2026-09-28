namespace PropertyPulse.App.Services;

public interface IApiSettings
{
	string BaseUrl { get; set; }

	string DefaultBaseUrl { get; }
}
