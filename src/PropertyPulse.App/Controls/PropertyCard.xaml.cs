using PropertyPulse.Shared.Properties;

namespace PropertyPulse.App.Controls;

public partial class PropertyCard : ContentView
{
	public static readonly BindableProperty PropertyProperty =
		BindableProperty.Create(nameof(Property), typeof(PropertyDto), typeof(PropertyCard));

	public PropertyCard()
	{
		InitializeComponent();
	}

	public PropertyDto? Property
	{
		get => (PropertyDto?)GetValue(PropertyProperty);
		set => SetValue(PropertyProperty, value);
	}
}
