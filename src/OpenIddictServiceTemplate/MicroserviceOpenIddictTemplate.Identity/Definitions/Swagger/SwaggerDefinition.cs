using MicroserviceOpenIddictTemplate.DAL.Domain;
using MicroserviceOpenIddictTemplate.Identity.Definitions.Options.Models;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Pepegov.MicroserviceFramerwork.Attributes;
using Pepegov.MicroserviceFramerwork.Patterns.Definition;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.Swagger
{
    public class SwaggerDefinition : Definition
    {
        private const string _swaggerConfig = "/swagger/v1/swagger.json";

        public override void ConfigureServicesAsync(IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddSwaggerGen(options =>
            {
                // Swagger description
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = AppData.ServiceName,
                    Version = AppData.ServiceVersion,
                    Description = AppData.ServiceDescription
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
                
                var url = identityConfiguration.GetSection("IdentityServerUrl").GetValue<string>("Authority");
                
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
        }

        public override void ConfigureApplicationAsync(WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                return;
            }
            
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
        }
    }
}
