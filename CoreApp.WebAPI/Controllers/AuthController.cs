using CoreApp.Application.Features.Auth.Commands.Login;
using CoreApp.Application.Features.Auth.Commands.Register;
using CoreApp.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoreApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterCommand(request));

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _mediator.Send(new LoginCommand(request));

        return Ok(result);
    }
}
