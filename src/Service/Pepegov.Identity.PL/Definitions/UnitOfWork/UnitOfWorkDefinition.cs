using Pepegov.Identity.DAL.Database;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework.Configuration;

namespace Pepegov.Identity.PL.Definitions.UnitOfWork
{
    public class UnitOfWorkDefinition : ApplicationDefinition
    {
        public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
        {
            context.ServiceCollection.AddUnitOfWork(context =>
            {
                context.UsingEntityFramework((registrationContext, configurator) =>
                {
                    configurator.DatabaseContext<ApplicationDbContext>();
                });
            });
            
            return base.ConfigureServicesAsync(context);
        }
    }
}
