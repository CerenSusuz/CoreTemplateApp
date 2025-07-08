using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public async IAsyncEnumerable<string> StreamPromptAsync(string prompt, AIRequestOptions? options = null)
    {
        var model = options?.Model ?? "mistral";

        var requestBody = new
        {
            model,
            prompt,
            stream = true
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/generate")
        {
            Content = content
        };

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            yield return $"[ERROR]: Ollama Streaming Error - {response.StatusCode}";
            yield break;
        }

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string? text = null;

            try
            {
                using var doc = JsonDocument.Parse(line);

                if (doc.RootElement.TryGetProperty("response", out var responseElement))
                {
                    text = responseElement.GetString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Streaming Parse Error] " + ex.Message);
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine("[Service Chunk] " + text);
                yield return text;
            }
        }

    }


    private class OllamaStreamChunk
    {
        public string? Response { get; set; }

        public bool Done { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraData { get; set; }
    }

}
