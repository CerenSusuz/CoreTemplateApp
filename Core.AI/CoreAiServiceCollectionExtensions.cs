using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.FunctionCalling;
using Core.AI.Memory;
using Core.AI.Providers;
using Core.AI.Providers.Ollama;
using Core.AI.Providers.OpenRouter;
using Core.AI.Providers.Profiles;
using Core.AI.Providers.SemanticKernel;
using Core.AI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Core.AI;

public static class CoreAiServiceCollectionExtensions
{
    public static IServiceCollection AddCoreAi(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<AISettings>(config.GetSection("AiSettings"));
        services.Configure<OllamaSettings>(config.GetSection("Ollama"));
        services.Configure<OpenRouterSettings>(config.GetSection("OpenRouter"));
        services.Configure<AiCatalogOptions>(config.GetSection("AiCatalog"));

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AISettings>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<OllamaSettings>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<OpenRouterSettings>>().Value);

        services.AddSingleton<ChatHistoryStore>();
        services.AddSingleton<AgentProfileProvider>();

        services.AddScoped<AiFunctionDispatcher>();
        services.AddSingleton<IFunctionRegistry, InMemoryFunctionRegistry>();

        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(c => c.AssignableTo<IAiFunction>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        services.AddHttpClient<OllamaAiService>();
        services.AddScoped<OpenRouterAiService>();

        services.AddScoped<IAIService, AIServiceResolver>();

        services.AddScoped<IAgentService, SemanticKernelAgentService>();

        services.AddSingleton<IAiCatalogService, AiCatalogService>();

        return services;
    }
}
