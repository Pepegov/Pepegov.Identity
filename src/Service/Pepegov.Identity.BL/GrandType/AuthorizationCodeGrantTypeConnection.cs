using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using OpenIddict.Abstractions;
using Pepegov.Identity.BL.GrandType.Infrastructure;
using Pepegov.Identity.BL.GrandType.Model;
using Pepegov.Identity.DAL.Domain;

namespace Pepegov.Identity.BL.GrandType;

[GrantType(OpenIddictConstants.GrantTypes.AuthorizationCode)]
public class AuthorizationCodeGrantTypeConnection : IGrantTypeConnection
{
    public async Task<IResult> SignInAsync(AuthorizationContext context)
    {
        var claimsPrincipal = await CreateClaimsPrincipalAsync(context);
        return Results.SignIn(claimsPrincipal, new AuthenticationProperties() { IsPersistent = true }, AuthData.SingInScheme);
    }

    public async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(AuthorizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context.HttpContext);
        var authenticateResult = await context.HttpContext.AuthenticateAsync(AuthData.SingInScheme);
        var principal = authenticateResult.Principal;
        principal.AddClaim(OpenIddictConstants.Claims.ClientId, context.OpenIddictRequest.ClientId!);
        principal.AddClaim(OpenIddictConstants.Claims.TokenType, OpenIddictConstants.GrantTypes.AuthorizationCode);
        return principal!;
    }
}