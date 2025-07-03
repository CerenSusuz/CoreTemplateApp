using System.Text;
using System.Text.Json;
using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.Models;

namespace Core.AI.Providers.Ollama;

public class OllamaAiService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly AIModelProviderResolver _modelResolver;

    public OllamaAiService(AIModelProviderResolver modelResolver)
    {
        _modelResolver = modelResolver;
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434/") };
    }

    public async Task<string> PromptAsync(string prompt, AIRequestOptions? options = null)
    {
        var model = options?.Model ?? "mistral";

        var requestBody = new
        {
            model,
            prompt,
            stream = false
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/generate", content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return $"[ERROR]: Ollama API Error - {response.StatusCode}";

        try
        {
            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("response").GetString() ?? string.Empty;
        }
        catch
        {
            return "[Ollama response parse error]";
        }
    }

    public async Task<bool> IsModelSupportedAsync(string model)
    {
        var provider = _modelResolver.Resolve(AIProvider.Ollama);
        var models = await provider.GetAvailableModelsAsync();
        return models.Contains(model);
    }
}
