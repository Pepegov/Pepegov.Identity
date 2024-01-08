using MicroserviceOpenIddictTemplate.DAL.Models.Options;
using MicroserviceOpenIddictTemplate.PL.Definitions.Options.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace MicroserviceOpenIddictTemplate.PL.Definitions.Options;

public class OptionsDefinition : ApplicationDefinition
{
    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        var identityConfiguration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("identitysetting.json")
            .Build();
        
        context.ServiceCollection.Configure<IdentityAddressOption>(identityConfiguration.GetSection("IdentityServerUrl"));
        context.ServiceCollection.Configure<IdentityClientOption>(identityConfiguration.GetSection("CurrentIdentityClient"));
        context.ServiceCollection.Configure<AdminProfileOption>(context.Configuration.GetSection("AdminUser"));

        return base.ConfigureServicesAsync(context);
    }
}
