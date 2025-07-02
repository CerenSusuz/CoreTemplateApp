namespace CoreApp.Application.Common.Models;

public class AiRequestOptions
{
    public string? Context { get; set; }

    public string? Purpose { get; set; }

    public string? Language { get; set; }

    public int? MaxTokens { get; set; }

    public float? Temperature { get; set; }
}
