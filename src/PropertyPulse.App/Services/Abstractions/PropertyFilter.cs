using System.Globalization;

namespace PropertyPulse.App.Services;

public record PropertyFilter(
	string? PropertyType = null,
	string? Status = null,
	string? City = null,
	decimal? MinPrice = null,
	decimal? MaxPrice = null,
	string? Search = null)
{
	public string ToQueryString(int page, int pageSize)
	{
		var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };

		AddIfSet(parts, "type", PropertyType);
		AddIfSet(parts, "status", Status);
		AddIfSet(parts, "city", City);
		AddIfSet(parts, "search", Search);
		AddIfSet(parts, "minPrice", MinPrice?.ToString(CultureInfo.InvariantCulture));
		AddIfSet(parts, "maxPrice", MaxPrice?.ToString(CultureInfo.InvariantCulture));

		return string.Join('&', parts);
	}

	private static void AddIfSet(List<string> parts, string name, string? value)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			parts.Add($"{name}={Uri.EscapeDataString(value)}");
		}
	}
}
