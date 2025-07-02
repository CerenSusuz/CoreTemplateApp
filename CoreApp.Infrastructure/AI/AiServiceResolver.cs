using CoreApp.Application.Common.Interfaces.AI;
using CoreApp.Application.Common.Models;
using CoreApp.Application.Common.Settings;

namespace CoreApp.Infrastructure.AI;

public class AiServiceResolver : IAIService
{
    private readonly IAIService _activeService;

    public AiServiceResolver(
        AISettings settings,
        OpenRouterAiService openAiService,
        OllamaAiService ollamaAiService)
    {
        _activeService = settings.Provider.ToLower() switch
        {
            "ollama" => ollamaAiService,
            _ => openAiService
        };
    }

    public Task<string> PromptAsync(string prompt, AiRequestOptions? options = null)
        => _activeService.PromptAsync(prompt, options);
}