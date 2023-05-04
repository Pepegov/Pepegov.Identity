using System.Security.Claims;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Pepegov.MicroserviceFramerwork.Helpers;
using OpenIddictConstants = OpenIddict.Abstractions.OpenIddictConstants;
using OpenIddictRequest = OpenIddict.Abstractions.OpenIddictRequest;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Connect.Helper;

public static partial class ConnectHelper
{
    public static async Task<IResult> ConnectDeviceCodeGrantType(HttpContext httpContext, 
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var result = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Retrieve the user profile corresponding to the authorization code/refresh token.
        var user = await userManager.FindByIdAsync(result.Principal.GetClaim(OpenIddictConstants.Claims.Subject));
        if (user is null)
        {
            return Results.Forbid(
                authenticationSchemes: new List<string> {OpenIddictServerAspNetCoreDefaults.AuthenticationScheme},
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                }!));
        }

        // Ensure the user is still allowed to sign in.
        if (!await signInManager.CanSignInAsync(user))
        {
            return Results.Forbid(
                authenticationSchemes: new List<string> {OpenIddictServerAspNetCoreDefaults.AuthenticationScheme},
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                }!));
        }

        var identity = new ClaimsIdentity(result.Principal.Claims,
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        identity.SetDestinations(GetDestinations);

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return Results.SignIn(new ClaimsPrincipal(identity),null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
    
    public static async Task<IResult> ConnectAuthorizationCodeGrantType(HttpContext httpContext)
    {
        var authenticateResult = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var claimsPrincipal = authenticateResult.Principal;
        return Results.SignIn(claimsPrincipal!, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
    
    public static async Task<IResult> ConnectClientCredentialsGrantType(OpenIddictRequest? request, IOpenIddictScopeManager scopeManager)
    {
        var claimsPrincipal = await CreateCredentialsClaimsPrincipal(request, scopeManager);
        return Results.SignIn(claimsPrincipal, new AuthenticationProperties(), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    public static async Task<IResult> ConnectPasswordGrantType(OpenIddictRequest? request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOpenIddictScopeManager scopeManager,
        IAccountService accountService)
    {
        var user = await userManager.FindByNameAsync(request.Username);

        var properties = await CheckUser(user, request, userManager, signInManager);
        if (properties is not null)
        {
            return Results.Forbid(properties, new List<string>(){OpenIddictServerAspNetCoreDefaults.AuthenticationScheme});
        }
        
        // Reset the lockout count
        if (userManager.SupportsUserLockout)
        {
            await userManager.ResetAccessFailedCountAsync(user);
        }

        var claimsPrincipal = await CreatePasswordClaimsPrincipal(request, accountService, user, scopeManager);

        return Results.SignIn(claimsPrincipal, new AuthenticationProperties(), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    public static async Task<IResult> ConnectRefreshTokenGrantType(
        OpenIddictRequest? request,
        HttpContext httpContext,
        IAccountService accountService,
        IOpenIddictScopeManager scopeManager,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        
        // Retrieve the claims principal stored in the refresh token.
        var result = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var type = ClaimsHelper.GetValue<string>((ClaimsIdentity)result.Principal.Identity!,
            OpenIddictConstants.Claims.TokenType);

        if (type == OpenIddictConstants.GrantTypes.ClientCredentials)
        {
            return await ConnectClientCredentialsGrantType(request, scopeManager);
        }

        if (type == OpenIddictConstants.GrantTypes.Password)
        {   
            var user = await userManager.FindByIdAsync(result.Principal.GetClaim(OpenIddictConstants.Claims.Subject));
            
            var properties = await CheckUser(user, request, userManager, signInManager);
            if (properties is not null)
            {
                return Results.Forbid(properties, new List<string>(){OpenIddictServerAspNetCoreDefaults.AuthenticationScheme});
            }
        
            // Reset the lockout count
            if (userManager.SupportsUserLockout)
            {
                await userManager.ResetAccessFailedCountAsync(user);
            }

            var claimsPrincipal = await CreatePasswordClaimsPrincipal(request, accountService, user, scopeManager);
            return Results.SignIn(claimsPrincipal, new AuthenticationProperties(), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return Results.BadRequest("Authentication scheme is not found");
    }

    public static async Task<AuthenticationProperties?> CheckUser(ApplicationUser user, OpenIddictRequest? request, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
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
        if (!await signInManager.CanSignInAsync(user))
        {
            return new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
            }!);
        }
        
        // Ensure the user is not already locked out
        if (userManager.SupportsUserLockout && await userManager.IsLockedOutAsync(user))
        {
            return new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is already locked out."
            }!);
        }
        
        // Ensure the password is valid
        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            if (userManager.SupportsUserLockout)
            {
                await userManager.AccessFailedAsync(user);
            }
            
            return new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The password is invalid."
            }!);
        }

        return null;
    }

    public static async Task<ClaimsPrincipal> CreateCredentialsClaimsPrincipal(OpenIddictRequest? request, IOpenIddictScopeManager scopeManager)
    {
        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Subject or sub is a required field, we use the client id as the subject identifier here.
        identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId!);
        identity.AddClaim(OpenIddictConstants.Claims.ClientId, request.ClientId!);
        identity.AddClaim(OpenIddictConstants.Claims.TokenType, OpenIddictConstants.GrantTypes.ClientCredentials);
        
        // Don't forget to add destination otherwise it won't be added to the access token.
        if (!string.IsNullOrEmpty(request.Scope))
        {
            identity.AddClaim(OpenIddictConstants.Claims.Scope, request.Scope!, OpenIddictConstants.Destinations.AccessToken);
        }
        
        identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
        identity.SetDestinations(GetDestinations);

        var claimsPrincipal = new ClaimsPrincipal(identity);
        claimsPrincipal.SetScopes(request.GetScopes());

        return claimsPrincipal;
    }

    public static async Task<ClaimsPrincipal> CreatePasswordClaimsPrincipal(OpenIddictRequest? request, IAccountService accountService, ApplicationUser user, IOpenIddictScopeManager scopeManager)
    {
        var principal = await accountService.GetPrincipalForUserAsync(user);
        principal.AddClaim(OpenIddictConstants.Claims.ClientId, request.ClientId!);
        principal.AddClaim(OpenIddictConstants.Claims.TokenType, OpenIddictConstants.GrantTypes.Password);

        
        // Don't forget to add destination otherwise it won't be added to the access token.
        if (!string.IsNullOrEmpty(request.Scope))
        {
            principal.AddClaim(OpenIddictConstants.Claims.Scope, request.Scope!, OpenIddictConstants.Destinations.AccessToken);
        }
        
        principal.SetResources(await scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());
        principal.SetDestinations(GetDestinations);
        
        var claimsPrincipal = new ClaimsPrincipal(principal);
        claimsPrincipal.SetScopes(request.GetScopes());

        return claimsPrincipal;
    }
}