using Pepegov.Identity.DAL.Models.Options;
using Pepegov.Identity.PL.Definitions.Options.Models;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace Pepegov.Identity.PL.Definitions.Options;

public class OptionsDefinition : ApplicationDefinition
{
    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        var identityConfiguration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("identitysetting.json")
            .Build();
        
        context.ServiceCollection.Configure<IdentityAddressOption>(context.Configuration.GetSection("IdentityServerUrl"));
        context.ServiceCollection.Configure<IdentityClientOption>(identityConfiguration.GetSection("CurrentIdentityClient"));
        context.ServiceCollection.Configure<AdminProfileOption>(context.Configuration.GetSection("AdminUser"));

        return base.ConfigureServicesAsync(context);
    }
}
