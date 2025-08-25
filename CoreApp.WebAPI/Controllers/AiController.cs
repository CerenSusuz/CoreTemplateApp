using Core.AI.Abstractions;
using Core.AI.Commands;
using Core.AI.Config;
using Core.AI.Models;
using Core.AI.Models.Agent;
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
    private readonly IAgentService _agentService;

    public AiController(IMediator mediator, IAIService aiService, IAgentService agentService)
    {
        _mediator = mediator;
        _aiService = aiService;
        _agentService = agentService;
    }

    [HttpPost("prompt")]
    public async Task<IActionResult> Prompt([FromBody] PromptTextCommand command)
        => Ok(new { result = await _mediator.Send(command) });

    [HttpPost("completion")]
    public async Task<IActionResult> Completion([FromBody] PromptTextCommand command)
        => Ok(new { result = await _aiService.GetCompletionAsync(command.Prompt) });

    [HttpGet("model-supported")]
    public async Task<IActionResult> IsModelSupported([FromQuery] string model)
        => Ok(new { model, isSupported = await _aiService.IsModelSupportedAsync(model) });

    [HttpPost("stream")]
    public async Task StreamPrompt([FromBody] PromptTextCommand command)
    {
        Response.ContentType = "text/plain";
        await foreach (var chunk in _aiService.StreamPromptAsync(command.Prompt, command.Options))
        {
            var buffer = Encoding.UTF8.GetBytes(chunk);
            await Response.Body.WriteAsync(buffer);
            await Response.Body.FlushAsync();
        }
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] AiChatRequest request)
    {
        var opts = new AgentRequestOptions
        {
            AgentId = request.AgentId,
            Profile = request.AgentId, // Id/Name fark etmez
            Model = request.Model,
            UseFunctionCalling = request.UseFunctionCalling,
            Provider = Enum.TryParse<AIProvider>(request.Provider, out var p) ? p : null
        };

        var response = await _agentService.ChatAsync(request.Prompt, opts);
        return Ok(new { Content = response });
    }

    public class AiChatRequest
    {
        public string Prompt { get; set; } = "";
        public string Provider { get; set; } = "OpenRouter";
        public string Model { get; set; } = "gpt-3.5-turbo";
        public bool UseFunctionCalling { get; set; } = true;
        public string? AgentId { get; set; }
    }
}
