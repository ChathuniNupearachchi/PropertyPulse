using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PropertyPulse.Api.Settings;
using PropertyPulse.Api.Services;
using PropertyPulse.Domain.Entities;
using PropertyPulse.Domain.Enums;
using PropertyPulse.Domain.Exceptions;
using PropertyPulse.Infrastructure.Repositories;
using PropertyPulse.Shared.Auth;

namespace PropertyPulse.Api.Tests.Services;

public class AuthServiceTests
{
    private const string Password = "Password123!";

    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly PasswordHasher<User> _passwordHasher = new();
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var jwtOptions = Options.Create(new JwtOptions { SigningKey = new string('k', 48) });

        _authService = new AuthService(
            _userRepository.Object,
            _passwordHasher,
            new JwtTokenService(jwtOptions),
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task Register_WithValidRequest_CreatesAgentWithNormalizedEmail()
    {
        User? savedUser = null;
        _userRepository
            .Setup(r => r.FindByEmailAsync("new.agent@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepository
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => savedUser = user)
            .Returns(Task.CompletedTask);

        var response = await _authService.RegisterAsync(new RegisterRequest("  New Agent ", " New.Agent@Example.com ", Password));

        Assert.NotNull(savedUser);
        Assert.Equal("new.agent@example.com", savedUser.Email);
        Assert.Equal("New Agent", savedUser.FullName);
        Assert.Equal(UserRole.Agent, savedUser.Role);
        Assert.Equal("Agent", response.User.Role);
        Assert.False(string.IsNullOrEmpty(response.Token));
    }

    [Fact]
    public async Task Register_StoresHashInsteadOfPassword_AndSaltsEachHash()
    {
        var savedUsers = new List<User>();
        _userRepository
            .Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepository
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => savedUsers.Add(user))
            .Returns(Task.CompletedTask);

        await _authService.RegisterAsync(new RegisterRequest("First", "first@example.com", Password));
        await _authService.RegisterAsync(new RegisterRequest("Second", "second@example.com", Password));

        Assert.All(savedUsers, user => Assert.NotEqual(Password, user.PasswordHash));
        Assert.NotEqual(savedUsers[0].PasswordHash, savedUsers[1].PasswordHash);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ThrowsAndDoesNotSave()
    {
        _userRepository
            .Setup(r => r.FindByEmailAsync("taken@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUser("taken@example.com"));

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(
            () => _authService.RegisterAsync(new RegisterRequest("Someone", "Taken@Example.com", Password)));

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsTokenAndUser()
    {
        var user = CreateUser("agent@example.com");
        _userRepository
            .Setup(r => r.FindByEmailAsync("agent@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var response = await _authService.LoginAsync(new LoginRequest(" Agent@Example.com ", Password));

        Assert.False(string.IsNullOrEmpty(response.Token));
        Assert.Equal(user.Id, response.User.Id);
        Assert.True(response.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithWrongPasswordOrUnknownEmail_ThrowsSameError()
    {
        _userRepository
            .Setup(r => r.FindByEmailAsync("agent@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUser("agent@example.com"));
        _userRepository
            .Setup(r => r.FindByEmailAsync("nobody@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var wrongPassword = await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _authService.LoginAsync(new LoginRequest("agent@example.com", "WrongPassword1")));
        var unknownEmail = await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _authService.LoginAsync(new LoginRequest("nobody@example.com", Password)));

        Assert.Equal(wrongPassword.Message, unknownEmail.Message);
    }

    private User CreateUser(string email)
    {
        var user = new User { FullName = "Test Agent", Email = email, Role = UserRole.Agent };
        user.PasswordHash = _passwordHasher.HashPassword(user, Password);
        return user;
    }
}
