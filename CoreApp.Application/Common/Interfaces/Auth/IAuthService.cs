using CoreApp.Application.Features.Auth.DTOs;

namespace CoreApp.Application.Common.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        Task<AuthResponse> LoginAsync(LoginRequest request);

        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    }
}
