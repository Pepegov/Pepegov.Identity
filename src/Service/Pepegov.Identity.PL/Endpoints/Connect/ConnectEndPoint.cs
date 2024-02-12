using System.Reflection;
using System.Security.Claims;
using MassTransit.Internals;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.Server.AspNetCore;
using Pepegov.Identity.BL.AuthorizationStrategy;
using Pepegov.Identity.BL.GrandType.Infrastructure;
using Pepegov.Identity.BL.GrandType.Model;
using Pepegov.Identity.BL.Services.Interfaces;
using Pepegov.Identity.DAL.Domain;
using Pepegov.Identity.DAL.Models.Identity;
using Pepegov.Identity.PL.Endpoints.Connect.Handlers;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;
using Pepegov.MicroserviceFramework.Infrastructure.Helpers;
using Serilog;

namespace Pepegov.Identity.PL.Endpoints.Connect;

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
        
        //User area
        app.MapPost("~/connect/token", Token).WithOpenApi().WithTags("Connect");
        app.MapPost("~/connect/authorize", Authorize).WithOpenApi().WithTags("Connect");
        app.MapGet("~/connect/authorize", Authorize).WithOpenApi().WithTags("Connect");
        
        app.MapGet("~/connect/sing-out", SingOut).WithOpenApi().WithTags("Connect");;
        
        //Admin area
        app.MapGet("~/connect/superadmin/authorize", SuperAdminAuthorize).WithOpenApi().WithTags("Connect");
        app.MapPost("~/connect/superadmin/authorize", SuperAdminAuthorize).WithOpenApi().WithTags("Connect");

        
        return base.ConfigureApplicationAsync(context);
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    private async Task<IResult> SingOut(
        HttpContext httpContext)
    {
        await httpContext.SignOutAsync(AuthData.AuthenticationSchemes);
        return Results.SignOut();
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

        var roles = ClaimsHelper.GetValues<string>((ClaimsIdentity)httpContext.User.Identity, OpenIddictConstants.Claims.Role);
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
        [FromServices] ConnectHandler connectHandler)
    {
        var context = new AuthorizationContext();
        context.CancellationToken = httpContext.RequestAborted;
        var result = await connectHandler.AuthorizeCookieAsync(context);
        
        if (!result.Succeeded)
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
    
}