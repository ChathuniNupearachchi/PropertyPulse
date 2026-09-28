using Moq;
using PropertyPulse.App.Services;
using PropertyPulse.App.ViewModels;
using PropertyPulse.Shared.Auth;

namespace PropertyPulse.App.Tests;

public class ShellViewModelTests
{
	private readonly Mock<ISessionService> _sessionService = new();
	private readonly Mock<INavigationService> _navigationService = new();
	private readonly ShellViewModel _viewModel;

	public ShellViewModelTests()
	{
		_viewModel = new ShellViewModel(_sessionService.Object, _navigationService.Object);
	}

	[Fact]
	public void IsManager_IsFalseBeforeAnySession()
	{
		Assert.False(_viewModel.IsManager);
	}

	[Theory]
	[InlineData("Manager", true)]
	[InlineData("Agent", false)]
	public void SessionChanged_SetsIsManagerFromTheRole(string role, bool expected)
	{
		RaiseSessionChanged(CreateSession(role));

		Assert.Equal(expected, _viewModel.IsManager);
		_navigationService.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void SessionChanged_WhenTheSessionEnds_HidesManagerTabsAndOpensLogin()
	{
		RaiseSessionChanged(CreateSession("Manager"));

		RaiseSessionChanged(null);

		Assert.False(_viewModel.IsManager);
		_navigationService.Verify(n => n.GoToAsync(Routes.Login), Times.Once);
	}

	[Fact]
	public void SessionChanged_ForADifferentRole_UpdatesIsManager()
	{
		RaiseSessionChanged(CreateSession("Manager"));
		RaiseSessionChanged(null);

		RaiseSessionChanged(CreateSession("Agent"));

		Assert.False(_viewModel.IsManager);
	}

	private void RaiseSessionChanged(AuthResponse? session)
	{
		_sessionService.SetupGet(s => s.Current).Returns(session);
		_sessionService.Raise(s => s.SessionChanged += null, EventArgs.Empty);
	}

	private static AuthResponse CreateSession(string role) =>
		new("token", DateTime.UtcNow.AddHours(1), new UserDto(Guid.NewGuid(), "Test User", "user@propertypulse.lk", role));
}
