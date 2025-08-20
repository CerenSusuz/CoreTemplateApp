using Core.AI.Models;
using Microsoft.Extensions.Configuration;

namespace Core.AI.Providers.Profiles;

/// <summary>
/// Provides access to configured agent profiles from appsettings.json.
/// </summary>
public class AgentProfileProvider
{
    private readonly List<AgentProfile> _profiles;

    /// <summary>
    /// Loads agent profiles from configuration.
    /// </summary>
    public AgentProfileProvider(IConfiguration configuration)
    {
        _profiles = configuration
            .GetSection("AgentProfiles")
            .Get<List<AgentProfile>>() ?? new();
    }

    /// <summary>
    /// Gets the profile by name, or the first/default if not found.
    /// </summary>
    public AgentProfile GetProfile(string name)
    {
        return _profiles.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
               ?? _profiles.First();
    }

    /// <summary>
    /// Returns all loaded agent profiles.
    /// </summary>
    public IEnumerable<AgentProfile> GetAllProfiles() => _profiles;
}
