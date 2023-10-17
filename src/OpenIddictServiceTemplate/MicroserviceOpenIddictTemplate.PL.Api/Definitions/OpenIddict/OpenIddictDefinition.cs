using MicroserviceOpenIddictTemplate.DAL.Database;
using MicroserviceOpenIddictTemplate.PL.Api.Definitions.Options.Models;
using OpenIddict.Abstractions;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace MicroserviceOpenIddictTemplate.PL.Api.Definitions.OpenIddict;

public class OpenIddictDefinition : ApplicationDefinition
{
    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("identitysetting.json");
        IConfiguration identityConfiguration = configurationBuilder.Build();
        
        var scopeOptions = identityConfiguration.GetSection("Scopes").Get<List<IdentityScopeOption>>()!;
        var scopes = new List<string> { OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles, };
        scopeOptions.ForEach(x => scopes.Add(x.Name));

        context.ServiceCollection.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<ApplicationDbContext>()
                    .ReplaceDefaultEntities<Guid>();
            })
            .AddServer(options =>
            {
                options
                    .AllowDeviceCodeFlow()
                    .AllowAuthorizationCodeFlow()//.RequireProofKeyForCodeExchange()
                    .AllowPasswordFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow();
                
                options.SetAccessTokenLifetime(TimeSpan.FromHours(30));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

                options.SetAuthorizationEndpointUris("connect/authorize")
                    //.RequireProofKeyForCodeExchange() // enable PKCE
                    .SetDeviceEndpointUris("connect/device")
                    //.SetIntrospectionEndpointUris("connect/introspect")
                    .SetLogoutEndpointUris("connect/logout")
                    .SetTokenEndpointUris("connect/token")
                    .SetVerificationEndpointUris("connect/verify")
                    .SetUserinfoEndpointUris("connect/userinfo");
                
                // only for developer mode
                options
                    .AddEphemeralEncryptionKey()
                    .AddEphemeralSigningKey() 
                    .DisableAccessTokenEncryption();
                
                // server scopes
                options.RegisterScopes(scopes.ToArray());
                
                //certificate
                options
                    .AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();
                
                
                // registration ASP.NET Core host and configure setting for ASP.NET Core.
                options
                    .UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserinfoEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer(); 
                options.UseAspNetCore();
            });
        return base.ConfigureServicesAsync(context);
    }
}