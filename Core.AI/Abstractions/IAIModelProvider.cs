namespace Core.AI.Abstractions;

// <summary>
/// Provides access to AI model listing.
/// </summary>
public interface IAIModelProvider
{
    /// <summary>
    /// Returns a list of available model names.
    /// </summary>
    Task<List<string>> GetAvailableModelsAsync();
}
