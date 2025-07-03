using System.Net.Http.Headers;
using System.Text.Json;
using Core.AI.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Core.AI.Providers.OpenRouter;

public class OpenRouterModelProvider : IAIModelProvider
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public OpenRouterModelProvider(IConfiguration config)
    {
        _apiKey = config["OpenAI:ApiKey"]!;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://openrouter.ai/api/v1/")
        };
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<List<string>> GetAvailableModelsAsync()
    {
        var response = await _httpClient.GetAsync("models");
        var content = await response.Content.ReadAsStringAsync();

        var modelList = new List<string>();
        try
        {
            using var doc = JsonDocument.Parse(content);
            var models = doc.RootElement.GetProperty("data");

            foreach (var model in models.EnumerateArray())
            {
                var id = model.GetProperty("id").GetString();
                if (!string.IsNullOrWhiteSpace(id))
                    modelList.Add(id);
            }
        }
        catch
        {
            // optionally log
        }

        return modelList;
    }
}
