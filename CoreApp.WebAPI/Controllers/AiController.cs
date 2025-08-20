using Core.AI.Abstractions;
using Core.AI.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text;

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

    // POST: /api/ai/completion
    [HttpPost("completion")]
    public async Task<IActionResult> Completion([FromBody] PromptTextCommand command)
    {
        var result = await _aiService.GetCompletionAsync(command.Prompt);
        return Ok(new { result });
    }

    // GET: /api/ai/model-supported?model=llama3
    [HttpGet("model-supported")]
    public async Task<IActionResult> IsModelSupported([FromQuery] string model)
    {
        var isSupported = await _aiService.IsModelSupportedAsync(model);
        return Ok(new { model, isSupported });
    }

    // POST: /api/ai/stream
    [HttpPost("stream")]
    public async Task StreamPrompt([FromBody] PromptTextCommand command)
    {
        Response.ContentType = "text/plain";
        Console.WriteLine("[Streaming] Started...");

        await foreach (var chunk in _aiService.StreamPromptAsync(command.Prompt, command.Options))
        {
            Console.WriteLine($"[Chunk] {chunk}");
            var buffer = Encoding.UTF8.GetBytes(chunk);
            await Response.Body.WriteAsync(buffer);
            await Response.Body.FlushAsync();
        }

        Console.WriteLine("[Streaming] Ended.");
    }
}
