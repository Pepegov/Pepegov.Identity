using AegisForge.Application.GrandType.Infrastructure;
using AegisForge.Application.GrandType.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace AegisForge.Application.AuthorizationStrategy;

public class AuthorizationStrategy(IHttpContextAccessor httpContextAccessor, ILogger<AuthorizationStrategy> logger)
{
    public Task<IResult> Authorize(OpenIddictRequest request, IGrantTypeConnection grantTypeConnection)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor.HttpContext);
        
        var authorizationContext = new AuthorizationContext()
        {
            HttpContext = httpContextAccessor.HttpContext,
            OpenIddictRequest = request,
        };
        
        return grantTypeConnection.SignInAsync(authorizationContext);
    }
}