using Core.AI.Models;

namespace Core.AI.Abstractions;
public interface IAIService
{
    Task<string> GetCompletionAsync(string prompt, AIRequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<string> PromptAsync(string prompt, AIRequestOptions? options = null);
    IAsyncEnumerable<string> StreamPromptAsync(string prompt, AIRequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<bool> IsModelSupportedAsync(string model);
}

