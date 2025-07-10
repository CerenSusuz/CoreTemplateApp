using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.Models
{
    public class AgentPromptRequest
    {
        public string Prompt { get; set; } = "";
        public AgentRequestOptions? Options { get; set; }
        public string? UserId { get; set; }
    }
}
