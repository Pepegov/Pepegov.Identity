using System.Security.Claims;
using AegisForge.Application.GrandType.Model;
using AegisForge.Domain.Aggregate;
using AegisForge.Infrastructure.Options;
using MassTransit.Internals;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace AegisForge.PL.Endpoints.Connect.Handlers;

public partial class ConnectHandler(
    IOpenIddictApplicationManager openIddictApplicationManager,
    IOpenIddictScopeManager openIddictScopeManager,
    IOpenIddictAuthorizationManager openIddictAuthorizationManager,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IOptions<List<IdentityScopeOption>> scopeOption,
    IOptions<IdentityClientOption> currentIdentityClientOption)
{
    public async Task<AuthenticateResult> AuthorizeCookieAsync(AuthorizationContext context)
    {
        //Throw if httpcontex or request is null
        if (httpContextAccessor.HttpContext is null)
        {
            ArgumentNullException.ThrowIfNull(httpContextAccessor.HttpContext);
            ArgumentNullException.ThrowIfNull(httpContextAccessor.HttpContext.Request);
        }
        
        context.HttpContext = httpContextAccessor.HttpContext;
        context.OpenIddictRequest = httpContextAccessor.HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Retrieve the user principal stored in the authentication cookie.
        return await httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
    
    public async Task<AuthorizationContext> ConfigureUserAsync(AuthorizationContext context, ClaimsPrincipal principal)
    {
        //User information
        context.User = await userManager.GetUserAsync(principal) ?? throw new InvalidOperationException("The user details cannot be retrieved.");
        context.UserId = await userManager.GetUserIdAsync(context.User);

        return context;
    }
    
    public async Task<AuthorizationContext> ConfigureOpenIddictAsync(AuthorizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context.OpenIddictRequest);
        ArgumentNullException.ThrowIfNull(context.UserId);
        
        //OpenIddict Application
        context.OpenIddictApplication = new OpenIddictApplication();
        context.OpenIddictApplication.Application = await openIddictApplicationManager.FindByClientIdAsync(context.OpenIddictRequest.ClientId!, context.CancellationToken) 
                                                   ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");
        context.OpenIddictApplication.ApplicationId = await openIddictApplicationManager.GetIdAsync(context.OpenIddictApplication.Application, context.CancellationToken)!
                                                     ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");
        
        context.Authorizations = (List<object>?)await openIddictAuthorizationManager.FindAsync(
                     subject: context.UserId,
                     client: context.OpenIddictApplication.ApplicationId!,
                     status: OpenIddictConstants.Statuses.Valid,
                     type: OpenIddictConstants.AuthorizationTypes.Permanent,
                     scopes: context.OpenIddictRequest.GetScopes(), 
                     cancellationToken: context.CancellationToken)
                 .ToListAsync(context.CancellationToken)
             ?? throw new InvalidOperationException("No authorization found");
        
        return context;
    }
}




