namespace Core.AI.FunctionCalling;

public class FunctionRegistry : IFunctionRegistry
{
    private readonly Dictionary<string, IAiFunction> _functions =
    new(StringComparer.OrdinalIgnoreCase);

    public void Register(IAiFunction function)
    {
        if (_functions.ContainsKey(function.Name))
            throw new InvalidOperationException($"Function '{function.Name}' is already registered.");


        _functions[function.Name] = function;
    }

    public IReadOnlyCollection<IAiFunction> GetAll()
    {
        return _functions.Values.ToList().AsReadOnly();
    }

    public IAiFunction? GetByName(string name)
    {
        _functions.TryGetValue(name, out var function);

        return function;
    }
}