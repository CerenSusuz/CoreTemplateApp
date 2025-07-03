using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Core.AI.Abstractions;
using Core.AI.Models;
using Microsoft.Extensions.Configuration;
using Core.AI.Config;
using Microsoft.Extensions.Options;

namespace Core.AI.Providers;

public class OpenRouterAiService : IAIService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;

    public OpenRouterAiService(IConfiguration config, IOptions<AISettings> settings)
    {
        _apiKey = config["OpenAI:ApiKey"]!;
        _settings = settings.Value;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://openrouter.ai/api/v1/")
        };
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> PromptAsync(string prompt, AIRequestOptions? options = null)
    {
        var requestBody = new
        {
            model = _settings.Model,
            messages = new[]
            {
                new { role = "system", content = options?.Context ?? "You are a helpful assistant." },
                new { role = "user", content = prompt }
            },
            temperature = options?.Temperature ?? 0.7f,
            max_tokens = options?.MaxTokens ?? 500
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("chat/completions", content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return $"[ERROR]: API Error - {response.StatusCode}";

        using var doc = JsonDocument.Parse(responseString);
        var contentElement = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content");

        return contentElement.GetString() ?? "[AI response missing]";
    }
}
