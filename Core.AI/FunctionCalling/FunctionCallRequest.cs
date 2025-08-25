namespace Core.AI.FunctionCalling;

/// <summary>
/// Represents the function call request.
/// </summary>
public class FunctionCallRequest
{
    public string Prompt { get; set; } = string.Empty;
    public FunctionCallOptions? Options { get; set; }
}
