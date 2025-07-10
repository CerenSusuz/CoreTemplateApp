using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.Models
{
    public class AgentProfile
    {
        public string Name { get; set; } = string.Empty;
        public string Context { get; set; } = "You are a helpful assistant.";
        public float Temperature { get; set; } = 0.7f;
        public string Model { get; set; } = "gpt-3.5-turbo";
    }
}
