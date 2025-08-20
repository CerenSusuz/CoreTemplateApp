using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.Models;

/// <summary>
/// Represents an agent profile that defines behavior, tone, and model settings for AI interactions.
/// </summary>
public class AgentProfile
{
    /// <summary>
    /// Name of the agent profile (e.g., "default", "expert").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// System context or instruction for the AI (e.g., personality or behavior guide).
    /// </summary>
    public string Context { get; set; } = "You are a helpful assistant.";

    /// <summary>
    /// Controls randomness of output (0 = deterministic, 1 = very random).
    /// </summary>
    public float Temperature { get; set; } = 0.7f;

    /// <summary>
    /// The AI model name to use (e.g., "gpt-3.5-turbo").
    /// </summary>
    public string Model { get; set; } = "gpt-3.5-turbo";
}