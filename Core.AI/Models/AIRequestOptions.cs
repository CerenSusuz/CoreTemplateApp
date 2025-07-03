namespace Core.AI.Models;

public class AIRequestOptions
{
    public string? Context { get; set; }
    public string? Purpose { get; set; }
    public string? Language { get; set; }
    public int? MaxTokens { get; set; }
    public float? Temperature { get; set; }
    public string? Model { get; set; }
}
