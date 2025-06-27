using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Request.Username)
                .NotEmpty().WithMessage("Username is required.");

            RuleFor(x => x.Request.Email)
                .NotEmpty().EmailAddress();

            RuleFor(x => x.Request.Password)
                .NotEmpty().MinimumLength(6);
        }
    }
}
