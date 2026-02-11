using AegisForge.Application.GrandType.Model;
using MassTransit.Internals;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace AegisForge.PL.Endpoints.Connect.Handlers;

public partial class ConnectHandler
{
    public async Task<IResult> Authorize(AuthorizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context.User);
     
        if (context.OpenIddictRequest is null)
        {
            return await IsOpenIddictRequestIsNull(context);
        }
     
        ArgumentNullException.ThrowIfNull(context.OpenIddictApplication);
        
        switch (await openIddictApplicationManager.GetConsentTypeAsync(context.OpenIddictApplication.Application, context.CancellationToken))
        {
            // If the consent is external (e.g when authorizations are granted by a sysadmin),
            // immediately return an error if no authorization can be found in the database.
            case OpenIddictConstants.ConsentTypes.External when context.Authorizations.Count == 0:
                return IsExternalNotAuthorizations();

            // If the consent is implicit or if an authorization was found,
            // return an authorization response without displaying the consent form.
            case OpenIddictConstants.ConsentTypes.Implicit:
            case OpenIddictConstants.ConsentTypes.External when context.Authorizations.Count != 0:
            case OpenIddictConstants.ConsentTypes.Explicit when context.Authorizations.Count != 0 && !context.OpenIddictRequest.HasPrompt(OpenIddictConstants.Prompts.Consent):
                return await IsExplictHasConsent(context);

            // At this point, no authorization was found in the database and an error must be returned
            // if the client application specified prompt=none in the authorization request.
            case OpenIddictConstants.ConsentTypes.Explicit when context.OpenIddictRequest.HasPrompt(OpenIddictConstants.Prompts.None):
            case OpenIddictConstants.ConsentTypes.Systematic when context.OpenIddictRequest.HasPrompt(OpenIddictConstants.Prompts.None):
                return IsSystematic();

            // In every other case, render the consent form.
            default:
                return Results.Challenge(
                    authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                    properties: new AuthenticationProperties { RedirectUri = "/" });
        }
    }
    
    private static IResult IsExternalNotAuthorizations() 
        => Results.Forbid(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
            properties: new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                    "The logged in user is not allowed to access this client application."
            }!));
    
    private static IResult IsSystematic() 
        => Results.Forbid(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
            properties: new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                    "Interactive user consent is required."
            }!));

    private async Task<IResult> IsOpenIddictRequestIsNull(AuthorizationContext context)
    {
        var principal = await signInManager.CreateUserPrincipalAsync(context.User!);

        context.OpenIddictApplication = new OpenIddictApplication
        {
            Application = await openIddictApplicationManager.FindByClientIdAsync(currentIdentityClientOption.Value.Id, context.CancellationToken) 
                          ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.")
        };
        context.OpenIddictApplication.ApplicationId = await openIddictApplicationManager.GetIdAsync(context.OpenIddictApplication.Application, context.CancellationToken)!
                                                      ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");
        
        //Add all scopes
        principal.SetScopes(scopeOption.Value.Select(x => x.Name));
        principal.SetResources(await openIddictScopeManager.ListResourcesAsync(principal.GetScopes(), context.CancellationToken).ToListAsync(context.CancellationToken));
        
        context.Authorizations = (List<object>?)await openIddictAuthorizationManager.FindAsync(
                                         subject: context.UserId!,
                                         client: context.OpenIddictApplication.ApplicationId!,
                                         status: OpenIddictConstants.Statuses.Valid,
                                         type: OpenIddictConstants.AuthorizationTypes.Permanent,
                                         scopes: principal.GetScopes(), 
                                         cancellationToken: context.CancellationToken)
                                     .ToListAsync(context.CancellationToken)
                                 ?? throw new InvalidOperationException("No authorization found");
        
        // Automatically create a permanent authorization to avoid requiring explicit consent
        // for future authorization or token requests containing the same scopes.
        var authorization = context.Authorizations.LastOrDefault() 
                            ?? await openIddictAuthorizationManager.CreateAsync(
                                principal: principal,
                                subject: context.UserId!,
                                client: context.OpenIddictApplication.ApplicationId,
                                type: OpenIddictConstants.AuthorizationTypes.Permanent,
                                scopes: principal.GetScopes(), 
                                cancellationToken: context.CancellationToken);
        
        principal.SetAuthorizationId(await openIddictAuthorizationManager.GetIdAsync(authorization, context.CancellationToken)); ;
        
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
            _ => [OpenIddictConstants.Destinations.AccessToken]
        });
        
        return Results.SignIn(principal, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
    
    private async Task<IResult> IsExplictHasConsent(AuthorizationContext context)
    {
        var principal = await signInManager.CreateUserPrincipalAsync(context.User!);

        // Note: in this sample, the granted scopes match the requested scope
        // but you may want to allow the user to uncheck specific scopes.
        // For that, simply restrict the list of scopes before calling SetScopes.

        principal.SetScopes(context.OpenIddictRequest!.GetScopes());
        principal.SetResources(await openIddictScopeManager.ListResourcesAsync(principal.GetScopes(), context.CancellationToken).ToListAsync(context.CancellationToken));

        // Automatically create a permanent authorization to avoid requiring explicit consent
        // for future authorization or token requests containing the same scopes.
        var authorization = context.Authorizations.LastOrDefault() 
                            ?? await openIddictAuthorizationManager.CreateAsync(
                            principal: principal,
                            subject: context.UserId!,
                            client: context.OpenIddictApplication!.ApplicationId!,
                            type: OpenIddictConstants.AuthorizationTypes.Permanent,
                            scopes: principal.GetScopes(), 
                            cancellationToken: context.CancellationToken);

        //var identifier = await _openIddictApplicationManager.GetIdAsync(authorization);
        var identifier = await openIddictAuthorizationManager.GetIdAsync(authorization, context.CancellationToken);
        principal.SetAuthorizationId(identifier);

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
            _ => [OpenIddictConstants.Destinations.AccessToken]
        });



        return Results.SignIn(principal, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}