using System.Security.Claims;
using MicroserviceOpenIddictTemplate.BL;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;

namespace MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Connect.Helper;

public partial class ConnectHandler
{
    private readonly IOpenIddictApplicationManager _openIddictApplicationManager;
    private readonly IOpenIddictScopeManager _openIddictScopeManager;
    private readonly IOpenIddictAuthorizationManager _openIddictAuthorizationManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationUserClaimsPrincipalFactory _claimsFactory;
    
    public ConnectHandler(IOpenIddictApplicationManager openIddictApplicationManager, 
        IOpenIddictScopeManager openIddictScopeManager, 
        IOpenIddictAuthorizationManager openIddictAuthorizationManager, 
        SignInManager<ApplicationUser> signInManager, 
        UserManager<ApplicationUser> userManager, ApplicationUserClaimsPrincipalFactory claimsFactory)
    {
        _openIddictApplicationManager = openIddictApplicationManager;
        _openIddictScopeManager = openIddictScopeManager;
        _openIddictAuthorizationManager = openIddictAuthorizationManager;
        _signInManager = signInManager;
        _userManager = userManager;
        _claimsFactory = claimsFactory;
    }
    
    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            case OpenIddictConstants.Claims.Name:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictConstants.Permissions.Scopes.Profile))
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            case OpenIddictConstants.Claims.Email:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictConstants.Permissions.Scopes.Email))
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            case OpenIddictConstants.Claims.Role:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictConstants.Permissions.Scopes.Roles))
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp": yield break;

            default:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield break;
        }
    }
}



