using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Pepegov.Identity.BL.GrandType.Infrastructure;
using Pepegov.Identity.BL.GrandType.Model;
using Pepegov.Identity.DAL.Domain;
using Pepegov.Identity.DAL.Models.Identity;
using Pepegov.MicroserviceFramework.Infrastructure.Helpers;

namespace Pepegov.Identity.BL.GrandType;

[GrantType(OpenIddictConstants.GrantTypes.RefreshToken)]
public class RefreshTokenGrantTypeConnection(
    IOpenIddictScopeManager openIddictScopeManager,
    ApplicationUserClaimsPrincipalFactory principalFactory,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationUserClaimsPrincipalFactory claimsFactory)
    : IGrantTypeConnection
{
    private readonly ClientCredentialsGrantTypeConnection _credentialsGrantTypeConnectionHandler = new(openIddictScopeManager);
    private readonly PasswordGrantTypeConnection _passwordGrantTypeConnection = new(principalFactory, openIddictScopeManager, userManager, signInManager);
    private readonly DeviceCodeGrantTypeConnection _deviceCodeGrantTypeConnection = new(userManager, signInManager, claimsFactory);
    private readonly AuthorizationCodeGrantTypeConnection _authorizationCodeGrantTypeConnection = new();

    public async Task<IResult> SignInAsync(AuthorizationContext context)
    {
        // Retrieve the claims principal stored in the refresh token.
        var result = await context.HttpContext.AuthenticateAsync(AuthData.SingInScheme);
        var type = ClaimsHelper.GetValue<string>((ClaimsIdentity)result.Principal.Identity!,
            OpenIddictConstants.Claims.TokenType);

        return type switch
        {
            OpenIddictConstants.GrantTypes.ClientCredentials =>
                await _credentialsGrantTypeConnectionHandler.SignInAsync(context),
            OpenIddictConstants.GrantTypes.Password => await _passwordGrantTypeConnection.SignInAsync(context),
            OpenIddictConstants.GrantTypes.DeviceCode => await _deviceCodeGrantTypeConnection.SignInAsync(context),
            OpenIddictConstants.GrantTypes.AuthorizationCode =>
                await _authorizationCodeGrantTypeConnection.SignInAsync(context),
            _ => Results.BadRequest("Authentication scheme is not found")
        };
    }
    
    public Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(AuthorizationContext context)
    {
        throw new NotImplementedException();
    }
}