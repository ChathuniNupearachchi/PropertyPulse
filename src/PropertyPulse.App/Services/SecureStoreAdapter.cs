namespace PropertyPulse.App.Services;

public class SecureStoreAdapter : ISecureStore
{
	public Task<string?> GetAsync(string key) => SecureStorage.Default.GetAsync(key);

	public Task SetAsync(string key, string value) => SecureStorage.Default.SetAsync(key, value);

	public void Remove(string key) => SecureStorage.Default.Remove(key);
}
