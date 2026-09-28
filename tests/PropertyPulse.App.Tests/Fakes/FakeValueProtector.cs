using System.Security.Cryptography;
using PropertyPulse.App.Services;

namespace PropertyPulse.App.Tests;

public class FakeValueProtector : IValueProtector
{
	private const byte Mask = 0x5A;

	public bool RejectUnprotect { get; set; }

	public Task<byte[]> ProtectAsync(byte[] data) => Task.FromResult(Xor(data));

	public Task<byte[]> UnprotectAsync(byte[] data)
	{
		if (RejectUnprotect)
		{
			throw new CryptographicException("The data cannot be unprotected.");
		}

		return Task.FromResult(Xor(data));
	}

	private static byte[] Xor(byte[] data) => data.Select(b => (byte)(b ^ Mask)).ToArray();
}
