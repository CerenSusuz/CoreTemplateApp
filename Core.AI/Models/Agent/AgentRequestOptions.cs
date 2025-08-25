namespace Core.AI.Models.Agent;

/// <summary>
/// Extended options for agent requests, including the use of named profiles.
/// </summary>
public class AgentRequestOptions : AIRequestOptions
{
    /// <summary>
    /// Optional agent profile name to override other options.
    /// </summary>
    public string? Profile { get; set; }

    public string? AgentId { get; set; }
}