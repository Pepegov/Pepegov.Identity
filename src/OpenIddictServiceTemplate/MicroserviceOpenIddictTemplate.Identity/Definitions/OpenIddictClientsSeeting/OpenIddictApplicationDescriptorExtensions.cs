using OpenIddict.Abstractions;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.OpenIddictClientsSeeting;

public static class OpenIddictApplicationDescriptorExtensions
{
    public static void AddScopes(this OpenIddictApplicationDescriptor application, List<string>? scopes)
    {
        if (scopes is not null)
        {
            scopes.ForEach(x => 
                application.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + x));
        }
    }

    public static void AddGrandTypes(this OpenIddictApplicationDescriptor application, List<string>? grandTypes)
    {
        if (grandTypes is not null)
        {
            grandTypes.ForEach(x => 
                application.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.GrantType + x));
        }
    }

    public static void AddResponseTypes(this OpenIddictApplicationDescriptor application)
    {
        foreach (var item in application.Permissions.ToList())
        {
            switch (item)
            {
                case OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode:
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdToken);
                    break;
                case OpenIddictConstants.Permissions.GrantTypes.ClientCredentials:
                    break;
            }
        }
    }
    
    public static void AddEndpoints(this OpenIddictApplicationDescriptor application)
    {
        foreach (var item in application.Permissions.ToList())
        {
            switch (item)
            {
                case OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode:
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
                    break;
                case OpenIddictConstants.Permissions.GrantTypes.ClientCredentials:
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
                    break;
            }
        }
    }

    public static void AddRedirectUrisForTesting(this OpenIddictApplicationDescriptor application, string url)
    {
        application.RedirectUris.Add(new Uri("https://www.thunderclient.com/oauth/callback"));          // https://www.thunderclient.com/
        application.RedirectUris.Add(new Uri($"{url}/swagger/oauth2-redirect.html"));                   // https://swagger.io/
        application.RedirectUris.Add(new Uri("https://localhost:20001/swagger/oauth2-redirect.html"));  // https://swagger.io/ for mobile
        application.RedirectUris.Add(new Uri("https://localhost:8001/swagger/oauth2-redirect.html"));   // https://localhost:8001 Microservice Template
    }

    public static void AddRedirectUris(this OpenIddictApplicationDescriptor application, List<string>? uris)
    {
        if (uris is null)
        {
            return;
        }

        foreach (var url in uris)
        {
            application.RedirectUris.Add(new Uri(url));
        }
    }
}