using System.Security.Claims;
using MassTransit.Internals;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Pepegov.Identity.BL.GrandType.Infrastructure;
using Pepegov.Identity.BL.GrandType.Model;
using Pepegov.Identity.DAL.Domain;
using Pepegov.Identity.DAL.Models.Identity;
using Pepegov.MicroserviceFramework.Infrastructure.Helpers;

namespace Pepegov.Identity.BL.GrandType;

[GrantType(OpenIddictConstants.GrantTypes.Password)]
public class PasswordGrantTypeConnection : IGrantTypeConnection
{
    private readonly ApplicationUserClaimsPrincipalFactory _claimsFactory;
    private readonly IOpenIddictScopeManager _openIddictScopeManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public PasswordGrantTypeConnection(ApplicationUserClaimsPrincipalFactory claimsFactory, IOpenIddictScopeManager openIddictScopeManager, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _claimsFactory = claimsFactory;
        _openIddictScopeManager = openIddictScopeManager;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IResult> SignInAsync(AuthorizationContext context)
    {
        var userName = context.OpenIddictRequest.Username;
        if (userName is null)
        {
            // Retrieve the claims principal stored in the refresh token.
            var result = await context.HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                var authenticationProperties = new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.AccessDenied,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "User not found or the refresh token is no longer valid."
                }!);
                Results.Forbid(authenticationProperties, new List<string>(){AuthData.SingInScheme});
            }
            userName = result.Principal.GetClaim(OpenIddictConstants.Claims.Name);
        }
        
        var user = await _userManager.FindByNameAsync(userName!);
        
        var properties = await CheckUser(user, context.OpenIddictRequest);
        if (properties is not null)
        {
            return Results.Forbid(properties, new List<string>(){AuthData.SingInScheme});
        }
        
        // Reset the lockout count
        if (_userManager.SupportsUserLockout)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        context.User = user;
        var claimsPrincipal = await CreateClaimsPrincipalAsync(context);
        
        await context.HttpContext?.SignInAsync(AuthData.SingInScheme, claimsPrincipal)!;
        return Results.SignIn(claimsPrincipal, new AuthenticationProperties() {}, AuthData.SingInScheme);
    }

    public async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(AuthorizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context.User);
        var principal = await _claimsFactory.CreateAsync(context.User);
        principal.AddClaim(OpenIddictConstants.Claims.ClientId, context.OpenIddictRequest.ClientId!);
        principal.AddClaim(OpenIddictConstants.Claims.TokenType, OpenIddictConstants.GrantTypes.Password);

        
        // Don't forget to add destination otherwise it won't be added to the access token.
        if (!string.IsNullOrEmpty(context.OpenIddictRequest.Scope))
        {
            principal.AddClaim(OpenIddictConstants.Claims.Scope, context.OpenIddictRequest.Scope!, OpenIddictConstants.Destinations.AccessToken);
        }
        
        principal.SetResources(await _openIddictScopeManager.ListResourcesAsync(principal.GetScopes(), context.CancellationToken).ToListAsync(context.CancellationToken));
        principal.SetDestinations(PrincipalHelper.GetDestinations);
        
        var claimsPrincipal = new ClaimsPrincipal(principal);
        claimsPrincipal.SetScopes(context.OpenIddictRequest.GetScopes());
        
        return claimsPrincipal;
    }
    
    public async Task<AuthenticationProperties?> CheckUser(ApplicationUser user, OpenIddictRequest? request)
    {
        if (user == null)
        {
            return new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "User not found or the refresh token is no longer valid."
            }!);
        }

        // Ensure the user is still allowed to sign in.
        if (!await _signInManager.CanSignInAsync(user))
        {
            return new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
            }!);
        }
        
        // Ensure the user is not already locked out
        if (_userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(user))
        {
            return new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is already locked out."
            }!);
        }
        
        // Ensure the password is valid
        if (!string.IsNullOrEmpty(request.Password) && !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            if (_userManager.SupportsUserLockout)
            {
                await _userManager.AccessFailedAsync(user);
            }
            
            return new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The password is invalid."
            }!);
        }

        return null;
    }
}