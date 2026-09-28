using Moq;
using PropertyPulse.App.Services;
using PropertyPulse.App.ViewModels;
using PropertyPulse.Shared.Auth;

namespace PropertyPulse.App.Tests;

public class SettingsViewModelTests
{
	private readonly Mock<ISessionService> _sessionService = new();

	[Fact]
	public void Constructor_SetsTheTitle()
	{
		Assert.Equal("Settings", new SettingsViewModel(_sessionService.Object).Title);
	}

	[Fact]
	public void Appearing_ShowsTheSignedInUser()
	{
		_sessionService.SetupGet(s => s.Current).Returns(
			new AuthResponse("token", DateTime.UtcNow.AddHours(1), new UserDto(Guid.NewGuid(), "Nimali Perera", "manager@propertypulse.lk", "Manager")));
		var viewModel = new SettingsViewModel(_sessionService.Object);

		viewModel.AppearingCommand.Execute(null);

		Assert.Equal("Nimali Perera", viewModel.FullName);
		Assert.Equal("manager@propertypulse.lk", viewModel.Email);
		Assert.Equal("Manager", viewModel.Role);
	}

	[Fact]
	public async Task SignOut_EndsTheSessionAsSignedOut()
	{
		_sessionService.Setup(s => s.EndAsync(SessionEndReason.SignedOut)).Returns(Task.CompletedTask);
		var viewModel = new SettingsViewModel(_sessionService.Object);

		await viewModel.SignOutCommand.ExecuteAsync(null);

		_sessionService.Verify(s => s.EndAsync(SessionEndReason.SignedOut), Times.Once);
	}
}
