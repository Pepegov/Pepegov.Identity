using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Pepegov.Identity.BL.GrandType.Infrastructure;
using Pepegov.Identity.BL.GrandType.Model;
using Pepegov.Identity.DAL.Domain;
using Pepegov.Identity.DAL.Models.Identity;

namespace Pepegov.Identity.BL.GrandType;

[GrantType(OpenIddictConstants.GrantTypes.DeviceCode)]
public class DeviceCodeGrantTypeConnection(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationUserClaimsPrincipalFactory claimsFactory)
    : IGrantTypeConnection
{
    public async Task<IResult> SignInAsync(AuthorizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context.HttpContext);
        
        var request = await context.HttpContext.AuthenticateAsync(AuthData.SingInScheme);

        // Retrieve the user profile corresponding to the authorization code/refresh token.
        context.User = await userManager.FindByIdAsync(request.Principal.GetClaim(OpenIddictConstants.Claims.Subject));
        if (context.User is null)
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
        if (!await signInManager.CanSignInAsync(context.User))
        {
            return Results.Forbid(
                authenticationSchemes: new List<string> {AuthData.SingInScheme},
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                }!));
        }
        
        var claimsPrincipal = await CreateClaimsPrincipalAsync(context);
        return Results.SignIn(claimsPrincipal , new AuthenticationProperties() { IsPersistent = true }, AuthData.SingInScheme);
    }

    public async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(AuthorizationContext context)
    {
        var principal = await claimsFactory.CreateAsync(context.User!);
        principal.AddClaim(OpenIddictConstants.Claims.ClientId, context.OpenIddictRequest!.ClientId!);
        principal.AddClaim(OpenIddictConstants.Claims.TokenType, OpenIddictConstants.GrantTypes.DeviceCode);
        
        // Don't forget to add destination otherwise it won't be added to the access token.
        if (!string.IsNullOrEmpty(context.OpenIddictRequest.Scope))
        {
            principal.AddClaim(OpenIddictConstants.Claims.Scope, context.OpenIddictRequest.Scope!, OpenIddictConstants.Destinations.AccessToken);
        }
        
        var identity = new ClaimsIdentity(principal.Claims,
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);
        
        identity.SetDestinations(PrincipalHelper.GetDestinations);

        var claimsPrincipal = new ClaimsPrincipal(identity);
        claimsPrincipal.SetScopes(context.OpenIddictRequest.GetScopes());
        
        return claimsPrincipal;
    }
}