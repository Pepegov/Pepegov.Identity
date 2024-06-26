﻿using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using Pepegov.Identity.DAL.Database;
using Pepegov.Identity.DAL.Models.Options;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace Pepegov.Identity.PL.Definitions.OpenIddict;

public class OpenIddictDefinition : ApplicationDefinition
{
    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        var identityConfiguration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("identitysetting.json")
            .Build();
        
        context.ServiceCollection.Configure<IdentityAddressOption>(context.Configuration.GetSection("IdentityServerUrl"));
        context.ServiceCollection.Configure<List<IdentityScopeOption>>(identityConfiguration.GetSection("Scopes"));
        context.ServiceCollection.Configure<IdentityClientOption>(identityConfiguration.GetSection("CurrentIdentityClient"));
        context.ServiceCollection.Configure<List<IdentityClientOption>>(identityConfiguration.GetSection("ClientsIdentity"));
        
        var scopeOptions = identityConfiguration.GetSection("Scopes").Get<List<IdentityScopeOption>>()!;
        var scopes = new List<string> { OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles, };
        scopeOptions.ForEach(x => scopes.Add(x.Name));

        context.ServiceCollection.AddOpenIddict()
            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                // Configure OpenIddict to use the Entity Framework Core stores and models.
                // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
                options.UseEntityFrameworkCore()
                    .UseDbContext<ApplicationDbContext>()
                    .ReplaceDefaultEntities<Guid>();
                
                // Enable Quartz.NET integration.
                options.UseQuartz();
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
                options.SetAuthorizationCodeLifetime(TimeSpan.FromHours(1));

                options.SetAuthorizationEndpointUris("connect/authorize", "connect/superadmin/authorize")
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
                    //.AddEncryptionKey(new SymmetricSecurityKey(Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")))
                    .AddDevelopmentSigningCertificate();
                options
                    .AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();
                
                // registration ASP.NET Core host and configure setting for ASP.NET Core.
                options
                    .UseAspNetCore()
                    .EnableVerificationEndpointPassthrough()
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