using System.Collections.Immutable;
using System.Security.Claims;
using MassTransit.Internals;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Pepegov.Identity.BL.GrandType.Infrastructure;
using Pepegov.Identity.DAL.Domain;
using Pepegov.Identity.DAL.Models.Identity;

namespace Pepegov.Identity.PL.Pages.Connect;

[Authorize]
public class VerifyModel : PageModel
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    
    public VerifyModel(IOpenIddictApplicationManager applicationManager, UserManager<ApplicationUser> userManager, IOpenIddictScopeManager scopeManager)
    {
        _applicationManager = applicationManager;
        _userManager = userManager;
        _scopeManager = scopeManager;
    }
    
    [BindProperty]
    public string? UserCode { get; set; }
    [BindProperty]
    public string? ApplicationName { get; set; }
    [BindProperty]
    public string? Scope { get; set; }

    public async Task OnGetAsync()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        
        // If the user code was not specified in the query string (e.g as part of the verification_uri_complete),
        // render a form to ask the user to enter the user code manually (non-digit chars are automatically ignored).
        if (string.IsNullOrEmpty(request.UserCode))
        {
            return;
        }
        
        // Retrieve the claims principal associated with the user code.
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        
        if (result.Succeeded)
        {
            // Retrieve the application details from the database using the client_id stored in the principal.
            var application = await _applicationManager.FindByClientIdAsync(result.Principal.GetClaim(OpenIddictConstants.Claims.ClientId)) ??
                              throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            // Render a form asking the user to confirm the authorization demand.
            ApplicationName = await _applicationManager.GetLocalizedDisplayNameAsync(application);
            Scope = string.Join(" ", result.Principal.GetScopes());
            UserCode = request.UserCode;
            return;
        }

        // Redisplay the form when the user code is not valid.
        ViewData["Error"] = "The specified user code is not valid. Please make sure you typed it correctly.";
        ViewData["ErrorTitle"] = OpenIddictConstants.Errors.InvalidToken;
    }

    public async Task OnPostAsync()
    {
        // Retrieve the profile of the logged in user.
        var user = await _userManager.GetUserAsync(User) ??
            throw new InvalidOperationException("The user details cannot be retrieved.");

        // Retrieve the claims principal associated with the user code.
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (result.Succeeded)
        {
            // Create the claims-based identity that will be used by OpenIddict to generate tokens.
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: OpenIddictConstants.Claims.Name,
                roleType: OpenIddictConstants.Claims.Role);

            // Add the claims that will be persisted in the tokens.
            identity.SetClaim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user))
                    .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
                    .SetClaim(OpenIddictConstants.Claims.Name, await _userManager.GetUserNameAsync(user))
                    .SetClaim(OpenIddictConstants.Claims.PreferredUsername, await _userManager.GetUserNameAsync(user))
                    .SetClaims(OpenIddictConstants.Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

            // Note: in this sample, the granted scopes match the requested scope
            // but you may want to allow the user to uncheck specific scopes.
            // For that, simply restrict the list of scopes before calling SetScopes.
            identity.SetScopes(result.Principal.GetScopes());
            identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
            identity.SetDestinations(PrincipalHelper.GetDestinations);

            var properties = new AuthenticationProperties
            {
                // This property points to the address OpenIddict will automatically
                // redirect the user to after validating the authorization demand.
                RedirectUri = "/close"
            };

            await HttpContext.SignInAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), properties);
            return;
        }

        // Redisplay the form when the user code is not valid.
        ViewData["Error"] = "The specified user code is not valid. Please make sure you typed it correctly.";
        ViewData["ErrorTitle"] = OpenIddictConstants.Errors.InvalidToken;
    }
}