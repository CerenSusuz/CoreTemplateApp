using CoreApp.Application.Common.Models;

namespace CoreApp.Application.Common.Interfaces.AI;

public interface IAIService
{
    Task<string> PromptAsync(string prompt, AiRequestOptions? options = null);
}
