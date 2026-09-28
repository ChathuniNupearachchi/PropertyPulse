namespace PropertyPulse.App.Services;

public class ShellNavigationService : INavigationService
{
	private const int MaxAttempts = 20;
	private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(100);

	// The dispatcher always queues the work, even when already on the UI thread. That matters at start-up: the Startup page
	// asks to navigate while Shell is still connecting its handlers during launch, and navigating from inside that
	// call stack makes Shell throw and crash the app. A 401 is also noticed on a background thread, so the UI thread is needed anyway.
	public Task GoToAsync(string route) =>
		Application.Current!.Dispatcher.DispatchAsync(() => NavigateAsync(route));

	private static async Task NavigateAsync(string route)
	{
		for (var attempt = 1; ; attempt++)
		{
			try
			{
				await Shell.Current.GoToAsync(route);
				return;
			}
			// Shell rejects a navigation while its own start-up navigation is still being processed.
			catch (InvalidOperationException ex) when (attempt < MaxAttempts && ex.Message.Contains("Pending Navigations"))
			{
				await Task.Delay(RetryDelay);
			}
		}
	}
}
