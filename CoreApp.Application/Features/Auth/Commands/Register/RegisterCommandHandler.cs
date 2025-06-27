using CoreApp.Application.Common.Interfaces.Auth;
using CoreApp.Application.Features.Auth.DTOs;
using MediatR;

namespace CoreApp.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RegisterAsync(request.Request);
    }
}
