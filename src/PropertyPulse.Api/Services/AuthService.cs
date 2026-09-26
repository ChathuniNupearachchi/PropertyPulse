using Microsoft.AspNetCore.Identity;
using PropertyPulse.Api.Mappings;
using PropertyPulse.Domain.Entities;
using PropertyPulse.Domain.Enums;
using PropertyPulse.Domain.Exceptions;
using PropertyPulse.Infrastructure.Repositories;
using PropertyPulse.Shared.Auth;

namespace PropertyPulse.Api.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    IJwtTokenService jwtTokenService,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);

        if (await userRepository.FindByEmailAsync(email, cancellationToken) is not null)
        {
            throw new EmailAlreadyInUseException();
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Role = UserRole.Agent
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        await userRepository.AddAsync(user, cancellationToken);

        return CreateResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByEmailAsync(NormalizeEmail(request.Email), cancellationToken);

        if (user is null ||
            passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            // The submitted email is left out on purpose so the log does not list probed accounts.
            logger.LogWarning("Login failed");
            throw new InvalidCredentialsException();
        }

        return CreateResponse(user);
    }

    private AuthResponse CreateResponse(User user)
    {
        var token = jwtTokenService.CreateToken(user);
        return new AuthResponse(token.Value, token.ExpiresAt, user.ToDto());
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
