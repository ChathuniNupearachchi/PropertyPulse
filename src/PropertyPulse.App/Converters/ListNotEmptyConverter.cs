using System.Collections;
using System.Globalization;

namespace PropertyPulse.App.Converters;

public class ListNotEmptyConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var hasItems = value is ICollection { Count: > 0 };
		return string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase) ? !hasItems : hasItems;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
