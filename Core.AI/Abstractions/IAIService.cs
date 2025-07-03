using Core.AI.Models;

namespace Core.AI.Abstractions;

public interface IAIService
{
    Task<string> PromptAsync(string prompt, AIRequestOptions? options = null);
}
