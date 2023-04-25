using System.Security.Claims;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
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
                }));
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
                }));
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
    
    public static Task<IResult> ConnectClientCredentialsGrantType(OpenIddictRequest? request)
    {
        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Subject or sub is a required field, we use the client id as the subject identifier here.
        identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId!);
        identity.AddClaim(OpenIddictConstants.Claims.ClientId, request.ClientId!);

        // Don't forget to add destination otherwise it won't be added to the access token.
        if (!string.IsNullOrEmpty(request.Scope))
        {
            identity.AddClaim(OpenIddictConstants.Claims.Scope, request.Scope!, OpenIddictConstants.Destinations.AccessToken);
        }

        var claimsPrincipal = new ClaimsPrincipal(identity);

        claimsPrincipal.SetScopes(request.GetScopes());
        var result = Results.SignIn(claimsPrincipal, new AuthenticationProperties(),OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        return Task.FromResult(result);
    }

    public static async Task<IResult> ConnectPasswordGrantType(OpenIddictRequest? request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAccountService accountService)
    {
        var user = await userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            return Results.Problem("Invalid operation");
        }
        // Ensure the user is allowed to sign in
        if (!await signInManager.CanSignInAsync(user))
        {
            return Results.Problem("Invalid operation");
        }
        // Ensure the user is not already locked out
        if (userManager.SupportsUserLockout && await userManager.IsLockedOutAsync(user))
        {
            return Results.Problem("Invalid operation");
        }
        // Ensure the password is valid
        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            if (userManager.SupportsUserLockout)
            {
                await userManager.AccessFailedAsync(user);
            }

            return Results.Problem("Invalid operation");
        }
        // Reset the lockout count
        if (userManager.SupportsUserLockout)
        {
            await userManager.ResetAccessFailedCountAsync(user);
        }

        var principal = await accountService.GetPrincipalForUserAsync(user);
        return Results.SignIn(principal, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}