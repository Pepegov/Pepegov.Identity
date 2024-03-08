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
public class RefreshTokenGrantTypeConnection : IGrantTypeConnection
{
    private readonly IOpenIddictScopeManager _openIddictScopeManager;
    private readonly ApplicationUserClaimsPrincipalFactory _principalFactory;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationUserClaimsPrincipalFactory _claimsFactory;
    
    private readonly ClientCredentialsGrantTypeConnection _credentialsGrantTypeConnectionHandler;
    private readonly PasswordGrantTypeConnection _passwordGrantTypeConnection;
    private readonly DeviceCodeGrantTypeConnection _deviceCodeGrantTypeConnection;
    private readonly AuthorizationCodeGrantTypeConnection _authorizationCodeGrantTypeConnection;
    
    public RefreshTokenGrantTypeConnection(IOpenIddictScopeManager openIddictScopeManager, ApplicationUserClaimsPrincipalFactory principalFactory, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationUserClaimsPrincipalFactory claimsFactory)
    {
        _openIddictScopeManager = openIddictScopeManager;
        _principalFactory = principalFactory;
        _userManager = userManager;
        _signInManager = signInManager;
        _claimsFactory = claimsFactory;

        _credentialsGrantTypeConnectionHandler = new ClientCredentialsGrantTypeConnection(openIddictScopeManager);
        _passwordGrantTypeConnection = new PasswordGrantTypeConnection(principalFactory, openIddictScopeManager, userManager, signInManager);
        _deviceCodeGrantTypeConnection = new DeviceCodeGrantTypeConnection(userManager, signInManager, claimsFactory);
        _authorizationCodeGrantTypeConnection = new AuthorizationCodeGrantTypeConnection();
    }

    public async Task<IResult> SignInAsync(AuthorizationContext context)
    {
        // Retrieve the claims principal stored in the refresh token.
        var result = await context.HttpContext.AuthenticateAsync(AuthData.SingInScheme);
        var type = ClaimsHelper.GetValue<string>((ClaimsIdentity)result.Principal.Identity!,
            OpenIddictConstants.Claims.TokenType);
        
        switch (type)
        {
            case OpenIddictConstants.GrantTypes.ClientCredentials:
                return await _credentialsGrantTypeConnectionHandler.SignInAsync(context);
            case OpenIddictConstants.GrantTypes.Password:
                return await _passwordGrantTypeConnection.SignInAsync(context);
            case OpenIddictConstants.GrantTypes.DeviceCode:
                return await _deviceCodeGrantTypeConnection.SignInAsync(context);
            case OpenIddictConstants.GrantTypes.AuthorizationCode:
                return await _authorizationCodeGrantTypeConnection.SignInAsync(context);
        }

        return Results.BadRequest("Authentication scheme is not found");
    }
    
    public Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(AuthorizationContext context)
    {
        throw new NotImplementedException();
    }
}