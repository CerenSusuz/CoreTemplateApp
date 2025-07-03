using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.Models;
using Microsoft.Extensions.Configuration;

namespace Core.AI.Providers.OpenRouter;

public class OpenRouterAiService : IAIService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly AIModelProviderResolver _modelResolver;

    public OpenRouterAiService(
        AIModelProviderResolver modelResolver,
        IConfiguration config)
    {
        _apiKey = config["OpenAI:ApiKey"]!;
        _modelResolver = modelResolver;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://openrouter.ai/api/v1/")
        };

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> PromptAsync(string prompt, AIRequestOptions? options = null)
    {
        var model = options?.Model ?? "mistralai/mistral-7b-instruct";

        var requestBody = new
        {
            model,
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
            return $"[ERROR]: OpenRouter API Error - {response.StatusCode}";

        try
        {
            using var doc = JsonDocument.Parse(responseString);
            var choices = doc.RootElement.GetProperty("choices");
            return choices[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
        }
        catch
        {
            return "[OpenRouter response parse error]";
        }
    }

    public async Task<bool> IsModelSupportedAsync(string model)
    {
        var provider = _modelResolver.Resolve(AIProvider.OpenRouter);
        var models = await provider.GetAvailableModelsAsync();
        return models.Contains(model);
    }
}
