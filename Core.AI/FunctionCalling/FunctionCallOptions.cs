namespace Core.AI.FunctionCalling;

/// <summary>
/// Represents options for a function call.
/// </summary>
public class FunctionCallOptions
{
    public string? Model { get; set; }
    public string? Provider { get; set; }
}