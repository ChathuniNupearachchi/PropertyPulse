using System.Globalization;
using PropertyPulse.Shared.Properties;

namespace PropertyPulse.App.Converters;

public class PropertyImagesToCoverUrlConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value is List<PropertyImageDto> { Count: > 0 } images ? images[0].Url : "property_placeholder.png";

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
