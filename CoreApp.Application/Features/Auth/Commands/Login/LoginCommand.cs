using CoreApp.Application.Features.Auth.DTOs;
using MediatR;

namespace CoreApp.Application.Features.Auth.Commands.Login;

public record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;
