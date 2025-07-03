using MediatR;

namespace CoreApp.Application.Features.AI.Commands;

public record PromptTextCommand(string Prompt, AiRequestOptions? Options) : IRequest<string>;
