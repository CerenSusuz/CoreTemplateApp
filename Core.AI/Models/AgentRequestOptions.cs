using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.Models;

/// <summary>
/// Extended options for agent requests, including the use of named profiles.
/// </summary>
public class AgentRequestOptions : AIRequestOptions
{
    /// <summary>
    /// Optional agent profile name to override other options.
    /// </summary>
    public string? Profile { get; set; }
}