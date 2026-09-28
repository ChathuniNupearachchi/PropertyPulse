namespace PropertyPulse.App.Services;

public class ApiSettings : IApiSettings
{
	private const string BaseUrlKey = "api_base_url";

	// 10.0.2.2 is how the Android emulator reaches the computer it runs on.
	public string DefaultBaseUrl =>
		DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5000" : "http://localhost:5000";

	public string BaseUrl
	{
		get => Preferences.Default.Get(BaseUrlKey, DefaultBaseUrl);
		set => Preferences.Default.Set(BaseUrlKey, value.Trim().TrimEnd('/'));
	}
}
