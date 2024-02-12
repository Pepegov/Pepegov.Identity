using Pepegov.Identity.DAL.Database;
using Pepegov.Identity.DAL.Models.Options;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace Pepegov.Identity.PL.Definitions.Database;

public class DatabaseSeedingDefinition : ApplicationDefinition
{
    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        context.ServiceCollection.Configure<List<SeedUserOption>>(context.Configuration.GetSection("SeedUsers"));
        return base.ConfigureServicesAsync(context);
    }

    public override async Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
    {
        using var scope = context.ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>()!;

        await new DatabaseInitializer(context.ServiceProvider, dbContext).Seed();
    }
}
