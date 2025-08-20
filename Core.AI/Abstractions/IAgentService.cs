using Core.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.Abstractions;

/// <summary>
/// Represents an AI agent capable of handling chat-based interactions and model validation.
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Checks if the given model is supported by the specified provider.
    /// </summary>
    Task<bool> IsModelSupportedAsync(string model, string provider = "OpenRouter");


    /// <summary>
    /// Sends a single prompt to the AI agent and returns the response.
    /// </summary>
    Task<string> ChatAsync(string prompt, AgentRequestOptions? options = null, string? userId = null);


    /// <summary>
    /// Streams the AI agent's response for a given prompt.
    /// </summary>
    IAsyncEnumerable<string> StreamChatAsync(string prompt, AgentRequestOptions? options = null, string? userId = null);
}
