using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.Models;

/// <summary>
/// Validator for agent request options to ensure safe and valid parameters.
/// </summary>
public class AgentRequestOptionsValidator : AbstractValidator<AgentRequestOptions>
{
    public AgentRequestOptionsValidator()
    {
        RuleFor(x => x.Temperature)
            .InclusiveBetween(0.0f, 1.0f)
            .WithMessage("Temperature must be between 0.0 and 1.0");

        RuleFor(x => x.Context)
            .NotEmpty()
            .WithMessage("Context must not be empty.");

        RuleFor(x => x.Model)
            .NotEmpty()
            .WithMessage("Model name is required.");
    }
}