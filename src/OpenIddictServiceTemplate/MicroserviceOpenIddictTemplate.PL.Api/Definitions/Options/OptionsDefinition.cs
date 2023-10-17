using MicroserviceOpenIddictTemplate.DAL.Models.Options;
using MicroserviceOpenIddictTemplate.PL.Api.Definitions.Options.Models;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace MicroserviceOpenIddictTemplate.PL.Api.Definitions.Options;

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
