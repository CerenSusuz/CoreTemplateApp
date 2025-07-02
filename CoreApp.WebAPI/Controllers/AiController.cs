using CoreApp.Application.Features.AI.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoreApp.WebAPI.Controllers
{
    [Route("api/ai")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AiController(IMediator mediator) => _mediator = mediator;

        [HttpPost("prompt")]
        public async Task<IActionResult> Prompt([FromBody] PromptTextCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(new { result });
        }
    }
}
