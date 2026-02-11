using AegisForge.Infrastructure.Options;
using AegisForge.PL.Jobs;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace AegisForge.PL.Definitions.Database;

public class DatabaseSeedingDefinition : ApplicationDefinition
{
    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        context.ServiceCollection.Configure<List<SeedUserOption>>(context.Configuration.GetSection("SeedUsers"));
        context.ServiceCollection.AddHostedService<DatabaseSeedingWorker>();
        return base.ConfigureServicesAsync(context);
    }
}
