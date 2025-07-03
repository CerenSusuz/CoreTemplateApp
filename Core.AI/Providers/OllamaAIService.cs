using System.Text;
using System.Text.Json;
using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.Models;
using Microsoft.Extensions.Options;

namespace Core.AI.Providers;

public class OllamaAiService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;

    public OllamaAiService(IOptions<AISettings> settings)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434/") };
        _settings = settings.Value;
    }

    public async Task<string> PromptAsync(string prompt, AIRequestOptions? options = null)
    {
        var requestBody = new
        {
            model = _settings.Model,
            prompt = prompt,
            stream = false
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/generate", content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return $"[ERROR]: Ollama API Error - {response.StatusCode}";

        using var doc = JsonDocument.Parse(responseString);

        return doc.RootElement.GetProperty("response").GetString() ?? "[Ollama response missing]";
    }
}

