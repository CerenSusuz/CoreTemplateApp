using CoreApp.Application.Common.Interfaces.AI;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp.Application.Features.AI.Commands
{
    public class PromptTextCommandHandler : IRequestHandler<PromptTextCommand, string>
    {
        private readonly IAIService _aiService;

        public PromptTextCommandHandler(IAIService aiService) => _aiService = aiService;

        public async Task<string> Handle(PromptTextCommand request, CancellationToken cancellationToken)
            => await _aiService.PromptAsync(request.Prompt, request.Options);
    }
}
