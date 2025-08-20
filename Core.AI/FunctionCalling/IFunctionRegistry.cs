namespace Core.AI.FunctionCalling;

/// <summary>
/// Interface for function registry.
/// </summary>
public interface IFunctionRegistry
{
    IReadOnlyCollection<IAiFunction> GetAll();

    IAiFunction? GetByName(string name);
}