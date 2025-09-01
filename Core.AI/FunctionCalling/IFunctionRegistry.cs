namespace Core.AI.FunctionCalling;

public interface IFunctionRegistry
{
    IReadOnlyCollection<IAiFunction> GetAll();

    IAiFunction? GetByName(string name);
}