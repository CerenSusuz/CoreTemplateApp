using Core.AI.Models;
using Microsoft.Extensions.Configuration;

namespace Core.AI.Providers.Profiles;

public class AgentProfileProvider
{
    private readonly List<AgentProfile> _profiles;

    public AgentProfileProvider(IConfiguration configuration)
    {
        _profiles = configuration
            .GetSection("AgentProfiles")
            .Get<List<AgentProfile>>() ?? new();
    }

    public AgentProfile GetProfile(string name)
    {
        return _profiles.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
               ?? _profiles.First();
    }

    public IEnumerable<AgentProfile> GetAllProfiles() => _profiles;
}
