using CoreApp.Application.DTOs.Auth;
using CoreApp.Application.Interfaces.Auth;

namespace CoreApp.WebAPI.Services
{
    public class MockAuthService : IAuthService
    {
        public Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            return Task.FromResult(new AuthResponse
            {
                Token = "mock-jwt-token",
                RefreshToken = "mock-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }

        public Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            return Task.FromResult(new AuthResponse
            {
                Token = "mock-jwt-token",
                RefreshToken = "mock-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }

        public Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            return Task.FromResult(new AuthResponse
            {
                Token = "refreshed-jwt-token",
                RefreshToken = "new-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }
    }
}
