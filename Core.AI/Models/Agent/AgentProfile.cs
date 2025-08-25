using System.Text.Json.Serialization;

namespace Core.AI.Models.Agent;

/// <summary>
/// Represents an agent profile that defines behavior, tone, and model settings for AI interactions.
/// </summary>
public class AgentProfile
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string SystemPrompt { get; set; } = string.Empty;
    public float Temperature { get; set; } = 0.7f;
    [JsonIgnore] public string? Context => SystemPrompt;
}