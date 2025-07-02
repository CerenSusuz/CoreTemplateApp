using CoreApp.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp.Application.Features.AI.Commands
{
    public record PromptTextCommand(string Prompt, AiRequestOptions? Options) : IRequest<string>;
}
