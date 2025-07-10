using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.Memory;
using Core.AI.Models;
using Core.AI.Providers.Profiles;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Core.AI.Providers.SemanticKernel;

public class SemanticKernelAgentService : IAgentService
{
    private readonly ChatHistoryStore _chatStore;
    private readonly AgentProfileProvider _profileProvider;
    private readonly AgentRequestOptionsValidator _validator;
    private readonly IConfiguration _config;

    public SemanticKernelAgentService(
        IConfiguration config,
        ChatHistoryStore chatStore,
        AgentProfileProvider profileProvider)
    {
        _config = config;
        _chatStore = chatStore;
        _profileProvider = profileProvider;
        _validator = new AgentRequestOptionsValidator();
    }

    private (Kernel Kernel, IChatCompletionService ChatService) BuildKernel(AIRequestOptions? options)
    {
        var defaultProvider = Enum.TryParse(_config["AiSettings:Provider"], out AIProvider fallbackProvider)
            ? fallbackProvider
            : AIProvider.OpenRouter;

        var provider = options?.Provider ?? defaultProvider;
        var model = options?.Model ?? _config["AiSettings:Model"];
        var builder = Kernel.CreateBuilder();

        switch (provider)
        {
            case AIProvider.OpenRouter:
                var openRouterApiKey = _config["OpenRouterAI:ApiKey"];
                builder.AddOpenAIChatCompletion(
                    modelId: model,
                    apiKey: openRouterApiKey,
                    serviceId: "openrouter",
                    endpoint: new Uri("https://openrouter.ai/api/v1")
                );
                break;

            case AIProvider.Ollama:
                builder.AddOpenAIChatCompletion(
                    modelId: model,
                    apiKey: null,
                    serviceId: "ollama",
                    endpoint: new Uri("http://localhost:11434/v1")
                );
                break;

            default:
                throw new NotSupportedException($"Unsupported provider: {provider}");
        }

        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        return (kernel, chatService);
    }

    public async Task<string> ChatAsync(string prompt, AgentRequestOptions? options = null, string? userId = null)
    {
        options ??= new AgentRequestOptions();

        var validationResult = _validator.Validate(options);
        if (!validationResult.IsValid)
            return $"[Validation Error] {string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage))}";

        var profile = _profileProvider.GetProfile(options.Profile ?? "Default");
        var context = options.Context ?? profile.Context;
        var temperature = options.Temperature ?? profile.Temperature;

        var (kernel, chatService) = BuildKernel(options);

        var messages = new ChatHistory();
        messages.AddSystemMessage(context);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var history = _chatStore.GetHistory(userId);
            foreach (var (role, content) in history)
            {
                if (role == "user") messages.AddUserMessage(content);
                else if (role == "assistant") messages.AddAssistantMessage(content);
            }

            _chatStore.AddMessage(userId, "user", prompt);
        }

        messages.AddUserMessage(prompt);

        var settings = new OpenAIPromptExecutionSettings { Temperature = temperature };
        var response = await chatService.GetChatMessageContentAsync(messages, settings);
        var result = response?.Content ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(userId))
            _chatStore.AddMessage(userId, "assistant", result);

        return result;
    }

    public async IAsyncEnumerable<string> StreamChatAsync(string prompt, AgentRequestOptions? options = null, string? userId = null)
    {
        options ??= new AgentRequestOptions();

        var validationResult = _validator.Validate(options);
        if (!validationResult.IsValid)
        {
            yield return $"[Validation Error] {string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage))}";
            yield break;
        }

        var profile = _profileProvider.GetProfile(options.Profile ?? "Default");
        var context = options.Context ?? profile.Context;
        var temperature = options.Temperature ?? profile.Temperature;

        var (kernel, chatService) = BuildKernel(options);

        var messages = new ChatHistory();
        messages.AddSystemMessage(context);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var history = _chatStore.GetHistory(userId);
            foreach (var (role, content) in history)
            {
                if (role == "user") messages.AddUserMessage(content);
                else if (role == "assistant") messages.AddAssistantMessage(content);
            }

            _chatStore.AddMessage(userId, "user", prompt);
        }

        messages.AddUserMessage(prompt);

        var settings = new OpenAIPromptExecutionSettings { Temperature = temperature };
        var responseStream = chatService.GetStreamingChatMessageContentsAsync(messages, settings);

        string fullResponse = "";

        await foreach (var content in responseStream)
        {
            if (!string.IsNullOrWhiteSpace(content.Content))
            {
                yield return content.Content;
                fullResponse += content.Content;
            }
        }

        if (!string.IsNullOrWhiteSpace(userId))
            _chatStore.AddMessage(userId, "assistant", fullResponse);
    }

    public Task<bool> IsModelSupportedAsync(string model, string provider)
        => Task.FromResult(true);
}
