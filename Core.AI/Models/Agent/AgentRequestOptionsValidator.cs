using FluentValidation;

namespace Core.AI.Models.Agent;

/// <summary>
/// Validator for agent request options to ensure safe and valid parameters.
/// </summary>
public class AgentRequestOptionsValidator : AbstractValidator<AgentRequestOptions>
{
    public AgentRequestOptionsValidator()
    {
        RuleFor(x => x.Temperature)
            .InclusiveBetween(0, 2).When(x => x.Temperature.HasValue);
        RuleFor(x => x.Model).Must(_ => true);
    }
}