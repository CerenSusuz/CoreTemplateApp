using CoreApp.Application.DTOs.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp.Application.Features.Auth.Commands.Login
{
    public record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;
}
