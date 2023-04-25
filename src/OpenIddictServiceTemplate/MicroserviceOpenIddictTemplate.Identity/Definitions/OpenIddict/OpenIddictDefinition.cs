using MicroserviceOpenIddictTemplate.DAL.Database;
using MicroserviceOpenIddictTemplate.Identity.Base.Definition;
using MicroserviceOpenIddictTemplate.Identity.Definitions.Options.Models;
using OpenIddict.Abstractions;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.OpenIddict;

public class OpenIddictDefinition : Definition
{
    public override void ConfigureServicesAsync(IServiceCollection services, WebApplicationBuilder builder)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("identitysetting.json");
        IConfiguration identityConfiguration = configurationBuilder.Build();
        
        var scopeOptions = identityConfiguration.GetSection("Scopes").Get<List<IdentityScopeOption>>()!;
        var scopes = new List<string> { OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles, };
        scopeOptions.ForEach(x => scopes.Add(x.Name));

        services.AddOpenIddict()
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
    }
}