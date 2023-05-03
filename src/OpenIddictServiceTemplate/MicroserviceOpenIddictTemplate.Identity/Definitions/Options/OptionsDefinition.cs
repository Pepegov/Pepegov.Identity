using MicroserviceOpenIddictTemplate.DAL.Models.Options;
using MicroserviceOpenIddictTemplate.Identity.Definitions.Options.Models;
using Pepegov.MicroserviceFramerwork.AspNetCore.Definition;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.Options;

public class OptionsDefinition : Definition
{
    public override void ConfigureServicesAsync(IServiceCollection services, WebApplicationBuilder builder)
    {
        var identityConfiguration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("identitysetting.json")
            .Build();
        
        services.Configure<IdentityAddressOption>(identityConfiguration.GetSection("IdentityServerUrl"));
        services.Configure<IdentityClientOption>(identityConfiguration.GetSection("CurrentIdentityClient"));
        services.Configure<AdminProfileOption>(builder.Configuration.GetSection("AdminUser"));
    }
}
