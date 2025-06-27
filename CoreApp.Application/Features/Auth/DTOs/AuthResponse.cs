namespace CoreApp.Application.Features.Auth.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}
