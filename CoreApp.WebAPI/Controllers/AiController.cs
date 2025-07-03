using Core.AI.Abstractions;
using Core.AI.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoreApp.WebAPI.Controllers;

[Route("api/ai")]
[ApiController]
public class AiController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAIService _aiService;

    public AiController(IMediator mediator, IAIService aiService)
    {
        _mediator = mediator;
        _aiService = aiService;
    }

    // POST: /api/ai/prompt
    [HttpPost("prompt")]
    public async Task<IActionResult> Prompt([FromBody] PromptTextCommand command)
    {
        var result = await _mediator.Send(command);

        return Ok(new { result });
    }

    // GET: /api/ai/model-supported?model=anthropic/claude-3-haiku
    [HttpGet("model-supported")]
    public async Task<IActionResult> IsModelSupported([FromQuery] string model)
    {
        var isSupported = await _aiService.IsModelSupportedAsync(model);

        return Ok(new { model, isSupported });
    }
}
