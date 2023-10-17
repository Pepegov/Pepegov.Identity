using MicroserviceOpenIddictTemplate.BL;
using MicroserviceOpenIddictTemplate.BL.Services;
using MicroserviceOpenIddictTemplate.BL.Services.Interfaces;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace MicroserviceOpenIddictTemplate.PL.Api.Definitions.Base;

public class BaseDefinition : ApplicationDefinition
{ 
    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        context.ServiceCollection.AddLocalization();
        context.ServiceCollection.AddResponseCaching();
        context.ServiceCollection.AddMemoryCache();
        //context.ServiceCollection.AddHttpContextAccessor();
        context.ServiceCollection.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

        context.ServiceCollection.AddMvc();
        context.ServiceCollection.AddEndpointsApiExplorer();
        
        context.ServiceCollection.AddRazorPages();
        
        context.ServiceCollection.AddTransient<IAccountService, AccountService>();
        context.ServiceCollection.AddTransient<ApplicationUserClaimsPrincipalFactory>();
        
        return base.ConfigureServicesAsync(context);
    }

    public override Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
    {
        var app = context.Parse<WebDefinitionApplicationContext>().WebApplication;
        
        app.UseHttpsRedirection();
        app.MapRazorPages();
        app.MapDefaultControllerRoute();
        
        return base.ConfigureApplicationAsync(context);
    }
}