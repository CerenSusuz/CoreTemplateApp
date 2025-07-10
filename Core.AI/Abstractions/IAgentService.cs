using Core.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.Abstractions
{
    public interface IAgentService
    {
        Task<bool> IsModelSupportedAsync(string model, string provider = "OpenRouter");

        Task<string> ChatAsync(string prompt, AgentRequestOptions? options = null, string? userId = null);

        IAsyncEnumerable<string> StreamChatAsync(string prompt, AgentRequestOptions? options = null, string? userId = null);
    }
}
