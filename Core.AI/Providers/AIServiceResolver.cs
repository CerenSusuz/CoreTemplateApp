using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.Models;
using Core.AI.Providers.Ollama;
using Core.AI.Providers.OpenRouter;
using Microsoft.Extensions.Options;

namespace Core.AI.Providers;

public class AIServiceResolver : IAIService
{
    private readonly IAIService _activeService;

    public AIServiceResolver(
        IOptions<AISettings> settings,
        OpenRouterAiService openRouterService,
        OllamaAiService ollamaService)
    {
        _activeService = settings.Value.Provider switch
        {
            AIProvider.Ollama => ollamaService,
            _ => openRouterService
        };
    }

    public Task<string> PromptAsync(string prompt, AIRequestOptions? options = null)
        => _activeService.PromptAsync(prompt, options);

    public Task<bool> IsModelSupportedAsync(string model)
        => _activeService.IsModelSupportedAsync(model);
}
