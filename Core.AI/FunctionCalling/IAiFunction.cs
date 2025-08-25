namespace Core.AI.FunctionCalling;

/// <summary>
/// Interface for describing a callable AI function.
/// </summary>
public interface IAiFunction
{
    string Name { get; }

    string Description { get; }

    Task<FunctionCallResult> InvokeAsync(string argumentsJson, CancellationToken cancellationToken);
}