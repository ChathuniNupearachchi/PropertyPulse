using Moq;
using PropertyPulse.App.Services;
using PropertyPulse.App.ViewModels;
using PropertyPulse.Shared.Auth;

namespace PropertyPulse.App.Tests;

public class LoginViewModelTests
{
	private const string ValidEmail = "agent1@propertypulse.lk";
	private const string ValidPassword = "Password123!";

	private readonly Mock<IAuthService> _authService = new();
	private readonly Mock<IApiSettings> _apiSettings = new();
	private readonly Mock<ISessionService> _sessionService = new();
	private readonly Mock<INavigationService> _navigationService = new();
	private readonly LoginViewModel _viewModel;

	public LoginViewModelTests()
	{
		_apiSettings.SetupProperty(s => s.BaseUrl, "http://localhost:5000");
		_viewModel = new LoginViewModel(_authService.Object, _apiSettings.Object, _sessionService.Object, _navigationService.Object);
		_viewModel.AppearingCommand.Execute(null);
	}

	[Fact]
	public async Task SignIn_WithEmptyFields_ShowsAMessageForEachFieldAndSendsNothing()
	{
		_viewModel.ServerUrl = string.Empty;

		await _viewModel.SignInCommand.ExecuteAsync(null);

		Assert.NotEmpty(_viewModel.EmailError);
		Assert.NotEmpty(_viewModel.PasswordError);
		Assert.NotEmpty(_viewModel.ServerUrlError);
		VerifyNoRequestSent();
	}

	[Theory]
	[InlineData("not-an-email")]
	[InlineData("agent1@")]
	[InlineData("Kasun <agent1@propertypulse.lk>")]
	public async Task SignIn_WithMalformedEmail_ShowsEmailErrorAndSendsNothing(string email)
	{
		FillValidForm();
		_viewModel.Email = email;

		await _viewModel.SignInCommand.ExecuteAsync(null);

		Assert.NotEmpty(_viewModel.EmailError);
		Assert.Empty(_viewModel.PasswordError);
		VerifyNoRequestSent();
	}

	[Fact]
	public async Task SignIn_WithEmptyPassword_ShowsPasswordErrorAndSendsNothing()
	{
		FillValidForm();
		_viewModel.Password = string.Empty;

		await _viewModel.SignInCommand.ExecuteAsync(null);

		Assert.NotEmpty(_viewModel.PasswordError);
		VerifyNoRequestSent();
	}

	[Theory]
	[InlineData("")]
	[InlineData("not a url")]
	[InlineData("ftp://192.168.1.20")]
	[InlineData("localhost:5000")]
	public async Task SignIn_WithInvalidServerAddress_ShowsServerErrorAndSendsNothing(string serverUrl)
	{
		FillValidForm();
		_viewModel.ServerUrl = serverUrl;

		await _viewModel.SignInCommand.ExecuteAsync(null);

		Assert.NotEmpty(_viewModel.ServerUrlError);
		VerifyNoRequestSent();
	}

