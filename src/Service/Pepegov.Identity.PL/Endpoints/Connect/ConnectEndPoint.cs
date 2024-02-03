using MassTransit.Internals;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using Pepegov.Identity.BL.Services.Interfaces;
using Pepegov.Identity.DAL.Domain;
using Pepegov.Identity.DAL.Models.Identity;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;
using Serilog;

namespace Pepegov.Identity.PL.Endpoints.Connect;

using ConnectHandler = Helper.ConnectHandler;

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
        
        app.MapPost("~/connect/token", Token).WithOpenApi().WithTags("Connect");
        
        app.MapPost("~/connect/authorize", Authorize).WithOpenApi().WithTags("Connect");
        app.MapGet("~/connect/authorize", Authorize).WithOpenApi().WithTags("Connect");
        
        app.MapGet("~/connect/sing-in/by-userid", SingInByUserId).WithOpenApi().WithTags("Connect");;
        app.MapGet("~/connect/sing-out", SingOut).WithOpenApi().WithTags("Connect");;

        return base.ConfigureApplicationAsync(context);
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = "Admin")]
    private async Task<IResult> SingOut(
        HttpContext httpContext)
    {
        await httpContext.SignOutAsync(AuthData.AuthenticationSchemes);
        return Results.SignOut();
    }
    
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = "Admin")]
    private async Task<IResult> SingInByUserId(
        HttpContext httpContext,
        [FromQuery] Guid userId,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IAccountService accountService
        )
    {
        try
        {
            var principal = await accountService.GetPrincipalByIdAsync(userId.ToString());
            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return Results.SignIn(principal);
        }
        catch (MicroserviceNotFoundException e)
        {
            return Results.Problem(e.Message);
        }
    }

    private async Task<IResult> Token(
        HttpContext httpContext,
        [FromServices] ConnectHandler connectHandler)
    {
        var request = httpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        if (request.Scope is null)
        {
            Log.Warning($"Request not have scopes. Connection id: {httpContext.Connection.Id}");
            //return Results.Problem("The specified grant type is not supported.");
        }

        IResult? result = null;

        if (request.IsClientCredentialsGrantType())
        {
            result = await connectHandler.ConnectClientCredentialsGrantType(request);
        }
        if (request.IsPasswordGrantType())
        {
            result = await connectHandler.ConnectPasswordGrantType(request, httpContext);
        }
        if (request.IsAuthorizationCodeGrantType())
        {
            result = await connectHandler.ConnectAuthorizationCodeGrantType(httpContext);
        }
        if (request.IsDeviceCodeGrantType())
        {
            result = await connectHandler.ConnectDeviceCodeGrantType(httpContext);
        }
        if (request.IsRefreshTokenGrantType())
        {
            result = await connectHandler.ConnectRefreshTokenGrantType(request, httpContext);
        }

        if (result is null)
        {
            return Results.Problem("The specified grant type is not supported.");
        }
        return result;
    }   
    
    private async Task<IResult> Authorize(
        HttpContext httpContext,
        [FromServices] ConnectHandler connectHandler,
        [FromServices] IOpenIddictScopeManager scopeManager,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] SignInManager<ApplicationUser> signInManager,
        [FromServices] IOpenIddictApplicationManager applicationManager,
        [FromServices] IOpenIddictAuthorizationManager authorizationManager)
    {
        var request = httpContext.Request;
        var iddictRequest = httpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Retrieve the user principal stored in the authentication cookie.
        var result = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            return Results.Challenge(new AuthenticationProperties
            {
                RedirectUri = request.PathBase + request.Path + QueryString.Create(request.HasFormContentType
                ? request.Form.ToList()
                : request.Query.ToList())
            },
            new List<string> { CookieAuthenticationDefaults.AuthenticationScheme });
        }

        var user = await userManager.GetUserAsync(result.Principal) ?? throw new InvalidOperationException("The user details cannot be retrieved.");

        var application = await applicationManager.FindByClientIdAsync(iddictRequest.ClientId!) ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");
        var applicationId = await applicationManager.GetIdAsync(application);
        var userId = await userManager.GetUserIdAsync(user);

        var authorizations = (List<object>?)await authorizationManager.FindAsync(
        subject: userId,
        client: applicationId!,
        status: OpenIddictConstants.Statuses.Valid,
        type: OpenIddictConstants.AuthorizationTypes.Permanent,
        scopes: iddictRequest.GetScopes()).ToListAsync();

        return await connectHandler.Authorize(application, authorizations, user, applicationId, iddictRequest);
    }
    
}