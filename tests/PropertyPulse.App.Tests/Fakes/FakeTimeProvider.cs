namespace PropertyPulse.App.Tests;

public class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
	public override DateTimeOffset GetUtcNow() => now;
}