	[Fact]
	public async Task SignIn_WithValidInput_SendsTheTrimmedEmailAndPassword()
	{
		SetupLogin(ApiResult<AuthResponse>.Failure(ApiError.Network));
		FillValidForm();
		_viewModel.Email = $"  {ValidEmail} ";

		await _viewModel.SignInCommand.ExecuteAsync(null);

		_authService.Verify(a => a.LoginAsync(ValidEmail, ValidPassword, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task SignIn_OnSuccess_SavesTheServerAddressAndOpensTheTabs()
	{
		SetupLogin(ApiResult<AuthResponse>.Success(CreateResponse()));
		FillValidForm();
		_viewModel.ServerUrl = "http://192.168.1.20:5000/";

		await _viewModel.SignInCommand.ExecuteAsync(null);

		Assert.Equal("http://192.168.1.20:5000", _apiSettings.Object.BaseUrl);
		_navigationService.Verify(n => n.GoToAsync(Routes.Properties), Times.Once);
		Assert.Empty(_viewModel.ErrorMessage);
	}

	[Fact]
	public async Task SignIn_WithWrongCredentials_ShowsInvalidCredentialsAndKeepsTheEmail()
	{
		SetupLogin(ApiResult<AuthResponse>.Failure(ApiError.Unauthorized));
		FillValidForm();

		await _viewModel.SignInCommand.ExecuteAsync(null);

		Assert.Equal("Invalid email or password.", _viewModel.ErrorMessage);
		Assert.Equal(ValidEmail, _viewModel.Email);
		_navigationService.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public async Task SignIn_WhenTheServerIsUnreachable_MentionsTheServerAddress()
	{
		SetupLogin(ApiResult<AuthResponse>.Failure(ApiError.Network));
		FillValidForm();

		await _viewModel.SignInCommand.ExecuteAsync(null);

		Assert.Contains("Could not reach the server", _viewModel.ErrorMessage);
		Assert.Contains("http://localhost:5000", _viewModel.ErrorMessage);
		Assert.Equal(ValidEmail, _viewModel.Email);
	}

	[Theory]
	[InlineData(ApiError.Server)]
	[InlineData(ApiError.Rejected)]
	[InlineData(ApiError.Forbidden)]
	public async Task SignIn_WithAnUnexpectedError_ShowsAGenericMessage(ApiError error)
	{
		SetupLogin(ApiResult<AuthResponse>.Failure(error, "details from the server"));
		FillValidForm();

		await _viewModel.SignInCommand.ExecuteAsync(null);

		Assert.Equal("Something went wrong. Please try again.", _viewModel.ErrorMessage);
	}

	[Fact]
	public async Task SignIn_WhileTheRequestIsPending_IsBusyAndCannotBeSubmittedAgain()
	{
		var pending = new TaskCompletionSource<ApiResult<AuthResponse>>();
		_authService
			.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.Returns(pending.Task);
		FillValidForm();

		var signIn = _viewModel.SignInCommand.ExecuteAsync(null);

		Assert.True(_viewModel.IsBusy);
		Assert.False(_viewModel.SignInCommand.CanExecute(null));

		pending.SetResult(ApiResult<AuthResponse>.Failure(ApiError.Unauthorized));
		await signIn;

		Assert.False(_viewModel.IsBusy);
		Assert.True(_viewModel.SignInCommand.CanExecute(null));
	}

	[Fact]
	public async Task SignIn_ClearsErrorsFromThePreviousAttempt()
	{
		FillValidForm();
		_viewModel.Password = string.Empty;
		await _viewModel.SignInCommand.ExecuteAsync(null);
		SetupLogin(ApiResult<AuthResponse>.Success(CreateResponse()));
		_viewModel.Password = ValidPassword;

		await _viewModel.SignInCommand.ExecuteAsync(null);

		Assert.Empty(_viewModel.PasswordError);
	}

	[Fact]
	public void Appearing_LoadsTheSavedServerAddress()
	{
		_apiSettings.Object.BaseUrl = "http://192.168.1.20:5000";

		_viewModel.AppearingCommand.Execute(null);

		Assert.Equal("http://192.168.1.20:5000", _viewModel.ServerUrl);
	}

	[Fact]
	public void Appearing_ClearsThePasswordAndErrors()
	{
		_viewModel.Password = ValidPassword;
		_viewModel.ErrorMessage = "Invalid email or password.";
		_viewModel.EmailError = "Enter your email.";

		_viewModel.AppearingCommand.Execute(null);

		Assert.Empty(_viewModel.Password);
		Assert.Empty(_viewModel.ErrorMessage);
		Assert.Empty(_viewModel.EmailError);
	}

	[Fact]
	public void Appearing_AfterAnExpiredSession_ShowsTheExpiredMessage()
	{
		_sessionService.Setup(s => s.ConsumeEndReason()).Returns(SessionEndReason.Expired);

		_viewModel.AppearingCommand.Execute(null);

		Assert.Equal("Your session has expired. Please sign in again.", _viewModel.ErrorMessage);
	}

	[Fact]
	public void Appearing_AfterSigningOut_ShowsNoMessage()
	{
		_sessionService.Setup(s => s.ConsumeEndReason()).Returns(SessionEndReason.SignedOut);

		_viewModel.AppearingCommand.Execute(null);

		Assert.Empty(_viewModel.ErrorMessage);
	}

	private void FillValidForm()
	{
		_viewModel.Email = ValidEmail;
		_viewModel.Password = ValidPassword;
		_viewModel.ServerUrl = "http://localhost:5000";
	}

	private void SetupLogin(ApiResult<AuthResponse> result) =>
		_authService
			.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(result);

	private void VerifyNoRequestSent() =>
		_authService.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

	private static AuthResponse CreateResponse() =>
		new("token", DateTime.UtcNow.AddHours(1), new UserDto(Guid.NewGuid(), "Kasun Fernando", ValidEmail, "Agent"));
}
