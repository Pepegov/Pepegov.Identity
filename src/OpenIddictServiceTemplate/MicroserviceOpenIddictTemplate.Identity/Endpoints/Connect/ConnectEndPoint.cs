using System.Security.Claims;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Application.Services;
using MicroserviceOpenIddictTemplate.Identity.Base.Definition;
using MicroserviceOpenIddictTemplate.Identity.Base.Helpers;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Connect.Helper;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Serilog;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Connect;

public class ConnectEndPoint : Definition
{
    public override void ConfigureApplicationAsync(WebApplication app)
    {
        app.MapPost("~/connect/token", Token).ExcludeFromDescription();
        
        app.MapPost("~/connect/authorize", Authorize).ExcludeFromDescription();
        app.MapGet("~/connect/authorize", Authorize).ExcludeFromDescription();
    }
    
    private async Task<IResult> Token(
        HttpContext httpContext,
        IOpenIddictScopeManager manager,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAccountService accountService)
    {
        var request = httpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        if (request.Scope is null)
        {
            Log.Warning($"Request not have scopes. Connection id: {httpContext.Connection.Id}");
            //return Results.Problem("The specified grant type is not supported.");
        }

        if (request.IsClientCredentialsGrantType())
        {
            return await ConnectHelper.ConnectClientCredentialsGrantType(request);
        }
        if (request.IsPasswordGrantType())
        {
            return await ConnectHelper.ConnectPasswordGrantType(request, userManager, signInManager, accountService);
        }
        if (request.IsAuthorizationCodeGrantType())
        {
            return await ConnectHelper.ConnectAuthorizationCodeGrantType(httpContext);
        }
        if (request.IsDeviceCodeGrantType())
        {
            return await ConnectHelper.ConnectDeviceCodeGrantType(httpContext, userManager, signInManager);
        }

        return Results.Problem("The specified grant type is not supported.");
    }   
    
    private async Task<IResult> Authorize(
        HttpContext httpContext,
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

        return await ConnectHelper.Authorize(
            applicationManager, scopeManager, authorizationManager,
            signInManager, userManager, application, authorizations,
            user, applicationId, iddictRequest);
    }
    
}