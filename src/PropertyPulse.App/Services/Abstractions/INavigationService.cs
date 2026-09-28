namespace PropertyPulse.App.Services;

public interface INavigationService
{
	Task GoToAsync(string route);
}
