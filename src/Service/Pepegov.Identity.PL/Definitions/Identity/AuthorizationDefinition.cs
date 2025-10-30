using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server.AspNetCore;
using Pepegov.Identity.BL.OpenIddictHandlers;
using Pepegov.Identity.DAL.Domain;
using Pepegov.Identity.PL.Definitions.OpenIddict;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace Pepegov.Identity.PL.Definitions.Identity;

public class AuthorizationDefinition : ApplicationDefinition
{
    public new int Priority = 8;
    
    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        context.ServiceCollection.AddDistributedMemoryCache();

        context.ServiceCollection
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = ".Pepegov.Session";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                
                options.LoginPath = "/connect/login";
                options.LogoutPath = "/connect/logout";
                options.AccessDeniedPath = "/connect/access-denied";
                
                options.Events.OnSignedIn = (context) =>
                {
                    context.Response.IssueSessionCookie();
                    return Task.CompletedTask;
                };
                options.Events.OnSigningOut = (context) =>
                {
                    context.Response.DeleteSessionCookie();
                    return Task.CompletedTask;
                };
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, "Bearer", options =>
            {
                var url = context.Configuration.GetSection("AuthServer").GetValue<string>("Url");

                options.SaveToken = true;
                options.Audience = "client-id-code";
                options.Authority = url;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience =
                        false, // Audience should be defined on the authorization server or disabled as shown
                    ClockSkew = new TimeSpan(0, 0, 30)
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        // Ensure we always have an error and error description.
                        if (string.IsNullOrEmpty(context.Error))
                        {
                            context.Error = "invalid_token";
                        }

                        if (string.IsNullOrEmpty(context.ErrorDescription))
                        {
                            context.ErrorDescription = "This request requires a valid JWT access token to be provided";
                        }

                        // Add some extra context for expired tokens.
                        if (context.AuthenticateFailure != null && context.AuthenticateFailure.GetType() ==
                            typeof(SecurityTokenExpiredException))
                        {
                            var authenticationException = context.AuthenticateFailure as SecurityTokenExpiredException;
                            context.Response.Headers.Append("x-token-expired",
                                authenticationException?.Expires.ToString("o"));
                            context.ErrorDescription = $"The token expired on {authenticationException?.Expires:o}";
                        }

                        return context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            error = context.Error,
                            error_description = context.ErrorDescription
                        }));
                    }
                };
            });
        
        context.ServiceCollection.AddAuthorization(options =>
        {
            options.AddPolicy(AuthData.AuthenticationSchemes, x =>
            {
                x.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
                x.RequireAuthenticatedUser();
            });
            
            options.AddPolicy(AuthData.SuperAdminArea, builder =>
            {
                builder.RequireAuthenticatedUser();
                builder.RequireRole(UserRoles.SuperAdmin);
            });
        });

        context.ServiceCollection.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        context.ServiceCollection.AddSingleton<IAuthorizationHandler, AppPermissionHandler>();
        
        return base.ConfigureServicesAsync(context);
    }

    public override Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
    {
        var app = context.Parse<WebDefinitionApplicationContext>().WebApplication;
        app.UseRouting();
        //app.UseCors(AppData.PolicyName);
        app.UseCors(c => c.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        app.UseAuthentication();
        app.UseAuthorization();

        //app.UseSession();
        
        // registering UserIdentity helper as singleton
        UserIdentity.Instance.Configure(app.Services.GetService<IHttpContextAccessor>()!);   
        
        return base.ConfigureApplicationAsync(context);
    }

}