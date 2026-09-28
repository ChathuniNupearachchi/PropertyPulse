using System.Text;

namespace PropertyPulse.App.Services;

public class ProtectedFileStore(string directory, IValueProtector protector) : ISecureStore
{
	public async Task<string?> GetAsync(string key)
	{
		var path = GetPath(key);
		if (!File.Exists(path))
		{
			return null;
		}

		var protectedBytes = await File.ReadAllBytesAsync(path);
		var bytes = await protector.UnprotectAsync(protectedBytes);
		return Encoding.UTF8.GetString(bytes);
	}

	public async Task SetAsync(string key, string value)
	{
		Directory.CreateDirectory(directory);

		var protectedBytes = await protector.ProtectAsync(Encoding.UTF8.GetBytes(value));

		// Write to a temporary file first so a crash cannot leave a half-written session behind.
		var path = GetPath(key);
		var temporaryPath = path + ".tmp";
		await File.WriteAllBytesAsync(temporaryPath, protectedBytes);
		File.Move(temporaryPath, path, overwrite: true);
	}

	public void Remove(string key)
	{
		var path = GetPath(key);
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	private string GetPath(string key) => Path.Combine(directory, Uri.EscapeDataString(key) + ".bin");
}
