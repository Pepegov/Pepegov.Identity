using System.Security.Claims;
using MassTransit.Internals;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using OpenIddict.Abstractions;
using Pepegov.Identity.BL.GrandType.Infrastructure;
using Pepegov.Identity.BL.GrandType.Model;
using Pepegov.Identity.DAL.Domain;

namespace Pepegov.Identity.BL.GrandType;

[GrantType(OpenIddictConstants.GrantTypes.ClientCredentials)]
public class ClientCredentialsGrantTypeConnection : IGrantTypeConnection
{
    private readonly IOpenIddictScopeManager _openIddictScopeManager;

    public ClientCredentialsGrantTypeConnection(IOpenIddictScopeManager openIddictScopeManager)
    {
        _openIddictScopeManager = openIddictScopeManager;
    }

    public async Task<IResult> SignInAsync(AuthorizationContext context)
    {
        var claimsPrincipal = await CreateClaimsPrincipalAsync(context);
        return Results.SignIn(claimsPrincipal, new AuthenticationProperties() { IsPersistent = true }, AuthData.SingInScheme);
    }

    public async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(AuthorizationContext context)
    {
        var identity = new ClaimsIdentity(AuthData.SingInScheme);

        // Subject or sub is a required field, we use the client id as the subject identifier here.
        identity.AddClaim(OpenIddictConstants.Claims.Subject, context.OpenIddictRequest.ClientId!);
        identity.AddClaim(OpenIddictConstants.Claims.ClientId, context.OpenIddictRequest.ClientId!);
        identity.AddClaim(OpenIddictConstants.Claims.TokenType, OpenIddictConstants.GrantTypes.ClientCredentials);
        
        // Don't forget to add destination otherwise it won't be added to the access token.
        if (!string.IsNullOrEmpty(context.OpenIddictRequest.Scope))
        {
            identity.AddClaim(OpenIddictConstants.Claims.Scope, context.OpenIddictRequest.Scope!, OpenIddictConstants.Destinations.AccessToken);
        }
        
        identity.SetResources(await _openIddictScopeManager.ListResourcesAsync(identity.GetScopes(), context.CancellationToken).ToListAsync(context.CancellationToken));
        identity.SetDestinations(PrincipalHelper.GetDestinations);

        var claimsPrincipal = new ClaimsPrincipal(identity);
        claimsPrincipal.SetScopes(context.OpenIddictRequest.GetScopes());

        return claimsPrincipal;
    }
}