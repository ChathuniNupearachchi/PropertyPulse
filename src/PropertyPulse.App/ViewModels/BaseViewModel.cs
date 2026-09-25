using CommunityToolkit.Mvvm.ComponentModel;

namespace PropertyPulse.App.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
	[ObservableProperty]
	public partial bool IsBusy { get; set; }

	[ObservableProperty]
	public partial string Title { get; set; } = string.Empty;
}
