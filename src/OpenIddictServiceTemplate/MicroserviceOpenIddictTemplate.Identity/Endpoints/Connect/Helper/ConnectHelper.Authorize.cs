using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Connect.Helper;

public static partial class ConnectHelper
{
    public static async Task<IResult> Authorize(
        IOpenIddictApplicationManager applicationManager, 
        IOpenIddictScopeManager scopeManager,
        IOpenIddictAuthorizationManager authorizationManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        object? application, 
        List<object>? authorizations,
        ApplicationUser user,
        string? applicationId,
        OpenIddictRequest? iddictRequest)
    {
        switch (await applicationManager.GetConsentTypeAsync(application))
        {
            // If the consent is external (e.g when authorizations are granted by a sysadmin),
            // immediately return an error if no authorization can be found in the database.
            case OpenIddictConstants.ConsentTypes.External when !authorizations.Any():
                return IsExternalNotAuthorizations();

            // If the consent is implicit or if an authorization was found,
            // return an authorization response without displaying the consent form.
            case OpenIddictConstants.ConsentTypes.Implicit:
            case OpenIddictConstants.ConsentTypes.External when authorizations.Any():
            case OpenIddictConstants.ConsentTypes.Explicit when authorizations.Any() && !iddictRequest.HasPrompt(OpenIddictConstants.Prompts.Consent):
                return await IsExplictHasConsent(signInManager, userManager, iddictRequest, scopeManager, authorizationManager, authorizations, user, applicationId);

            // At this point, no authorization was found in the database and an error must be returned
            // if the client application specified prompt=none in the authorization request.
            case OpenIddictConstants.ConsentTypes.Explicit when iddictRequest.HasPrompt(OpenIddictConstants.Prompts.None):
            case OpenIddictConstants.ConsentTypes.Systematic when iddictRequest.HasPrompt(OpenIddictConstants.Prompts.None):
                return IsSystematic();

            // In every other case, render the consent form.
            default:
                return Results.Challenge(
                    authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                    properties: new AuthenticationProperties { RedirectUri = "/" });
        }
    }
    
    private static IResult IsExternalNotAuthorizations() 
        => Results.Forbid(
            authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
            properties: new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                    "The logged in user is not allowed to access this client application."
            }!));
    
    private static IResult IsSystematic() 
        => Results.Forbid(
            authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
            properties: new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                    "Interactive user consent is required."
            }!));
    
    private static async Task<IResult> IsExplictHasConsent(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        OpenIddictRequest? iddictRequest,
        IOpenIddictScopeManager scopeManager,
        IOpenIddictAuthorizationManager authorizationManager,
        List<object>? authorizations,
        ApplicationUser user,
        string? applicationId)
    {
        var principal = await signInManager.CreateUserPrincipalAsync(user);

        // Note: in this sample, the granted scopes match the requested scope
        // but you may want to allow the user to uncheck specific scopes.
        // For that, simply restrict the list of scopes before calling SetScopes.

        principal.SetScopes(iddictRequest.GetScopes());
        principal.SetResources(await scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

        // Automatically create a permanent authorization to avoid requiring explicit consent
        // for future authorization or token requests containing the same scopes.
        var authorization = authorizations.LastOrDefault() ?? await authorizationManager.CreateAsync(
            principal: principal,
            subject: await userManager.GetUserIdAsync(user),
            client: applicationId!,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: principal.GetScopes());

        principal.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization));

        principal.SetDestinations(static claim => claim.Type switch
        {
            // If the "profile" scope was granted, allow the "name" claim to be
            // added to the access and identity tokens derived from the principal.
            OpenIddictConstants.Claims.Name when claim.Subject.HasScope(OpenIddictConstants.Scopes.Profile) => new[]
            {
                OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken
            },

            // Never add the "secret_value" claim to access or identity tokens.
            // In this case, it will only be added to authorization codes,
            // refresh tokens and user/device codes, that are always encrypted.
            "secret_value" => Array.Empty<string>(),

            // Otherwise, add the claim to the access tokens only.
            _ => new[] { OpenIddictConstants.Destinations.AccessToken }
        });



        return Results.SignIn(principal, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}