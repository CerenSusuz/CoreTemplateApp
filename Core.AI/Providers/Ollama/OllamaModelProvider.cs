using System.Text.Json;
using Core.AI.Abstractions;
using Core.AI.Config;

namespace Core.AI.Providers.Ollama;

/// <summary>
/// Provides a list of available Ollama models by querying the local Ollama instance.
/// </summary>
public class OllamaModelProvider : IAIModelProvider
{
    private readonly HttpClient _httpClient;
    private readonly OllamaSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaModelProvider"/> class.
    /// </summary>
    public OllamaModelProvider(OllamaSettings settings)
    {
        _settings = settings;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Fetches the list of available models from the Ollama server.
    /// </summary>
    /// <returns>A list of model names.</returns>
    public async Task<List<string>> GetAvailableModelsAsync()
    {
        var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/api/tags");
        var content = await response.Content.ReadAsStringAsync();

        var modelList = new List<string>();
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("models", out var models))
            {
                foreach (var m in models.EnumerateArray())
                {
                    var name = m.GetProperty("name").GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                        modelList.Add(name);
                }
            }
        }
        catch
        {
            // Optionally handle or log error
        }

        return modelList;
    }
}
