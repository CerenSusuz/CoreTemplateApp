using System.Text.Json;
using Core.AI.Abstractions;

namespace Core.AI.Providers.Ollama;

public class OllamaModelProvider : IAIModelProvider
{
    private readonly HttpClient _httpClient;

    public OllamaModelProvider()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434/") };
    }

    public async Task<List<string>> GetAvailableModelsAsync()
    {
        var response = await _httpClient.GetAsync("api/tags");
        var content = await response.Content.ReadAsStringAsync();

        var modelList = new List<string>();
        try
        {
            using var doc = JsonDocument.Parse(content);
            var models = doc.RootElement.GetProperty("models");

            foreach (var model in models.EnumerateArray())
            {
                var name = model.GetProperty("name").GetString();
                if (!string.IsNullOrWhiteSpace(name))
                    modelList.Add(name);
            }
        }
        catch
        {
            // optionally log
        }

        return modelList;
    }
}
