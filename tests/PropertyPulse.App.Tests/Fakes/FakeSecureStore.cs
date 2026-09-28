using PropertyPulse.App.Services;

namespace PropertyPulse.App.Tests;

public class FakeSecureStore : ISecureStore
{
	public Dictionary<string, string> Values { get; } = [];

	public bool ThrowOnSet { get; set; }

	public Task<string?> GetAsync(string key) => Task.FromResult(Values.GetValueOrDefault(key));

	public Task SetAsync(string key, string value)
	{
		if (ThrowOnSet)
		{
			throw new InvalidOperationException("Secure storage is unavailable.");
		}

		Values[key] = value;
		return Task.CompletedTask;
	}

	public void Remove(string key) => Values.Remove(key);
}
