using OpenIddict.Abstractions;

namespace Pepegov.Identity.BL;

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
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Token);
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken);
                    break;
                case OpenIddictConstants.Permissions.GrantTypes.Password:
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
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Logout);
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Introspection);
                    break;
                case OpenIddictConstants.Permissions.GrantTypes.DeviceCode:
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Device);
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Logout);
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Introspection);
                    break;
                case OpenIddictConstants.Permissions.GrantTypes.Password:
                case OpenIddictConstants.Permissions.GrantTypes.ClientCredentials:
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Logout);
                    application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Introspection);
                    break;
            }
        }
    }

    public static void AddRedirectUrisForTesting(this OpenIddictApplicationDescriptor application, string url)
    {
        application.RedirectUris.Add(new Uri("https://localhost:5500"));                                // https://localhost:5500 default vscode live server address 
        application.RedirectUris.Add(new Uri("https://oauth.pstmn.io/v1/callback"));                    // https://oauth.pstmn.io/v1/callback postman
        application.RedirectUris.Add(new Uri("https://postman-redirect.com/callback"));                 // https://postman-redirect.com/callback postman
        application.RedirectUris.Add(new Uri("https://www.thunderclient.com/oauth/callback"));          // https://www.thunderclient.com/
        application.RedirectUris.Add(new Uri($"{url}/swagger/oauth2-redirect.html"));                   // https://swagger.io/
        application.RedirectUris.Add(new Uri("https://localhost:20001/swagger/oauth2-redirect.html"));  // https://swagger.io/ for mobile
        application.RedirectUris.Add(new Uri("https://localhost:10001/swagger/oauth2-redirect.html"));  // https://localhost:10001 Microservice Template
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