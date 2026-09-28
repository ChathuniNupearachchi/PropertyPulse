namespace PropertyPulse.App.Services;

public static class Routes
{
	public const string Startup = "//startup";
	public const string Login = "//login";
	public const string Properties = "//properties";
	public const string Leads = "//leads";
	public const string Visits = "//visits";
	public const string Dashboard = "//dashboard";
	public const string Settings = "//settings";

	// Pushed onto the current tab's navigation stack rather than replacing the tab bar, so these are relative.
	public const string PropertyDetail = "propertydetail";
	public const string PropertyForm = "propertyform";
}
