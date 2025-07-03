using Core.AI.Abstractions;
using Core.AI.Models;
using Microsoft.Extensions.Configuration;

namespace Core.AI.Providers;

public class AIServiceResolver : IAIService
{
    private readonly IAIService _activeService;

    public AIServiceResolver(
        IConfiguration config,
        OpenRouterAiService openRouterService,
        OllamaAiService ollamaService)
    {
        var provider = config["AiSettings:Provider"]?.ToLower();

        _activeService = provider switch
        {
            "ollama" => ollamaService,
            _ => openRouterService
        };
    }

    public Task<string> PromptAsync(string prompt, AIRequestOptions? options = null)
        => _activeService.PromptAsync(prompt, options);
}
