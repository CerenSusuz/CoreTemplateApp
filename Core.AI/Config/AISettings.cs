namespace Core.AI.Config;

/// <summary>
/// Global configuration for AI settings.
/// </summary>
public class AISettings
{
    public AIProvider Provider { get; set; }
    public string Model { get; set; } = string.Empty;
}
