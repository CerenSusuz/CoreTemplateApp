using Core.AI.Abstractions;
using Core.AI.Config;
using Microsoft.Extensions.Options;

namespace Core.AI.Services;

public class AiCatalogService : IAiCatalogService
{
    private readonly AiCatalogOptions _catalog;
    private readonly AISettings _aiSettings;

    public AiCatalogService(IOptions<AiCatalogOptions> catalog, AISettings aiSettings)
    {
        _catalog = catalog.Value;
        _aiSettings = aiSettings;
    }

    public string GetDefaultProvider()
    {
        var p = _aiSettings.Provider;

        return Enum.IsDefined(typeof(AIProvider), p) ? p.ToString() : "OpenRouter";
    }

    public string GetDefaultModel() => _aiSettings.Model ?? "mistralai/mistral-small-3.2-24b-instruct:free";

    public IEnumerable<string> GetProviders()
        => _catalog.Providers.Keys;

    public IEnumerable<string> GetModels(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider)) return Enumerable.Empty<string>();
        return _catalog.Providers.TryGetValue(provider, out var list) ? list : Array.Empty<string>();
    }
}
