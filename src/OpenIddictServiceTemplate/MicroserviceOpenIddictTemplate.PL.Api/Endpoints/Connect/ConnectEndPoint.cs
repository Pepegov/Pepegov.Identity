using MicroserviceOpenIddictTemplate.BL.Services.Interfaces;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Connect.Helper;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;
using Serilog;

namespace MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Connect;

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
        
        app.MapPost("~/connect/token", Token).ExcludeFromDescription();
        
        app.MapPost("~/connect/authorize", Authorize).ExcludeFromDescription();
        app.MapGet("~/connect/authorize", Authorize).ExcludeFromDescription();

        return base.ConfigureApplicationAsync(context);
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

        var authorizations = await authorizationManager.FindAsync(
        subject: userId,
        client: applicationId!,
        status: OpenIddictConstants.Statuses.Valid,
        type: OpenIddictConstants.AuthorizationTypes.Permanent,
        scopes: iddictRequest.GetScopes()).ToListAsync();

        return await connectHandler.Authorize(application, authorizations, user, applicationId, iddictRequest);
    }
    
}