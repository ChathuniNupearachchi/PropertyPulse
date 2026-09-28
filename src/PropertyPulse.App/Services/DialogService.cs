namespace PropertyPulse.App.Services;

public class DialogService : IDialogService
{
	public Task<bool> ConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No") =>
		MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlertAsync(title, message, accept, cancel));
}
