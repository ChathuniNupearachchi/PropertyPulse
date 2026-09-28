using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;

namespace PropertyPulse.App.Services;

// MAUI's SecureStorage needs a packaged app on Windows, so the unpackaged build protects values with the Windows data
// protection API instead. LOCAL=user ties them to the current Windows user account.
public class DpapiValueProtector : IValueProtector
{
	private const string Descriptor = "LOCAL=user";

	public async Task<byte[]> ProtectAsync(byte[] data)
	{
		var provider = new DataProtectionProvider(Descriptor);
		var protectedBuffer = await provider.ProtectAsync(CryptographicBuffer.CreateFromByteArray(data));

		CryptographicBuffer.CopyToByteArray(protectedBuffer, out var protectedBytes);
		return protectedBytes;
	}

	public async Task<byte[]> UnprotectAsync(byte[] data)
	{
		var provider = new DataProtectionProvider();
		var buffer = await provider.UnprotectAsync(CryptographicBuffer.CreateFromByteArray(data));

		CryptographicBuffer.CopyToByteArray(buffer, out var bytes);
		return bytes;
	}
}
