using Core.AI.Abstractions;
using Core.AI.FunctionCalling;
using Core.AI.Memory;
using Core.AI.Providers;
using Core.AI.Providers.Ollama;
using Core.AI.Providers.OpenRouter;
using Core.AI.Providers.Profiles;
using Core.AI.Providers.SemanticKernel;
using CoreTemplate.AI.UI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ✅ Named HttpClient
builder.Services.AddHttpClient("AiApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7059/");
});

// AI Service setup
builder.Services.AddScoped<IAIService, AIServiceResolver>();

// Providers
builder.Services.AddScoped<OllamaAiService>();
builder.Services.AddScoped<OpenRouterAiService>();
builder.Services.AddScoped<OllamaModelProvider>();
builder.Services.AddScoped<OpenRouterModelProvider>();
builder.Services.AddScoped<AIModelProviderResolver>();

// Agents and Memory
builder.Services.AddScoped<IAgentService, SemanticKernelAgentService>();
builder.Services.AddSingleton<ChatHistoryStore>();
builder.Services.AddSingleton<AgentProfileProvider>();

// Function Calling
builder.Services.AddScoped<AiFunctionDispatcher>();
builder.Services.AddSingleton<IFunctionRegistry, InMemoryFunctionRegistry>();

builder.Services.Scan(scan => scan
    .FromAssemblies(Assembly.GetExecutingAssembly())
    .AddClasses(c => c.AssignableTo<IAiFunction>())
    .AsImplementedInterfaces()
    .WithSingletonLifetime());

await builder.Build().RunAsync();
