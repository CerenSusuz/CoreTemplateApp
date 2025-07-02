using CoreApp.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp.Application.Common.Interfaces.AI
{
    public interface IAIService
    {
        Task<string> PromptAsync(string prompt, AiRequestOptions? options = null);
    }
}
