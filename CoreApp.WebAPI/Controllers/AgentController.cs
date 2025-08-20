using Azure;
using Core.AI.Abstractions;
using Core.AI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CoreApp.WebAPI.Controllers;

[Route("api/agent")]
[ApiController]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;

    public AgentController(IAgentService agentService)
    {
        _agentService = agentService;
    }

    // POST /api/agent/prompt
    [HttpPost("prompt")]
    public async Task<IActionResult> Prompt([FromBody] AgentPromptRequest request)
    {
        var result = await _agentService.ChatAsync(request.Prompt, request.Options, request.UserId);
        return Ok(new { result });
    }

    // POST /api/agent/stream
    [HttpPost("stream")]
    public async Task StreamPrompt([FromBody] AgentPromptRequest request)
    {
        Response.ContentType = "text/plain";

        await foreach (var chunk in _agentService.StreamChatAsync(request.Prompt, request.Options, request.UserId))
        {
            var buffer = Encoding.UTF8.GetBytes(chunk);
            await Response.Body.WriteAsync(buffer);
            await Response.Body.FlushAsync();
        }
    }
}