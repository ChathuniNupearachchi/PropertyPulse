using Moq;
using PropertyPulse.App.Services;
using PropertyPulse.App.ViewModels;

namespace PropertyPulse.App.Tests;

public class StartupViewModelTests
{
	private readonly Mock<ISessionService> _sessionService = new();
	private readonly Mock<INavigationService> _navigationService = new();

	[Fact]
	public async Task Appearing_WithARestorableSession_OpensTheTabs()
	{
		_sessionService.Setup(s => s.RestoreAsync()).ReturnsAsync(true);

		await CreateViewModel().AppearingCommand.ExecuteAsync(null);

		_navigationService.Verify(n => n.GoToAsync(Routes.Properties), Times.Once);
	}

	[Fact]
	public async Task Appearing_WithoutARestorableSession_OpensLogin()
	{
		_sessionService.Setup(s => s.RestoreAsync()).ReturnsAsync(false);

		await CreateViewModel().AppearingCommand.ExecuteAsync(null);

		_navigationService.Verify(n => n.GoToAsync(Routes.Login), Times.Once);
	}

	private StartupViewModel CreateViewModel() => new(_sessionService.Object, _navigationService.Object);
}
