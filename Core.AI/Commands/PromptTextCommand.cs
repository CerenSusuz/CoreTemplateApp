using Core.AI.Models;
using MediatR;

namespace Core.AI.Commands;

/// <summary>
/// Represents a command to prompt text to an AI service.
/// </summary>
public record PromptTextCommand(string Prompt, AIRequestOptions? Options) : IRequest<string>;
