using CoreApp.Application.Features.Profile.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreApp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProfileController(IMediator mediator) => _mediator = mediator;

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _mediator.Send(new GetProfileQuery());

            return Ok(result);
        }
    }
}
