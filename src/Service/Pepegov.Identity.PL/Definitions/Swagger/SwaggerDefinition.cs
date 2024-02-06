using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Pepegov.Identity.DAL.Domain;
using Pepegov.Identity.PL.Definitions.Options.Models;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;
using Pepegov.MicroserviceFramework.Infrastructure.Attributes;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Pepegov.Identity.PL.Definitions.Swagger
{
    public class SwaggerDefinition : ApplicationDefinition
    {
        public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
        {
            context.ServiceCollection.AddSwaggerGen(options =>
            {
                // Swagger description
                var now = DateTime.Now.ToString("f");
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = AppData.ServiceName,
                    Version = AppData.ServiceVersion,
                    Description = AppData.ServiceDescription + $" | Upload time: {now}"
                });
                
                options.ResolveConflictingActions(x => x.First());
                
                // Controllers titles
                options.TagActionsBy(api =>
                {
                    string tag;
                    if (api.ActionDescriptor is { } descriptor)
                    {
                        var attribute = descriptor.EndpointMetadata.OfType<FeatureGroupNameAttribute>().FirstOrDefault();
                        tag = attribute?.GroupName ?? descriptor.RouteValues["controller"] ?? "Untitled";
                    }
                    else
                    {
                        tag = api.RelativePath!;
                    }

                    var tags = new List<string>();
                    if (!string.IsNullOrEmpty(tag))
                    {
                        tags.Add(tag);
                    }
                    return tags;
                });
                
                var identityConfiguration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("identitysetting.json")
                    .Build();
                
                var url = context.Configuration.GetSection("IdentityServerUrl").GetValue<string>("Authority");
                
                // Get scopes for AddSecurityDefinition 
                var scopesList = identityConfiguration.GetSection("Scopes").Get<List<IdentityScopeOption>>();
                var scopes = scopesList!.ToDictionary(x => x.Name, x => x.Description);

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                TokenUrl = new Uri($"{url}/connect/token", UriKind.Absolute),
                                AuthorizationUrl = new Uri($"{url}/connect/authorize", UriKind.Absolute),
                                Scopes = scopes,
                            },
                            ClientCredentials = new OpenApiOAuthFlow
                            {
                                Scopes = scopes,
                                TokenUrl = new Uri($"{url}/connect/token", UriKind.Absolute),
                            },
                            Password = new OpenApiOAuthFlow
                            {
                                TokenUrl = new Uri($"{url}/connect/token", UriKind.Absolute),
                                Scopes = scopes,
                            }
                        },
                        Type = SecuritySchemeType.OAuth2
                    }
                );
                
                
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            },
                            In = ParameterLocation.Cookie,
                            Type = SecuritySchemeType.OAuth2
                        },
                        new List<string>()
                    }
                });
            });
                        
            return base.ConfigureServicesAsync(context);
        }

        public override Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
        {
            var app = context.Parse<WebDefinitionApplicationContext>().WebApplication;
            
            using var scope = app.Services.CreateAsyncScope();
            var url = scope.ServiceProvider.GetService<IOptions<IdentityAddressOption>>()!.Value.Authority;
            var client = scope.ServiceProvider.GetService<IOptions<IdentityClientOption>>()!.Value;
            
            app.UseSwagger();
            app.UseSwaggerUI(settings =>
            {
                // settings.SwaggerEndpoint(_swaggerConfig, $"api");
                settings.DefaultModelExpandDepth(0);
                settings.DefaultModelRendering(ModelRendering.Model);
                settings.DefaultModelsExpandDepth(0);
                settings.DocExpansion(DocExpansion.None);
                settings.OAuthScopeSeparator(" ");
                settings.OAuthClientId(client.Id);
                settings.OAuthClientSecret(client.Secret);
                settings.OAuth2RedirectUrl($"{url}/swagger/oauth2-redirect.html");
                settings.DisplayRequestDuration();
            });
            return base.ConfigureApplicationAsync(context);
        }
    }
}
