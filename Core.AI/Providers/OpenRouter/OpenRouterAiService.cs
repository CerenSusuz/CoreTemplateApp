using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.FunctionCalling;
using Core.AI.FunctionCalling.FunctionSchema;
using Core.AI.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.AI.Providers.OpenRouter;

/// <summary>
/// AI service implementation for OpenRouter that supports completion, function calling, streaming, and model discovery.
/// </summary>
public class OpenRouterAiService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterSettings _settings;
    private readonly AISettings _aiSettings;
    private readonly AiFunctionDispatcher _dispatcher;
    private readonly IFunctionRegistry _registry;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenRouterAiService"/> class.
    /// </summary>
    public OpenRouterAiService(
        HttpClient httpClient,
        OpenRouterSettings settings,
        AISettings aiSettings,
        AiFunctionDispatcher dispatcher,
        IFunctionRegistry registry)
    {
        _httpClient = httpClient;
        _settings = settings;
        _aiSettings = aiSettings;
        _dispatcher = dispatcher;
        _registry = registry;
    }

    /// <inheritdoc />
    public async Task<string> PromptAsync(string prompt, AIRequestOptions? options = null)
    {
        var messages = new[] { new { role = "user", content = prompt } };
        var body = new
        {
            model = options?.Model ?? _aiSettings.Model,
            messages
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        request.Content = JsonContent.Create(body);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? string.Empty;
    }

    /// <inheritdoc />
    public async Task<string> GetCompletionAsync(string prompt, AIRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var messages = new[] { new { role = "user", content = prompt } };
        var tools = FunctionSchemaGenerator.ToOpenRouterTools(_registry.GetAll());

        var body = new
        {
            model = _aiSettings.Model,
            messages,
            tools,
            tool_choice = "auto"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        Console.WriteLine("OpenRouter Raw Response:");
        Console.WriteLine(responseJson);

        using var doc = JsonDocument.Parse(responseJson);
        var choices = doc.RootElement.GetProperty("choices");
        var message = choices[0].GetProperty("message");

        // Function call execution
        if (message.TryGetProperty("tool_calls", out var toolCalls))
        {
            foreach (var toolCall in toolCalls.EnumerateArray())
            {
                var fn = toolCall.GetProperty("function");
                var functionName = fn.GetProperty("name").GetString();
                var argsJson = fn.GetProperty("arguments").GetString();

                var argsDict = JsonDocument.Parse(argsJson!).RootElement;
                var args = new Dictionary<string, object>();

                foreach (var prop in argsDict.EnumerateObject())
                {
                    args[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString()!,
                        JsonValueKind.Number => prop.Value.TryGetInt32(out var i) ? i : prop.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => prop.Value.GetRawText()
                    };
                }

                var result = await _dispatcher.TryDispatchAsync(functionName!, args, cancellationToken);

                using var resultDoc = JsonDocument.Parse(result.Result);
                var parsedResult = resultDoc.RootElement.Clone();

                var aiResponse = new AIResponse
                {
                    Content = $"Function `{functionName}` executed successfully.",
                    FunctionExecuted = true,
                    FunctionName = functionName,
                    FunctionResult = parsedResult
                };

                return JsonSerializer.Serialize(aiResponse, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        // Plain text response
        var plainContent = message.GetProperty("content").GetString();
        return JsonSerializer.Serialize(new AIResponse
        {
            Content = plainContent,
            FunctionExecuted = false
        });
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> StreamPromptAsync(
        string prompt,
        AIRequestOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = new[] { new { role = "user", content = prompt } };
        var body = new
        {
            model = options?.Model ?? _aiSettings.Model,
            messages,
            stream = true
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

            var jsonLine = line["data:".Length..].Trim();
            if (jsonLine == "[DONE]") yield break;

            string? chunk = null;

            try
            {
                using var doc = JsonDocument.Parse(jsonLine);
                chunk = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("delta")
                    .GetProperty("content")
                    .GetString();
            }
            catch
            {
               
            }

            if (!string.IsNullOrWhiteSpace(chunk))
                yield return chunk!;
        }

    }

    /// <inheritdoc />
    public async Task<bool> IsModelSupportedAsync(string model)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://openrouter.ai/api/v1/models");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return false;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        foreach (var m in doc.RootElement.EnumerateArray())
        {
            if (m.TryGetProperty("id", out var idProp))
            {
                var id = idProp.GetString();
                if (string.Equals(id, model, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }
}
