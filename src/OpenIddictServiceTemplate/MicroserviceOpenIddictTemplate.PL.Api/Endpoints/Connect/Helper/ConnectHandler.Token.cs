using System.Security.Claims;
using MicroserviceOpenIddictTemplate.DAL.Domain;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Pepegov.MicroserviceFramework.Infrastructure.Helpers;

namespace MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Connect.Helper;

public partial class ConnectHandler
{
    public async Task<IResult> ConnectDeviceCodeGrantType(HttpContext httpContext)
    {
        var result = await httpContext.AuthenticateAsync(AuthData.SingInScheme);

        // Retrieve the user profile corresponding to the authorization code/refresh token.
        var user = await _userManager.FindByIdAsync(result.Principal.GetClaim(OpenIddictConstants.Claims.Subject));
        if (user is null)
        {
            return Results.Forbid(
                authenticationSchemes: new List<string> {AuthData.SingInScheme},
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                }!));
        }

        // Ensure the user is still allowed to sign in.
        if (!await _signInManager.CanSignInAsync(user))
        {
            return Results.Forbid(
                authenticationSchemes: new List<string> {AuthData.SingInScheme},
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
        return Results.SignIn(new ClaimsPrincipal(identity), new AuthenticationProperties() { IsPersistent = true }, AuthData.SingInScheme);
    }
    
    public async Task<IResult> ConnectAuthorizationCodeGrantType(HttpContext httpContext)
    {
        var authenticateResult = await httpContext.AuthenticateAsync(AuthData.SingInScheme);
        var claimsPrincipal = authenticateResult.Principal;
        
        return Results.SignIn(claimsPrincipal!, new AuthenticationProperties() { IsPersistent = true }, AuthData.SingInScheme);
    }
    
    public async Task<IResult> ConnectClientCredentialsGrantType(OpenIddictRequest? request)
    {
        var claimsPrincipal = await CreateCredentialsClaimsPrincipal(request);
        return Results.SignIn(claimsPrincipal, new AuthenticationProperties() { IsPersistent = true }, AuthData.SingInScheme);
    }

    public async Task<IResult> ConnectPasswordGrantType(OpenIddictRequest? request, HttpContext httpContext)
    {
        var user = await _userManager.FindByNameAsync(request.Username);

        var properties = await CheckUser(user, request);
        if (properties is not null)
        {
            return Results.Forbid(properties, new List<string>(){AuthData.SingInScheme});
        }
        
        // Reset the lockout count
        if (_userManager.SupportsUserLockout)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        var claimsPrincipal = await CreatePasswordClaimsPrincipal(request, user);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
        return Results.SignIn(claimsPrincipal, new AuthenticationProperties() {}, AuthData.SingInScheme);
    }

    public async Task<IResult> ConnectRefreshTokenGrantType(
        OpenIddictRequest? request,
        HttpContext httpContext)
    {
        
        // Retrieve the claims principal stored in the refresh token.
        var result = await httpContext.AuthenticateAsync(AuthData.SingInScheme);

        var type = ClaimsHelper.GetValue<string>((ClaimsIdentity)result.Principal.Identity!,
            OpenIddictConstants.Claims.TokenType);

        if (type == OpenIddictConstants.GrantTypes.ClientCredentials)
        {
            return await ConnectClientCredentialsGrantType(request);
        }

        if (type == OpenIddictConstants.GrantTypes.Password)
        {   
            var user = await _userManager.FindByIdAsync(result.Principal.GetClaim(OpenIddictConstants.Claims.Subject));
            
            var properties = await CheckUser(user, request);
            if (properties is not null)
            {
                return Results.Forbid(properties, new List<string>(){AuthData.SingInScheme});
            }
        
            // Reset the lockout count
            if (_userManager.SupportsUserLockout)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
            }

            var claimsPrincipal = await CreatePasswordClaimsPrincipal(request, user);
            return Results.SignIn(claimsPrincipal, new AuthenticationProperties() { IsPersistent = true }, AuthData.SingInScheme);
        }

        return Results.BadRequest("Authentication scheme is not found");
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
        if (!await _userManager.CheckPasswordAsync(user, request.Password))
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

    public async Task<ClaimsPrincipal> CreateCredentialsClaimsPrincipal(OpenIddictRequest? request)
    {
        var identity = new ClaimsIdentity(AuthData.SingInScheme);

        // Subject or sub is a required field, we use the client id as the subject identifier here.
        identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId!);
        identity.AddClaim(OpenIddictConstants.Claims.ClientId, request.ClientId!);
        identity.AddClaim(OpenIddictConstants.Claims.TokenType, OpenIddictConstants.GrantTypes.ClientCredentials);
        
        // Don't forget to add destination otherwise it won't be added to the access token.
        if (!string.IsNullOrEmpty(request.Scope))
        {
            identity.AddClaim(OpenIddictConstants.Claims.Scope, request.Scope!, OpenIddictConstants.Destinations.AccessToken);
        }
        
        identity.SetResources(await _openIddictScopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
        identity.SetDestinations(GetDestinations);

        var claimsPrincipal = new ClaimsPrincipal(identity);
        claimsPrincipal.SetScopes(request.GetScopes());

        return claimsPrincipal;
    }

    public async Task<ClaimsPrincipal> CreatePasswordClaimsPrincipal(OpenIddictRequest? request, ApplicationUser user)
    {
        var principal = await _claimsFactory.CreateAsync(user);
        principal.AddClaim(OpenIddictConstants.Claims.ClientId, request.ClientId!);
        principal.AddClaim(OpenIddictConstants.Claims.TokenType, OpenIddictConstants.GrantTypes.Password);

        
        // Don't forget to add destination otherwise it won't be added to the access token.
        if (!string.IsNullOrEmpty(request.Scope))
        {
            principal.AddClaim(OpenIddictConstants.Claims.Scope, request.Scope!, OpenIddictConstants.Destinations.AccessToken);
        }
        
        principal.SetResources(await _openIddictScopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());
        principal.SetDestinations(GetDestinations);
        
        var claimsPrincipal = new ClaimsPrincipal(principal);
        claimsPrincipal.SetScopes(request.GetScopes());

        return claimsPrincipal;
    }
}