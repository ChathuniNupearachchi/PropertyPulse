namespace PropertyPulse.Shared.Auth;

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);
