namespace PropertyPulse.App.Services;

/// <summary>A placeholder value for API calls that return no content, such as a delete.</summary>
public readonly record struct Unit
{
	public static readonly Unit Value = default;
}
