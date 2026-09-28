namespace PropertyPulse.App.Services;

public interface IDialogService
{
	Task<bool> ConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No");
}
