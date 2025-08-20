using System.Net.Http.Headers;
using System.Text.Json;
using Core.AI.Abstractions;
using Core.AI.Config;

namespace Core.AI.Providers.OpenRouter;

/// <summary>
/// Fetches available model IDs from OpenRouter's model registry API.
/// </summary>
public class OpenRouterModelProvider : IAIModelProvider
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenRouterModelProvider"/> class.
    /// </summary>
    public OpenRouterModelProvider(OpenRouterSettings settings)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://openrouter.ai/api/v1/")
        };
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", settings.ApiKey);
    }

    /// <summary>
    /// Retrieves the list of available model IDs from OpenRouter.
    /// </summary>
    /// <returns>List of model names.</returns>
    public async Task<List<string>> GetAvailableModelsAsync()
    {
        var response = await _httpClient.GetAsync("models");
        var content = await response.Content.ReadAsStringAsync();

        var list = new List<string>();
        try
        {
            using var doc = JsonDocument.Parse(content);
            var models = doc.RootElement.GetProperty("data");
            foreach (var m in models.EnumerateArray())
            {
                var id = m.GetProperty("id").GetString();
                if (!string.IsNullOrWhiteSpace(id)) list.Add(id);
            }
        }
        catch
        {
            // Optionally log or handle error
        }

        return list;
    }
}
