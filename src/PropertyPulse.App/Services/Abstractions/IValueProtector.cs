namespace PropertyPulse.App.Services;

public interface IValueProtector
{
	Task<byte[]> ProtectAsync(byte[] data);

	Task<byte[]> UnprotectAsync(byte[] data);
}
