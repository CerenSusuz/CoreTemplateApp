namespace Core.AI.FunctionCalling;

public class InMemoryFunctionRegistry : IFunctionRegistry
{
    private readonly Dictionary<string, IAiFunction> _functions;

    public InMemoryFunctionRegistry(IEnumerable<IAiFunction> functions)
    {
        _functions = functions
        .ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<IAiFunction> GetAll() => _functions.Values.ToList();

    public IAiFunction? GetByName(string name)
    {
        return _functions.TryGetValue(name, out var function) ? function : null;
    }
}