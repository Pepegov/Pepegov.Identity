using System.Security.Claims;
using MassTransit.Internals;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Pepegov.Identity.BL;
using Pepegov.Identity.BL.GrandType.Infrastructure;
using Pepegov.Identity.BL.GrandType.Model;
using Pepegov.Identity.DAL.Models.Identity;
using Pepegov.Identity.DAL.Models.Options;

namespace Pepegov.Identity.PL.Endpoints.Connect.Handlers;

public partial class ConnectHandler
{
    private readonly IOpenIddictApplicationManager _openIddictApplicationManager;
    private readonly IOpenIddictScopeManager _openIddictScopeManager;
    private readonly IOpenIddictAuthorizationManager _openIddictAuthorizationManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptions<List<IdentityScopeOption>> _scopeOption;
    private readonly IOptions<IdentityClientOption> _currentIdentityClientOption;
    
    public ConnectHandler(
        IOpenIddictApplicationManager openIddictApplicationManager, 
        IOpenIddictScopeManager openIddictScopeManager, 
        IOpenIddictAuthorizationManager openIddictAuthorizationManager, 
        SignInManager<ApplicationUser> signInManager, 
        UserManager<ApplicationUser> userManager, 
        IHttpContextAccessor httpContextAccessor, 
        IOptions<List<IdentityScopeOption>> scopeOption, IOptions<IdentityClientOption> currentIdentityClientOption)
    {
        _openIddictApplicationManager = openIddictApplicationManager;
        _openIddictScopeManager = openIddictScopeManager;
        _openIddictAuthorizationManager = openIddictAuthorizationManager;
        _signInManager = signInManager;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _scopeOption = scopeOption;
        _currentIdentityClientOption = currentIdentityClientOption;
    }

    public async Task<AuthenticateResult> AuthorizeCookieAsync(AuthorizationContext context)
    {
        //Throw if httpcontex or request is null
        if (_httpContextAccessor.HttpContext is null)
        {
            ArgumentNullException.ThrowIfNull(_httpContextAccessor.HttpContext);
            ArgumentNullException.ThrowIfNull(_httpContextAccessor.HttpContext.Request);
        }
        
        context.HttpContext = _httpContextAccessor.HttpContext;
        context.OpenIddictRequest = _httpContextAccessor.HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Retrieve the user principal stored in the authentication cookie.
        return await _httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
    
    public async Task<AuthorizationContext> ConfigureUserAsync(AuthorizationContext context, ClaimsPrincipal principal)
    {
        //User information
        context.User = await _userManager.GetUserAsync(principal) ?? throw new InvalidOperationException("The user details cannot be retrieved.");
        context.UserId = await _userManager.GetUserIdAsync(context.User);

        return context;
    }
    
    public async Task<AuthorizationContext> ConfigureOpenIddictAsync(AuthorizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context.OpenIddictRequest);
        ArgumentNullException.ThrowIfNull(context.UserId);
        
        //OpenIddict Application
        context.OpenIddictApplication = new OpenIddictApplication();
        context.OpenIddictApplication.Application = await _openIddictApplicationManager.FindByClientIdAsync(context.OpenIddictRequest.ClientId!, context.CancellationToken) 
                                                   ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");
        context.OpenIddictApplication.ApplicationId = await _openIddictApplicationManager.GetIdAsync(context.OpenIddictApplication.Application, context.CancellationToken)!
                                                     ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");
        
        context.Authorizations = (List<object>?)await _openIddictAuthorizationManager.FindAsync(
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




