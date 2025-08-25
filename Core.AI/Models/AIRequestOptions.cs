using Core.AI.Config;
using System.Text.Json.Serialization;

namespace Core.AI.Models;

/// <summary>
/// Represents common configuration options for AI prompt requests.
/// </summary>
public class AIRequestOptions
{
    /// <summary>
    /// Optional system context instruction (e.g., personality or behavior prompt).
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Optional metadata indicating the purpose of the request.
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Optional language specification (e.g., "en", "tr").
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Optional maximum token limit for the AI response.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Controls randomness of the AI response. Values closer to 1 produce more variation.
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// The AI model name to use (e.g., "gpt-4", "llama2").
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// AI provider to use (e.g., OpenRouter, Ollama).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AIProvider? Provider { get; set; }

    public bool UseFunctionCalling { get; set; } = true;

    public string? SystemPrompt { get; set; }
}