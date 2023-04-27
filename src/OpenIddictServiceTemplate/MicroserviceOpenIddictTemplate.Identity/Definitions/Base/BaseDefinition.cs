using MicroserviceOpenIddictTemplate.Identity.Application.Services;
using MicroserviceOpenIddictTemplate.Identity.Definitions.Identity;
using Pepegov.MicroserviceFramerwork.AspNetCore.Definition;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.Base;

public class BaseDefinition : Definition
{
    public override void ConfigureServicesAsync(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddLocalization();
        services.AddResponseCaching();
        services.AddMemoryCache();

        //services.AddHttpContextAccessor();
        services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
        
        services.AddMvcCore()
            .AddApiExplorer();
        services.AddEndpointsApiExplorer();
        
        services.AddRazorPages();
        
        services.AddTransient<IAccountService, AccountService>();
        services.AddTransient<ApplicationUserClaimsPrincipalFactory>();
    }

    public override void ConfigureApplicationAsync(WebApplication app)
    {
        app.UseHttpsRedirection();
        app.MapRazorPages();
        app.MapDefaultControllerRoute();
    }
}