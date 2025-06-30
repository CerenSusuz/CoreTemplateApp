namespace CoreApp.Application.Common.Settings
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;

        public string Issuer { get; set; } = "CoreApp";

        public string Audience { get; set; } = "CoreAppAPI";

        public int ExpirationMinutes { get; set; } = 60;
    }
}
