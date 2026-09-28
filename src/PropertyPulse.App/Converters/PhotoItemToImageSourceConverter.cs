using System.Globalization;
using PropertyPulse.App.ViewModels;

namespace PropertyPulse.App.Converters;

public class PhotoItemToImageSourceConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value is PhotoItem { IsPending: false, ExistingUrl: { } url } ? url : "property_placeholder.png";

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
