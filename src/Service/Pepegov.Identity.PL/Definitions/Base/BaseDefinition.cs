using Pepegov.Identity.BL;
using Pepegov.Identity.BL.Services;
using Pepegov.Identity.BL.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Pepegov.Identity.DAL.Domain;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace Pepegov.Identity.PL.Definitions.Base;

public class BaseDefinition : ApplicationDefinition
{ 
    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        context.ServiceCollection.AddLocalization();
        context.ServiceCollection.AddHttpContextAccessor();
        context.ServiceCollection.AddResponseCaching();
        context.ServiceCollection.AddMemoryCache();
        context.ServiceCollection.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

        context.ServiceCollection.AddEndpointsApiExplorer();
        context.ServiceCollection.AddControllersWithViews();
        context.ServiceCollection.AddRazorPages(options =>
        {
            options.Conventions.AuthorizePage("/Connect/SuperAdmin/Login", AuthData.SuperAdminArea);
        });
        
        context.ServiceCollection.AddTransient<IAccountService, AccountService>();
        context.ServiceCollection.AddTransient<ITokenManagementService, TokenManagementService>();
        context.ServiceCollection.AddTransient<ApplicationUserClaimsPrincipalFactory>();
        
        return base.ConfigureServicesAsync(context);
    }

    public override Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
    {
        var app = context.Parse<WebDefinitionApplicationContext>().WebApplication;
        
        app.UseHttpsRedirection();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapDefaultControllerRoute();
            endpoints.MapRazorPages();
        });
        
        return base.ConfigureApplicationAsync(context);
    }
}