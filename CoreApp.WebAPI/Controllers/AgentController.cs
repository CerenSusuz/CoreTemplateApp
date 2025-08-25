using Core.AI.Abstractions;
using Core.AI.Models.Agent;
using Core.AI.Providers.Profiles;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CoreApp.WebAPI.Controllers;

[Route("api/agent")]
[ApiController]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;
    private readonly AgentProfileProvider _profiles;

    public AgentController(IAgentService agentService, AgentProfileProvider profiles)
    {
        _agentService = agentService;
        _profiles = profiles;
    }

    [HttpGet("profiles")]
    public IActionResult GetProfiles()
    {
        var list = _profiles.GetAllProfiles().Select(p => new { p.Id, p.Name, p.Description });
        return Ok(list);
    }

    [HttpPost("prompt")]
    public async Task<IActionResult> Prompt([FromBody] AgentPromptRequest request)
    {
        var result = await _agentService.ChatAsync(request.Prompt, request.Options, request.UserId);
        return Ok(new { result });
    }

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
