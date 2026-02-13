using System.Security.Claims;
using AegisForge.Application.AuthorizationStrategy;
using AegisForge.Application.GrandType.Infrastructure;
using AegisForge.Application.GrandType.Model;
using AegisForge.Application.Service.Interfaces;
using AegisForge.Domain.Aggregate;
using AegisForge.Domain.Domain;
using AegisForge.Infrastructure.Domain;
using AegisForge.PL.Endpoints.Connect.Handlers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Pepegov.Identity.BL;
using Pepegov.Identity.BL.Services.Interfaces;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;
using Pepegov.MicroserviceFramework.Infrastructure.Helpers;
using Serilog;

namespace AegisForge.PL.Endpoints.Connect;

using ConnectHandler = ConnectHandler;

public class ConnectEndPoint : ApplicationDefinition
{
    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        context.ServiceCollection.AddTransient(typeof(ConnectHandler));
        return base.ConfigureServicesAsync(context);
    }

    public override Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
    {
        var app = context.Parse<WebDefinitionApplicationContext>().WebApplication;
        
        //default identity
        app.MapGet("~/Account/Login", (HttpContext httpContext) =>
        {
            return Results.Challenge(new AuthenticationProperties
                {
                    RedirectUri = httpContext.Request.PathBase + httpContext.Request.Query.ToList().FirstOrDefault(x => x.Key == "ReturnUrl").Value
                },
                new List<string> { CookieAuthenticationDefaults.AuthenticationScheme });
        }).ExcludeFromDescription();
        
        //User area
        app.MapPost("~/connect/token", Token).WithOpenApi().WithTags("Auth.Connect");
        app.MapPost("~/connect/authorize", Authorize).WithOpenApi().WithTags("Auth.Connect");
        app.MapGet("~/connect/authorize", Authorize).WithOpenApi().WithTags("Auth.Connect");
        
        app.MapPost("~/connect/logout", Logout).WithOpenApi().WithTags("Auth.Connect");
        app.MapGet("~/connect/logout", Logout).WithOpenApi().WithTags("Auth.Connect");
        
        app.MapGet("~/connect/userinfo", UserInfo).WithOpenApi().WithTags("Auth.Connect");
        app.MapPost("~/connect/userinfo", UserInfo).WithOpenApi().WithTags("Auth.Connect");
        
        //Admin area
        app.MapGet("~/connect/superadmin/authorize", SuperAdminAuthorize).WithOpenApi().WithTags("Auth.Connect");
        app.MapPost("~/connect/superadmin/authorize", SuperAdminAuthorize).WithOpenApi().WithTags("Auth.Connect");
        
        return base.ConfigureApplicationAsync(context);
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    private async Task<IResult> Logout(
        [FromServices] SignInManager<ApplicationUser> signInManager,
        [FromServices] ITokenManagementService tokenManagementService,
        HttpContext httpContext,
        [FromQuery] string? redirectUri = null)
    {
        await signInManager.SignOutAsync();
        return Results.SignOut(new AuthenticationProperties() { RedirectUri = redirectUri }, new List<string>() { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme } );
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes)]
    private async Task<IResult> UserInfo(
        HttpContext httpContext,
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        var principalUser = (ClaimsIdentity)httpContext.User.Identity!;
        var userId = ClaimsHelper.GetValue<string>(principalUser, OpenIddictConstants.Claims.Subject);
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.Challenge(
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is bound to an account that no longer exists."
                }!), new List<string>() { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme });
        }
        
        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            [OpenIddictConstants.Claims.Subject] = await userManager.GetUserIdAsync(user)
        };

        if (principalUser.FindScope(OpenIddictConstants.Permissions.Scopes.Email))
        {
            claims[OpenIddictConstants.Claims.Email] = await userManager.GetEmailAsync(user);
            claims[OpenIddictConstants.Claims.EmailVerified] = await userManager.IsEmailConfirmedAsync(user);
        }

        if (principalUser.FindScope(OpenIddictConstants.Permissions.Scopes.Phone))
        {
            claims[OpenIddictConstants.Claims.PhoneNumber] = await userManager.GetPhoneNumberAsync(user);
            claims[OpenIddictConstants.Claims.PhoneNumberVerified] = await userManager.IsPhoneNumberConfirmedAsync(user);
        }

        if (principalUser.FindScope(OpenIddictConstants.Permissions.Scopes.Profile))
        {
            claims[OpenIddictConstants.Claims.Name] = user.FirstName;
            claims[OpenIddictConstants.Claims.GivenName] = user.FirstName;
            claims[OpenIddictConstants.Claims.FamilyName] = user.LastName;
            claims[OpenIddictConstants.Claims.Birthdate] = user.BirthDate.ToUniversalTime();
            claims[OpenIddictConstants.Claims.UpdatedAt] = user.ApplicationUserProfile!.Updated.ToUniversalTime();
            
            if (user.Gender != null) 
                claims[OpenIddictConstants.Claims.Gender] = user.Gender.ToString()!;
            if (user.UserName != null) 
                claims[OpenIddictConstants.Claims.Nickname] = user.UserName;
            if (user.MiddleName != null) 
                claims[OpenIddictConstants.Claims.MiddleName] = user.MiddleName;
        }

        if (principalUser.FindScope(OpenIddictConstants.Permissions.Scopes.Roles))
        {
            claims[OpenIddictConstants.Claims.Role] = await userManager.GetRolesAsync(user);
        }

        // Note: the complete list of standard claims supported by the OpenID Connect specification
        // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

        return Results.Ok(claims);
    }
    
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    private async Task<IResult> SuperAdminAuthorize(
        HttpContext httpContext,
        [FromQuery] string client_id,
        [FromQuery] string response_type,
        [FromQuery] string redirect_uri,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IAccountService accountService,
        [FromServices] ConnectHandler connectHandler)
    {
        var context = new AuthorizationContext();
        context.CancellationToken = httpContext.RequestAborted;
        var result = await connectHandler.AuthorizeCookieAsync(context);

        var roles = ClaimsHelper.GetValues<string>((ClaimsIdentity)httpContext.User.Identity!, OpenIddictConstants.Claims.Role);
        if (!result.Succeeded ||
            httpContext.User.Identity is not null && roles.Contains(UserRoles.SuperAdmin))
        {
            ArgumentNullException.ThrowIfNull(context.HttpContext);
            return Results.Challenge(new AuthenticationProperties
                {
                    RedirectUri = context.HttpContext.Request.PathBase + context.HttpContext.Request.Path + QueryString.Create(context.HttpContext.Request.HasFormContentType
                        ? context.HttpContext.Request.Form.ToList()
                        : context.HttpContext.Request.Query.ToList())
                },
                new List<string> { CookieAuthenticationDefaults.AuthenticationScheme });
        } 
        
        await connectHandler.ConfigureUserAsync(context, result.Principal);
        await connectHandler.ConfigureOpenIddictAsync(context);

        return await connectHandler.Authorize(context);
    }
    
    private async Task<IResult> Token(
        HttpContext httpContext,
        [FromServices] GrantTypeConnectionManager grantTypeConnectionManager,
        [FromServices] AuthorizationStrategy authorizationStrategy)
    {
        var request = httpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        if (request.Scope is null)
        {
            Log.Logger.Warning($"Request not have scopes. Connection id: {httpContext.Connection.Id}");
        }
        
        if (grantTypeConnectionManager.TryGet(request.GrantType!, out var connectHandler))
        {
            return await authorizationStrategy.Authorize(request, connectHandler);
        }

        return Results.Problem("The specified grant type is not supported.");
    }   
    
    private async Task<IResult> Authorize(
        HttpContext httpContext,
        [FromServices] ConnectHandler connectHandler,
        [FromServices] ISessionService sessionService)
    {
        var context = new AuthorizationContext
        {
            CancellationToken = httpContext.RequestAborted,
            HttpContext = httpContext,
        };
        
        if (await sessionService.IsSessionTerminated() is true)
        {
            return Results.Challenge(new AuthenticationProperties
                {
                    RedirectUri = httpContext!.Request.PathBase + httpContext.Request.Path + QueryString.Create(httpContext.Request.HasFormContentType
                        ? httpContext.Request.Form.ToList()
                        : httpContext.Request.Query.ToList())
                },
                new List<string> { CookieAuthenticationDefaults.AuthenticationScheme });
        }
        
        // This call also initializes context.HttpContext and context.OpenIddictRequest.
        var result = await connectHandler.AuthorizeCookieAsync(context);

        // For prompt=none, the authorization endpoint MUST NOT trigger interactive login UI.
        // If the user is not authenticated, return a proper OIDC error response.
        if (!result.Succeeded)
        {
            if (context.OpenIddictRequest?.HasPrompt(OpenIddictConstants.Prompts.None) is true)
            {
                return Results.Forbid(
                    authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "User is not authenticated."
                    }));
            }

            ArgumentNullException.ThrowIfNull(context.HttpContext);
            return Results.Challenge(new AuthenticationProperties
                {
                    RedirectUri = context.HttpContext.Request.PathBase + context.HttpContext.Request.Path + QueryString.Create(context.HttpContext.Request.HasFormContentType
                        ? context.HttpContext.Request.Form.ToList()
                        : context.HttpContext.Request.Query.ToList())
                },
                new List<string> { CookieAuthenticationDefaults.AuthenticationScheme });
        }

        await connectHandler.ConfigureUserAsync(context, result.Principal!);
        await connectHandler.ConfigureOpenIddictAsync(context);

        return await connectHandler.Authorize(context);
    }
    
}