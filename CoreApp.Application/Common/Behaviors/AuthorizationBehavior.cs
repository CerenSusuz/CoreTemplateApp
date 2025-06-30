using CoreApp.Application.Common.Interfaces;
using CoreApp.Application.Common.Interfaces.Auth;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CoreApp.Application.Common.Behaviors
{
    public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationBehavior(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (request is not IAuthorizedRequest)
                return await next();

            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            return await next();
        }
    }
}
