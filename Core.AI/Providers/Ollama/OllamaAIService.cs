using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.FunctionCalling;
using Core.AI.FunctionCalling.FunctionSchema;
using Core.AI.Models;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Core.AI.Providers.Ollama;

/// <summary>
/// Provides AI chat capabilities using the Ollama API, including prompt completion, streaming, and function calling.
/// </summary>
public class OllamaAiService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;
    private readonly OllamaSettings _ollama;
    private readonly AiFunctionDispatcher _dispatcher;
    private readonly IFunctionRegistry _registry;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaAiService"/> class.
    /// </summary>
    public OllamaAiService(
        HttpClient httpClient,
        AISettings settings,
        OllamaSettings ollama,
        AiFunctionDispatcher dispatcher,
        IFunctionRegistry registry)
    {
        _httpClient = httpClient;
        _settings = settings;
        _ollama = ollama;
        _dispatcher = dispatcher;
        _registry = registry;
    }

    /// <inheritdoc />
    public async Task<string> GetCompletionAsync(string prompt, AIRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var messages = new[] { new { role = "user", content = prompt } };
        var tools = FunctionSchemaGenerator.ToOllamaSchemas(_registry.GetAll());

        var requestBody = new { model = _settings.Model, messages, tools };

        var response = await _httpClient.PostAsJsonAsync($"{_ollama.BaseUrl}/api/chat", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Handle function call
        if (root.TryGetProperty("message", out var message) &&
            message.TryGetProperty("tool_calls", out var toolCalls))
        {
            foreach (var toolCall in toolCalls.EnumerateArray())
            {
                var fn = toolCall.GetProperty("function");
                var functionName = fn.GetProperty("name").GetString();
                var argsJson = fn.GetProperty("arguments").GetRawText();

                var args = JsonSerializer.Deserialize<Dictionary<string, object>>(argsJson)
                          ?? new Dictionary<string, object>();

                var result = await _dispatcher.TryDispatchAsync(functionName!, args, cancellationToken);

                var aiResponse = new AIResponse
                {
                    Content = $"Function {functionName} executed",
                    FunctionExecuted = true,
                    FunctionName = functionName,
                    FunctionResult = result.Result
                };
                return JsonSerializer.Serialize(aiResponse);
            }
        }

        // Handle plain message
        if (root.TryGetProperty("message", out var msg) &&
            msg.TryGetProperty("content", out var content))
        {
            var aiResponse = new AIResponse
            {
                Content = content.GetString(),
                FunctionExecuted = false
            };
            return JsonSerializer.Serialize(aiResponse);
        }

        return JsonSerializer.Serialize(new AIResponse
        {
            Content = "[No response from Ollama]",
            FunctionExecuted = false
        });
    }

    /// <inheritdoc />
    public async Task<string> PromptAsync(string prompt, AIRequestOptions? options = null)
    {
        var body = new
        {
            model = options?.Model ?? _settings.Model,
            prompt,
            stream = false
        };

        var res = await _httpClient.PostAsJsonAsync($"{_ollama.BaseUrl}/api/generate", body);
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("response").GetString() ?? string.Empty;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> StreamPromptAsync(
        string prompt,
        AIRequestOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var body = new { model = options?.Model ?? _settings.Model, prompt, stream = true };

        using var res = await _httpClient.PostAsJsonAsync($"{_ollama.BaseUrl}/api/generate", body, cancellationToken);
        res.EnsureSuccessStatusCode();

        using var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string? value = null;
            try
            {
                using var doc = JsonDocument.Parse(line);
                if (doc.RootElement.TryGetProperty("response", out var resp))
                    value = resp.GetString();
            }
            catch { }

            if (!string.IsNullOrEmpty(value))
                yield return value;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsModelSupportedAsync(string model)
    {
        var res = await _httpClient.GetAsync($"{_ollama.BaseUrl}/api/tags");
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("models", out var models))
        {
            foreach (var m in models.EnumerateArray())
            {
                var name = m.GetProperty("name").GetString();
                if (string.Equals(name, model, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }
}
