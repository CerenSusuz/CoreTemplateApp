using CoreApp.Application.Common.Interfaces.Auth;
using CoreApp.Application.Features.Auth.DTOs;
using MediatR;

namespace CoreApp.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
    {
        private readonly IAuthService _authService;

        public LoginCommandHandler(IAuthService authService) => _authService = authService;

        public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            return await _authService.LoginAsync(request.Request);
        }
    }
}
