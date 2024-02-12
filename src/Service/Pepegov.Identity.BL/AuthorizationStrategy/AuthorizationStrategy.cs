using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using Pepegov.Identity.BL.GrandType.Infrastructure;
using Pepegov.Identity.BL.GrandType.Model;

namespace Pepegov.Identity.BL.AuthorizationStrategy;

public class AuthorizationStrategy
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthorizationStrategy> _logger;

    public AuthorizationStrategy(IHttpContextAccessor httpContextAccessor, ILogger<AuthorizationStrategy> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Task<IResult> Authorize(OpenIddictRequest request, IGrantTypeConnection grantTypeConnection)
    {
        ArgumentNullException.ThrowIfNull(_httpContextAccessor.HttpContext);
        
        var authorizationContext = new AuthorizationContext()
        {
            HttpContext = _httpContextAccessor.HttpContext,
            OpenIddictRequest = request,
        };
        
        return grantTypeConnection.SignInAsync(authorizationContext);
    }
}