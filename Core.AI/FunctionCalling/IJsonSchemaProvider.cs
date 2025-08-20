namespace Core.AI.FunctionCalling;

/// <summary>
/// Interface for defining JSON schema for a function.
/// </summary>
public interface IJsonSchemaProvider
{
    object GetJsonSchema();
}