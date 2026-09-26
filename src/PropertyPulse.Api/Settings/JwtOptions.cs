namespace PropertyPulse.Api.Settings;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinimumSigningKeyLength = 32;

    public string Issuer { get; set; } = "PropertyPulse";
    public string Audience { get; set; } = "PropertyPulse";
    public string SigningKey { get; set; } = string.Empty;
    public int LifetimeMinutes { get; set; } = 60;
}
