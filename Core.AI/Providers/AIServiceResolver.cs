using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.Models;
using Core.AI.Providers.Ollama;
using Core.AI.Providers.OpenRouter;
using Microsoft.Extensions.Options;

namespace Core.AI.Providers;

public class AIServiceResolver : IAIService
{
    private readonly AISettings _settings;
    private readonly OpenRouterAiService _openRouter;
    private readonly OllamaAiService _ollama;

    public AIServiceResolver(IOptions<AISettings> settings, OpenRouterAiService open, OllamaAiService ollama)
    {
        _settings = settings.Value;
        _openRouter = open;
        _ollama = ollama;
    }

    private IAIService Resolve(AIProvider? overrideProvider)
    {
        return overrideProvider switch
        {
            AIProvider.Ollama => _ollama,
            AIProvider.OpenRouter => _openRouter,
            null => _settings.Provider == AIProvider.Ollama ? _ollama : _openRouter
        };
    }

    public Task<string> PromptAsync(string prompt, AIRequestOptions? options = null)
        => Resolve(options?.Provider).PromptAsync(prompt, options);

    public Task<bool> IsModelSupportedAsync(string model)
        => Resolve(null).IsModelSupportedAsync(model);

    public IAsyncEnumerable<string> StreamPromptAsync(string prompt, AIRequestOptions? options = null)
    => Resolve(options?.Provider).StreamPromptAsync(prompt, options);
}
