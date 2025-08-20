using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.Models;

/// <summary>
/// Represents a request to prompt an agent with optional settings and user context.
/// </summary>
public class AgentPromptRequest
{
    /// <summary>
    /// The actual prompt message from the user.
    /// </summary>
    public string Prompt { get; set; } = "";

    /// <summary>
    /// Optional request configuration (e.g., model, temperature, context).
    /// </summary>
    public AgentRequestOptions? Options { get; set; }

    /// <summary>
    /// Optional identifier for the user (used for chat memory/history).
    /// </summary>
    public string? UserId { get; set; }
}