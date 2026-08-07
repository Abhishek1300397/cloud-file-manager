namespace CloudStorage.Application.Configuration
{
    public class JwtOptions
    {
        public static readonly string SectionName = "Jwt";

        public string SecretKey { get; init; } = string.Empty;

        public string Issuer { get; init; } = string.Empty;

        public string Audience { get; init; } = string.Empty;

        public int ExpirationMinutes { get; init; }
    }
}
