using Moq;
using PropertyPulse.App.Services;
using PropertyPulse.App.ViewModels;
using PropertyPulse.Shared.Auth;
using PropertyPulse.Shared.Properties;

namespace PropertyPulse.App.Tests;

public class PropertyDetailViewModelTests
{
	private readonly Mock<IPropertyService> _propertyService = new();
	private readonly Mock<ISessionService> _sessionService = new();
	private readonly Mock<INavigationService> _navigationService = new();
	private readonly Mock<IDialogService> _dialogService = new();
	private readonly Guid _agentId = Guid.NewGuid();
	private readonly PropertyDetailViewModel _viewModel;

	public PropertyDetailViewModelTests()
	{
		_viewModel = new PropertyDetailViewModel(
			_propertyService.Object, _sessionService.Object, _navigationService.Object, _dialogService.Object);
	}

	[Fact]
	public async Task Appearing_WithAnExistingProperty_LoadsItsFields()
	{
		var property = CreateProperty(_agentId);
		SetupGet(property);
		SignInAs(_agentId, "Agent");
		_viewModel.PropertyId = property.Id.ToString();

		await _viewModel.AppearingCommand.ExecuteAsync(null);

		Assert.Equal(property, _viewModel.Property);
		Assert.False(_viewModel.NotFound);
	}

	[Fact]
	public async Task CanEdit_ForTheOwningAgent_IsTrue()
	{
		var property = CreateProperty(_agentId);
		SetupGet(property);
		SignInAs(_agentId, "Agent");
		_viewModel.PropertyId = property.Id.ToString();

		await _viewModel.AppearingCommand.ExecuteAsync(null);

		Assert.True(_viewModel.CanEdit);
	}

	[Fact]
	public async Task CanEdit_ForAManager_IsTrueRegardlessOfOwner()
	{
		var property = CreateProperty(_agentId);
		SetupGet(property);
		SignInAs(Guid.NewGuid(), "Manager");
		_viewModel.PropertyId = property.Id.ToString();

		await _viewModel.AppearingCommand.ExecuteAsync(null);

		Assert.True(_viewModel.CanEdit);
	}

	[Fact]
	public async Task CanEdit_ForANonOwningAgent_IsFalse()
	{
		var property = CreateProperty(_agentId);
		SetupGet(property);
		SignInAs(Guid.NewGuid(), "Agent");
		_viewModel.PropertyId = property.Id.ToString();

		await _viewModel.AppearingCommand.ExecuteAsync(null);

		Assert.False(_viewModel.CanEdit);
	}

	[Fact]
	public async Task Delete_WhenConfirmed_DeletesAndNavigatesBack()
	{
		var property = CreateProperty(_agentId);
		SetupGet(property);
		SignInAs(_agentId, "Agent");
		_viewModel.PropertyId = property.Id.ToString();
		await _viewModel.AppearingCommand.ExecuteAsync(null);
		_dialogService.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
		_propertyService.Setup(s => s.DeleteAsync(property.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ApiResult<Unit>.Success(Unit.Value));

		await _viewModel.DeleteCommand.ExecuteAsync(null);

		_propertyService.Verify(s => s.DeleteAsync(property.Id, It.IsAny<CancellationToken>()), Times.Once);
		_navigationService.Verify(n => n.GoToAsync(".."), Times.Once);
	}

	[Fact]
	public async Task Delete_WhenCancelled_DoesNotDeleteOrNavigate()
	{
		var property = CreateProperty(_agentId);
		SetupGet(property);
		SignInAs(_agentId, "Agent");
		_viewModel.PropertyId = property.Id.ToString();
		await _viewModel.AppearingCommand.ExecuteAsync(null);
		_dialogService.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

		await _viewModel.DeleteCommand.ExecuteAsync(null);

		_propertyService.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
		_navigationService.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public async Task Appearing_WhenThePropertyNoLongerExists_ShowsNotFound()
	{
		_propertyService
			.Setup(s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(ApiResult<PropertyDto>.Failure(ApiError.Rejected));
		_viewModel.PropertyId = Guid.NewGuid().ToString();

		await _viewModel.AppearingCommand.ExecuteAsync(null);

		Assert.True(_viewModel.NotFound);
		Assert.Null(_viewModel.Property);
	}

	[Fact]
	public async Task Appearing_WhenTheLoadFails_ShowsAnErrorMessage()
	{
		_propertyService
			.Setup(s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(ApiResult<PropertyDto>.Failure(ApiError.Network));
		_viewModel.PropertyId = Guid.NewGuid().ToString();

		await _viewModel.AppearingCommand.ExecuteAsync(null);

		Assert.False(_viewModel.NotFound);
		Assert.NotEmpty(_viewModel.ErrorMessage);
	}

	[Fact]
	public async Task Retry_ReloadsTheProperty()
	{
		var property = CreateProperty(_agentId);
		_propertyService
			.SetupSequence(s => s.GetAsync(property.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(ApiResult<PropertyDto>.Failure(ApiError.Network))
			.ReturnsAsync(ApiResult<PropertyDto>.Success(property));
		SignInAs(_agentId, "Agent");
		_viewModel.PropertyId = property.Id.ToString();
		await _viewModel.AppearingCommand.ExecuteAsync(null);

		await _viewModel.RetryCommand.ExecuteAsync(null);

		Assert.Equal(property, _viewModel.Property);
		Assert.Empty(_viewModel.ErrorMessage);
	}

	private void SetupGet(PropertyDto property) =>
		_propertyService
			.Setup(s => s.GetAsync(property.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(ApiResult<PropertyDto>.Success(property));

	private void SignInAs(Guid userId, string role) =>
		_sessionService.SetupGet(s => s.Current).Returns(
			new AuthResponse("token", DateTime.UtcNow.AddHours(1), new UserDto(userId, "Test User", "test@propertypulse.lk", role)));

	private static PropertyDto CreateProperty(Guid agentId) => new(
		Guid.NewGuid(), "Test House", "Description", "House", "Available", 1_000_000m, "Address", "Colombo",
		null, null, 3, 2, null, null, agentId, "Agent Name", []);
}
