using CoreApp.Application.Common.Interfaces.AI;
using CoreApp.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using CoreApp.Application.Common.Settings;
using System.Text.Json;
using System.Text;

namespace CoreApp.Infrastructure.AI;

public class OpenRouterAiService : IAIService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;

    public OpenRouterAiService(IConfiguration config, AISettings settings)
    {
        _settings = settings;
        _apiKey = config["OpenAI:ApiKey"]!;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://openrouter.ai/api/v1/")
        };
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> PromptAsync(string prompt, AiRequestOptions? options = null)
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

        try
        {
            using var doc = JsonDocument.Parse(responseString);
            var contentElement = doc.RootElement.GetProperty("choices")[0]
                .GetProperty("message").GetProperty("content");
            return contentElement.GetString() ?? string.Empty;
        }
        catch
        {
            return "[AI response error]";
        }
    }
}